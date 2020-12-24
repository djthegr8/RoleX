using System;
using static RoleX.Modules.SqliteClass;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Public_Bot;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using GuildPermissions = Public_Bot.GuildPermissions;
namespace RoleX.Modules
{
    [DiscordCommandClass("General", "General commands for all!")]
    public class Humans : CommandModuleBase
    {
        [DiscordCommand("humans", description = "Shows number of users in server", commandHelp = "humans")]
        public async Task hmans()
        {
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = $"There are {Context.Guild.MemberCount} users in {Context.Guild.Name}!",
                Description = $"Wow nice server guys!",
                Color = Blurple,
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Hehe!"
                }
            }.WithCurrentTimestamp());
            return;
        }
    }
}