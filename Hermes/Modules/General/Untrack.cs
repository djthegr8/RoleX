using System.Threading.Tasks;
using Discord;
using Hermes.Modules.Services;

namespace Hermes.Modules.General
{
    [DiscordCommandClass("General", "General Commands for all!")]
    internal class Untrack : CommandModuleBase
    {
        [DiscordCommand("untrack", commandHelp = "untrack <@user>", description = "Untracks the given user", example = "Untrack @weirdDude")]
        public async Task UntrackCommand(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "Mention the user you wish to untrack",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (await GetUser(args[0]) != null)
            {
                args[0] = (await GetUser(args[0])).Id.ToString();
            }
            if (args[0] == "all")
            {
                await SqliteClass.Track_AllCDRemover(Context.User.Id);
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "All users untracked",
                    Description = "Clear!",
                    Color = Blurple
                }.WithCurrentTimestamp());
                return;
            }
            if (
                ulong.TryParse(args[0], out ulong userID) &&
                Program.Client.GetUser(userID) != null
                )
            {
                var user = Program.Client.GetUser(userID);
                await SqliteClass.Track_CDRemover(Context.User.Id, user.Id);
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "User untracked",
                    Description = "If you were tracking that user, you are now not!\nIf you don't remember who you were tracking run `untrack all` to remove all.",
                    Color = Blurple
                }.WithCurrentTimestamp());
            }
            else
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "Mention the user you wish to untrack",
                    Color = Color.Red
                }.WithCurrentTimestamp());
            }
        }
    }
}
