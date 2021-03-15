using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using RoleX.Modules.Services;

namespace RoleX.Modules.Role_Editor
{
    [DiscordCommandClass("Role Editor", "Class for editing of Roles!")]
    public class RRename : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageRoles)]
        [Alt("rrn")]
        [DiscordCommand("rrename", commandHelp = "rrename @Role <name>", example = "rrename @WeirdRole Weirder Role", description = "Renames the given role, that should've been obvious ngl")]
        public async Task RRe(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid Parameters",
                    Description = "You need to provide a role, I can't read your mind (yet)",
                    Color = Color.Red
                }.WithCurrentTimestamp()


                );
                return;
            }
            var x = GetRole(args[0]);
            if (x == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid Role",
                    Description = $"Couldn't parse `{args[0]}` as role",
                    Color = Color.Red
                }.WithCurrentTimestamp()

                );
                return;
            }
            if ((Context.User as SocketGuildUser).Roles.Max().Position <= x.Position && Context.Guild.OwnerId != Context.User.Id && Context.User.Id != 701029647760097361 && Context.User.Id != 615873008959225856)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Oops!",
                    Description = "You're below the role you want to edit!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (x.Id == Context.Guild.EveryoneRole.Id)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Oops!",
                    Description = "Can't do that to the everyone role...",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            var name = string.Join(' ', args.Skip(1));
            await x.ModifyAsync(xdot => xdot.Name = name);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Set.",
                Description = $"The role has now been renamed to {x.Mention}",
                Color = Blurple
            }.WithCurrentTimestamp());
        }
    }
}