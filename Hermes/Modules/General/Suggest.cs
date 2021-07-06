using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hermes.Modules.Services;

namespace Hermes.Modules.General
{
    [DiscordCommandClass("General", "General commands for all")]
    internal class Suggest : CommandModuleBase
    {
        [DiscordCommand("botsuggest", commandHelp = "botsuggest <suggestions-for-Hermes-to-improve>",
            description = "Tells the devs about a suggestion", example = "botsuggest new weird command")]
        public async Task SuggestCommand(params string[] args)
        {
            if (args.Length == 0)
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Insufficient Arguments",
                    Description =
                        $"The way to use the command is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}botsuggest <suggestions-for-Hermes-to-improve>",
                    Color = Color.Red
                }.WithCurrentTimestamp());

            var join = string.Join(' ', args);
            await (Program.Client.GetChannel(755640198662062220) as SocketTextChannel).SendMessageAsync("", false,
                new EmbedBuilder
                    {
                        Title = $"New suggestion from {Context.User} in {Context.Guild.Name}!",
                        Description = $"Here it goes:\n```\n{join}```",
                        Color = Blurple
                    }.AddField($"By {Context.User}",
                        $"**ID**: {Context.User.Id}\n**Mention**: {Context.User.Mention}\n**Server ID**: {Context.Guild.Id}")
                    .WithCurrentTimestamp().Build());
            await ReplyAsync("Your suggestion has been noted!");
        }
    }
}