using System.Threading.Tasks;
using RoleX.Modules.Services;

namespace RoleX.Modules.General
{
    [DiscordCommandClass("General", "General commands for all!")]
    public class Ping : CommandModuleBase
    {
        [DiscordCommand("ping",commandHelp ="ping", description ="Fetches RoleX's latency!")]
        public async Task RPing(params string[] _)
        {
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync($"***RoleX enters the #{Context.Client.ShardId} Shard of Discord Universe in {Context.Client.Latency} miliseconds***");
        }
    }
}