using Discord;
using Discord.WebSocket;
using Public_Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RoleX.Modules
{
    [DiscordCommandClass("Role Editor", "Class for editing of Roles!")]
    public class Duplicate : CommandModuleBase
    {
        [Alt("dup")]
        [RequiredUserPermissions(new[] { GuildPermission.ManageRoles, GuildPermission.ManageGuild})]
        [DiscordCommand("duplicate", commandHelp = "duplicate <@role-to-be-duplicated> <@role-to-be-placed-above>", description = "Duplicates a role and places it above the given second role", example = "duplicate @Admin @Moderator")]
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
                    }.WithCurrentTimestamp());
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
                }.WithCurrentTimestamp());
                return;
            }
            if (((Context.User as SocketGuildUser).Roles.Max().Position <= rlA.Position || (Context.User as SocketGuildUser).Roles.Max().Position <= rlD.Position) && Context.Guild.OwnerId != Context.User.Id && Context.User.Id != 701029647760097361 && Context.User.Id != 615873008959225856)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Oops!",
                    Description = "You're below the roles you want to duplicate and place!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var newlyMadeRole = await Context.Guild.CreateRoleAsync(rlD.Name + "~1", rlD.Permissions, rlD.Color, rlD.IsHoisted, rlD.IsMentionable);
            await Context.Guild.ReorderRolesAsync(new List<ReorderRoleProperties>() { new ReorderRoleProperties(newlyMadeRole.Id, rlA.Position) });
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Role Duplicated Successfully",
                Description = $"{newlyMadeRole.Mention} was created from {rlD.Mention} and placed above {rlA.Mention}!",
                Color = Blurple
            }.WithCurrentTimestamp());
            return;
        }
    }
}