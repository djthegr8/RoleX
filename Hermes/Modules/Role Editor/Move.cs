using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hermes.Modules.Services;

namespace Hermes.Modules.Role_Editor
{
    [DiscordCommandClass("Role Editor", "Class for editing of Roles!")]
    public class Move : CommandModuleBase
    {
        [Alt("moverole")]
        [RequiredUserPermissions(GuildPermission.ManageRoles, GuildPermission.ManageGuild)]
        [DiscordCommand("move", commandHelp = "move <@role-to-be-moved> <@role-to-be-placed-below>",
            description = "Moves a role below the given second role", example = "move @Moderator @Admin")]
        public async Task CreateRole(params string[] args)
        {
            switch (args.Length)
            {
                case 0 or 1:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters",
                        Description =
                            $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}move <@role-to-be-moved> <@role-to-be-placed-below>`",
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
                    Description =
                        $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}move <@role-to-be-moved> <@role-to-be-placed-below>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            if (((Context.User as SocketGuildUser).Roles.Max().Position <= rlA.Position ||
                 (Context.User as SocketGuildUser).Roles.Max().Position <= rlD.Position) &&
                Context.Guild.OwnerId != Context.User.Id && devids.All(k => k != Context.User.Id))
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Oops!",
                    Description = "You're below the roles you want to move!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Role Moved Successfully",
                Description = $"{rlD.Mention} was placed below {rlA.Mention}!",
                Color = Blurple
            }.WithCurrentTimestamp());
            await Context.Guild.ReorderRolesAsync(new List<ReorderRoleProperties>
            {
                new(rlD.Id, rlA.Position)
            });
        }
    }
}