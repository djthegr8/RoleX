using System.Threading.Tasks;
using Hermes.Modules.Services;
using Discord;
namespace Hermes.Modules.General
{
    [DiscordCommandClass("General", "General commands for all!")]
    public class React : CommandModuleBase
    {
        [DiscordCommand("react", commandHelp = "react <emote>", description = "Reacts with the given emote to replied msg")]
        public async Task RReact(string em, params string[] args)
        {
            var isemoji = new Emoji(em);
            var emote = isemoji != null ? (IEmote)isemoji : (IEmote)(await GetEmote(em));
            var mes = Context.Message.ReferencedMessage;
            if (emote == null || mes == null){
                await ReplyAsync("-_-");
                return;
            }
            await mes.AddReactionAsync(emote);
        }
    }
}