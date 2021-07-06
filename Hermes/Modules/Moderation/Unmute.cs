using System.Threading.Tasks;
using Discord;
using Hermes.Modules.Services;
using static Hermes.Modules.Services.SqliteClass;

namespace Hermes.Modules.Moderation
{
    [DiscordCommandClass("Moderation", "Basic Moderation for yer server!")]
    public class Unmute : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageRoles)]
        [DiscordCommand("unmute", commandHelp = "unmute <@user>", example = "unmute @RegretfulMan",
            description = "Unmutes given user")]
        public async Task RUnmute(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "Mention the user you wish to unmute",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            if (await GetUser(args[0]) == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = $"Couldn't parse `{args[0]}` as user",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            try
            {
                await (await GetUser(args[0])).RemoveRoleAsync(
                    Context.Guild.GetRole(await MutedRoleIdGetter(Context.Guild.Id)));
            }
            catch
            {
            }

            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "User unmuted successfully!",
                Description = $"{await GetUser(args[0])} was successfully unmuted",
                Color = Blurple
            }.WithCurrentTimestamp());
        }
    }
}