using System.Threading.Tasks;
using Hermes.Modules.Services;

namespace Hermes.Modules.General
{
    [DiscordCommandClass("General", "General commands for all!")]
    public class React : CommandModuleBase
    {
        [DiscordCommand("react", commandHelp = "react <emote>", description = "Reacts with the given emote to replied msg")]
        public async Task RReact(string em, params string[] args)
        {
            emote = await GetEmote(em)
            mes = Context.Message.ReferencedMessage
            if (emote == null || mes == null){
                await ReplyAsync("-_-")
                return
            }
            await mes.AddReactionAsync(emote);
        }
    }
}