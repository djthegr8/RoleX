using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Public_Bot;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using static RoleX.Modules.SqliteClass;
using GuildPermissions = Public_Bot.GuildPermissions;
namespace RoleX.Modules
{
    [DiscordCommandClass("Moderation", "Basic Moderation for yer server!")]
    public class Bans : CommandModuleBase
    {
        [DiscordCommand("bans", commandHelp = "bans", description = "Shows the bans in the server")]
        public async Task RBans(params string[] _)
        {
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "List of bans in the server",
                Description = string.Join("\n\n", (await Context.Guild.GetBansAsync()).Select((r5, idx) => $"**{idx + 1}) User**: <@{r5.User.Id}>\n**Reason**: {(r5.Reason ?? "Not given")}")),
                Color = Blurple
            }.WithCurrentTimestamp()
            );
        }
    }
}