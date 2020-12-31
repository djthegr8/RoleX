using System;
using static RoleX.Modules.SqliteClass;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Public_Bot;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;

namespace RoleX.Modules
{
    [DiscordCommandClass("General", "General commands for all!")]
    public class Ping : CommandModuleBase
    {
        [DiscordCommand("ping",commandHelp ="ping", description ="Finds the latency!")]
        public async Task RPing(params string[] _)
        {
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync($"***RoleX enters the Discord Universe in {Context.Client.Latency} miliseconds***");
        }
    }
}