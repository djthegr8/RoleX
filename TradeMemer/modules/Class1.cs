using static Public_Bot.CustomCommandService;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using TradeMemer.modules;
using Newtonsoft.Json;
using System.IO;
using Public_Bot;
using GuildPermissions = Public_Bot.GuildPermissions;
using System.Security.Cryptography.X509Certificates;

namespace TradeMemer.modules
{
    [DiscordCommandClass("Role Editor","Class for editing of Roles")]
    public class RoleEditor: CommandModuleBase
    {
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
                        Title = "U need to give the Role to be duplicated, and to be placed above",
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
                    Title = "Not gonna happen kid",
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
                    Title = "Not gonna happen kid",
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
            if (!(Context.User as SocketGuildUser).Roles.Any(rl => rl.Position > roleA.Position) && Context.Guild.OwnerId != Context.User.Id)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Not gonna happen kid",
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
                        Title = "U need to give the Role and Permission",
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
            if (!(Context.User as SocketGuildUser).Roles.Any(rl => rl.Position > roleA.Position) && Context.Guild.OwnerId != Context.User.Id)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Not gonna happen kid",
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
    }
}