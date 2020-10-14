using static Public_Bot.CustomCommandService;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using RoleX.modules;
using Newtonsoft.Json;
using System.IO;
using Public_Bot;
using GuildPermissions = Public_Bot.GuildPermissions;


namespace RoleX.modules
{
    [DiscordCommandClass("Role Editor","Class for editing of Roles")]
    public class RoleEditor: CommandModuleBase
    {
        [GuildPermissions(GuildPermission.ManageRoles)]
        [DiscordCommand("color",description ="Changes the color of role", commandHelp ="color <@role> <hex/None>", example ="color @LightPurple #bb86fc")]
        public async Task ChangeRole(params string[] args)
        {
            if (args.Length < 2)
            {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters",
                        Description = $"The way to use the command is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}color <@role> <color>`",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                return;
            }
            var role = GetRole(args[0]);
            if (role == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "That role is invalid",
                    Description = $"I couldn't parse `{args[0]}` as a role!",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            if (!(Context.User as SocketGuildUser).Roles.Any(rl => rl.Position > role.Position) && Context.Guild.OwnerId != Context.User.Id)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Oops!",
                    Description = "You're below the role you want to edit!",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            if (args[1].ToLower() == "none" || args[1].ToLower() == "invisible")
            {
                await role.ModifyAsync(x => x.Color = new Color());
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Done!!",
                    Description = $"The role {role.Name}'s color is removed!",
                    Color = Blurple
                }.WithCurrentTimestamp().Build());
                return;
            }
            System.Drawing.ColorConverter c = new System.Drawing.ColorConverter();
            System.Drawing.Color col = new System.Drawing.Color();
            bool hasC = false;
            var hArgs1 = args[1][0] != '#' ? $"#{args[1]}" : args[1];
            if (Regex.IsMatch(hArgs1, "^(#[0-9A-Fa-f]{3})$|^(#[0-9A-Fa-f]{6})$"))
            {

                col = (System.Drawing.Color)c.ConvertFromString(hArgs1);
                hasC = true;
            }
            else
            {
                System.ComponentModel.TypeConverter.StandardValuesCollection svc = (System.Drawing.ColorConverter.StandardValuesCollection)c.GetStandardValues();
                foreach (System.Drawing.Color o in svc)
                {
                    if (o.Name.Equals(args[1], StringComparison.OrdinalIgnoreCase))
                    {
                        col = (System.Drawing.Color)c.ConvertFromString(args[1]);
                        hasC = true;
                    }
                }
            }
            if (hasC == false)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What color??",
                    Description = $"Couldn't parse `{args[1]}` as a color!",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            await role.ModifyAsync(x => x.Color = new Color(col.R,col.G,col.B));
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Done!!",
                Description = $"The role {role.Name} is now set to the color of this embed!",
                Color = new Color(col.R, col.G, col.B) == new Color(255,255,255) ? new Color(254,254,254) : new Color(col.R, col.G, col.B)
            }.WithCurrentTimestamp().Build());
            return;
        }
        [Alt("dup")]
        [GuildPermissions(GuildPermission.ManageRoles)]
        [DiscordCommand("duplicate",commandHelp ="duplicate <@role-to-be-duplicated> <@role-to-be-placed-above>",description ="Duplicates a role and places it above the given second role", example ="duplicate @Admin @Moderator")]
        public async Task CreateRole(params string[] args)
        {
            switch (args.Length)
            {
                case 0 or 1:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters",
                        Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}duplicate <@role-to-be-duplicated> <@role-to-be-placed-above>`",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
            }
            var rlD = GetRole(args[0]);
            var rlA = GetRole(args[1]);
            if (rlD == null || rlA == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Couldn't find the role",
                    Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}duplicate <@role-to-be-duplicated> <@role-to-be-placed-above>`",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            if (((Context.User as SocketGuildUser).Roles.Max().Position <= rlA.Position || (Context.User as SocketGuildUser).Roles.Max().Position <= rlD.Position) && Context.Guild.OwnerId != Context.User.Id)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Oops!",
                    Description = "You're below the roles you want to duplicate and place!",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            var newlyMadeRole = await Context.Guild.CreateRoleAsync(rlD.Name + "~ 1", rlD.Permissions, rlD.Color, rlD.IsHoisted, rlD.IsMentionable);
            await Context.Guild.ReorderRolesAsync(new List<ReorderRoleProperties>() { new ReorderRoleProperties(newlyMadeRole.Id, rlA.Position) });
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Role Duplicated Successfully",
                Description = $"{newlyMadeRole.Mention} was created from {rlD.Mention} and placed above {rlA.Mention}!",
                Color = Blurple
            }.WithCurrentTimestamp().Build());
            return;
        }
        [GuildPermissions(GuildPermission.ManageRoles)]
        [Alt("del")]
        [DiscordCommand("delete",commandHelp ="delete <@role/id>", description ="Deletes the mentioned role",example ="delete @DumbRole")]
        public async Task DelRole(params string[] args)
        {
            SocketRole DeleteRole;
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What role?",
                    Description = "Mention the role you wish to delete",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            DeleteRole = GetRole(args[0]);
            if (DeleteRole == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What role?",
                    Description = "Mention the role you wish to delete",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            if (Context.Guild.CurrentUser.Roles.All(idk => idk.CompareTo(DeleteRole) < 0))
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Hey, thats above me",
                    Description = $"The bot's highest role => {Context.Guild.CurrentUser.Roles.Max().Name}\nThe role you wish to delete => {DeleteRole.Name}",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            if (!(Context.User as SocketGuildUser).Roles.Any(rl => rl.Position > DeleteRole.Position) && Context.Guild.OwnerId != Context.User.Id)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Oops!",
                    Description = "You're below the role you want to delete!",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            else
            {
                var nm = DeleteRole.Name;
                await DeleteRole.DeleteAsync();
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = $"Role deleted successfully!",
                    Description = $"The role `{nm}` was successfully deleted",
                    Color = Blurple
                }.WithCurrentTimestamp().Build());
            }
        }

        [Alt("addpermission")]
        [GuildPermissions(GuildPermission.ManageRoles)]
        [DiscordCommand("addperms",commandHelp ="addperms <@role/id> <Permission>",description ="Adds the given permission to the requested role")]
        public async Task AddPerms(params string[] args)
        {
            switch (args.Length)
            {
                case 0 or 1:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "U need to give the Role and Permission",
                        Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}addperms <@role/id> <Permission>`",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
            }
            var roleA = GetRole(args[0]);
            if (roleA == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "That role is invalid",
                    Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}addperms <@role/id> <Permission>`",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            if (Context.Guild.CurrentUser.Roles.All(idk => idk.CompareTo(roleA) < 0))
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Hey, thats above me",
                    Description = $"The bot's highest role => {Context.Guild.CurrentUser.Roles.Max().Name}\nThe role you wish to delete => {roleA.Name}",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            if (!(Context.User as SocketGuildUser).Roles.Any(rl => rl.Position > roleA.Position) && Context.Guild.OwnerId != Context.User.Id)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Oops!",
                    Description = "You're below the role you want to edit!",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            var gp = GetPermission(args[1]);
            if (gp.Item2 == false)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "That permission is invalid",
                    Description = $"The list of permissions is ~ ```{string.Join('\n', Enum.GetNames(typeof(GuildPermission)))}```",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            await roleA.ModifyAsync(rl => rl.Permissions = EditPerm(roleA, gp.Item1, true));
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = $"Permission added to Role!",
                Description = $"`{roleA.Name}` now has the permission `{args[1]}`",
                Color = Blurple
            }.WithCurrentTimestamp().Build());
            return;
        }
        [Alt("removepermission")]
        [GuildPermissions(GuildPermission.ManageRoles)]
        [DiscordCommand("removeperms", commandHelp = "removeperms <@role/id> <Permission>", description = "Remove the given permission from the requested role")]
        public async Task RemovePerms(params string[] args)
        {
            switch (args.Length)
            {
                case 0 or 1:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters",
                        Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}removeperms <@role/id> <Permission>`",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
            }
            var roleA = GetRole(args[0]);
            if (roleA == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "That role is invalid",
                    Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}removeperms <@role/id> <Permission>`",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            if (Context.Guild.CurrentUser.Roles.All(idk => idk.CompareTo(roleA) < 0))
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Hey, thats above me",
                    Description = $"The bot's highest role => {Context.Guild.CurrentUser.Roles.Max().Name}\nThe role you wish to delete => {roleA.Name}",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            if (!(Context.User as SocketGuildUser).Roles.Any(rl => rl.Position > roleA.Position) && Context.Guild.OwnerId != Context.User.Id)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Oops!",
                    Description = "You're below the role you want to edit!",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            var gp = GetPermission(args[1]);
            if (gp.Item2 == false)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "That permission is invalid",
                    Description = $"The list of permissions is ~ ```{string.Join('\n', Enum.GetNames(typeof(GuildPermission)))}```",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            await roleA.ModifyAsync(rl => rl.Permissions = EditPerm(roleA, gp.Item1, false));
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = $"Permission removed From Role!",
                Description = $"Permission `{args[1]}` revoked from `{roleA.Name.ToUpper()}`",
                Color = Blurple
            }.WithCurrentTimestamp().Build());
            return;
        }
        [GuildPermissions(GuildPermission.ManageRoles)]
        [DiscordCommand("add", commandHelp = "add <@user> <@role>", description = "Adds the role to the given user", example = "add @DJ001 @Criminal")]
        public async Task Additive(params string[] args)
        {
            switch (args.Length)
            {
                case 0 or 1:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters",
                        Description = $"The way to use the command is {await SqliteClass.PrefixGetter(Context.Guild.Id)}add <@user> <@role>",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
            }
            SocketUser uzi;
            SocketRole role;
            if (Context.Message.MentionedRoles.Any() && Context.Message.MentionedUsers.Any())
            {
                role = Context.Message.MentionedRoles.First();
                uzi = Context.Message.MentionedUsers.First();
            }
            else if (Context.Message.MentionedUsers.Any())
            {
                uzi = Context.Message.MentionedUsers.First();
                short aa;
                if (args[0].Contains(uzi.Id.ToString()))
                {
                    aa = 1;
                }
                else
                {
                    aa = 0;
                }
                role = GetRole(args[aa]);
                if (role == null)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "What role!?",
                        Description = $"We couldn't parse `{aa}` as role!?",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
                }
            } else if (Context.Message.MentionedRoles.Any())
            {
                role = Context.Message.MentionedRoles.First();
                short aa;
                if (args[0].Contains(role.Id.ToString()))
                {
                    aa = 1;
                }
                else
                {
                    aa = 0;
                }
                uzi = GetUser(args[aa]);
                if (uzi == null)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "What user!?",
                        Description = $"We couldn't parse `{aa}` as user!?",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
                }
            } else
            {
                if (GetUser(args[0]) != null && GetRole(args[0]) == null)
                {
                    uzi = GetUser(args[0]);
                    role = GetRole(args[1]);
                    if (role == null)
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "What role!?",
                            Description = $"We couldn't parse `{args[1]}` as role!?",
                            Color = Color.Red
                        }.WithCurrentTimestamp().Build());
                        return; 
                    }
                }
                else if (GetRole(args[0]) != null && GetUser(args[0]) == null)
                {
                    role = GetRole(args[0]);
                    uzi = GetUser(args[1]);
                    if (uzi == null)
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "What user!?",
                            Description = $"We couldn't parse `{args[1]}` as user!?",
                            Color = Color.Red
                        }.WithCurrentTimestamp().Build());
                        return;
                    }
                } else if ((GetRole(args[0]) != null && GetUser(args[0]) != null) || (GetRole(args[1]) != null && GetUser(args[1]) != null))
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Multiple Possibilities Detected",
                        Description = $"Given {(GetUser(args[0]) == null? args[1] : args[0])}\n**Role Found:**\n{(GetRole(args[0]) == null ? GetRole(args[1]).Mention : GetRole(args[0]).Mention)}\n**User Found**\n{(GetUser(args[0]) == null ? GetUser(args[1]).Mention : GetUser(args[0]).Mention)}\nPlease use a mention instead of a search query!",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
                }
                else
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "What user and role!?",
                        Description = $"We couldn't parse either!?",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
                }
                if (role.Position > (Context.User as SocketGuildUser).Roles.Max().Position)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Could not add role",
                        Description = $"Your highest role, **{(Context.User as SocketGuildUser).Roles.Max().Name}** is below the role you wish to give, **{role.Name}**",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
                }
                if (Context.Guild.CurrentUser.Roles.All(idk => idk.CompareTo(role) < 0))
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Specified role is above me!",
                        Description = $"The bot's highest role => {Context.Guild.CurrentUser.Roles.Max().Name}\nThe role you wish to add => {role.Name}",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
                }
                await (uzi as SocketGuildUser).AddRoleAsync(role);
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Added {role} to {uzi}!",
                    Description = $"Role addition successful!",
                    Color = Blurple
                }.WithCurrentTimestamp().Build());
                return;
            }
            
        }
    }
}
