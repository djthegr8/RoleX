using System.Threading.Tasks;
using Hermes.Modules.Services;

namespace Hermes.Modules.General
{
    [DiscordCommandClass("General", "General commands for all!")]
    public class Ping : CommandModuleBase
    {
        [DiscordCommand("ping", commandHelp = "ping", description = "Fetches Hermes's latency!")]
        public async Task RPing(params string[] _)
        {
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync(
                $"***Hermes enters the #{Context.Client.ShardId} Shard of Discord Universe in {Context.Client.Latency} miliseconds***");
        }
    }
}