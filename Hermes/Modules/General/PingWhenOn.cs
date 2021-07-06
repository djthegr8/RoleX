using System.Linq;
using System.Threading.Tasks;
using Discord;
using Hermes.Modules.Services;

namespace Hermes.Modules.General
{
    [DiscordCommandClass("General", "General Commands for all!")]
    internal class OnlineTrack : CommandModuleBase
    {
        [DiscordCommand("track", commandHelp = "track <@User>",
            description = "DMs you next time that user gets online or DND!", example = "track @weirdDude")]
        public async Task Track(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "Mention the user you wish to track",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            if (await GetUser(args[0]) != null) args[0] = (await GetUser(args[0])).Id.ToString();
            if (args[0].ToLower() == "list")
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = $"List of users tracked by `{Context.User}`",
                    Description = $"You`re tracking ~\n<@{await SqliteClass.TrackCdGetUser(Context.User.Id)}>",
                    Color = Blurple
                }.WithCurrentTimestamp());
                return;
            }

            if (await SqliteClass.TrackCooldownGetter(Context.User.Id) && devids.All(k => k != Context.User.Id))
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "You're on tracking cooldown!",
                    Description =
                        $"You can only track one person at a time right now, and you're tracking <@{await SqliteClass.TrackCdGetUser(Context.User.Id)}>.\n Stay tuned for Hermes Premium for more!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            if (
                ulong.TryParse(args[0], out var userID) &&
                Program.Client.GetUser(userID) != null
            )
            {
                var user = Program.Client.GetUser(userID);
                if (user.Status != UserStatus.Offline)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "That user is already online :(",
                        Description = $"Go chat with <@{user.Id}> now",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }

                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = $"Tracking {user} now...",
                    Description = "I'll DM you when they're online or DND",
                    Color = Blurple
                }.WithCurrentTimestamp());
                await SqliteClass.Track_CDAdder(Context.User.Id, user.Id);
            }
            else
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Uhhh",
                    Description = "Either that user doesn't exist, or isn't known to Hermes",
                    Color = Color.Red
                }.WithCurrentTimestamp());
            }
        }
    }
}