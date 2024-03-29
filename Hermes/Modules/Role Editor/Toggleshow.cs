using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hermes.Modules.Services;

namespace Hermes.Modules.Role_Editor
{
    [DiscordCommandClass("Role Editor", "Class for editing of Roles!")]
    public class Toggleshow : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageRoles)]
        [Alt("ts")]
        [Alt("hoist")]
        [DiscordCommand("toggleshow", commandHelp = "toggleshow @Role", example = "toggleshow @role",
            description = "Hoists specified role", IsPremium = true)]
        public async Task ShowRole(params string[] args)
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

            if ((Context.User as SocketGuildUser).Roles.Max().Position <= x.Position &&
                Context.Guild.OwnerId != Context.User.Id && devids.All(k => k != Context.User.Id))
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
                    Description = "Can't do that to `@everyone`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            await x.ModifyAsync(xdot => xdot.Hoist = !x.IsHoisted);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Set.",
                Description = $"The role {x.Name} will now {(x.IsHoisted ? "" : "not ")} be hoisted",
                Color = Blurple
            }.WithCurrentTimestamp());
        }
    }
}