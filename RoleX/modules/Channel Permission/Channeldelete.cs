using Discord;
using Discord.WebSocket;
using Public_Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuildPermissions = Public_Bot.GuildPermissions;
namespace RoleX.Modules
{
    [DiscordCommandClass("Channel Editor", "Edit Channel-wise perms of a Role using these commands!")]
    public class Channeldelete : CommandModuleBase
    {
        [DiscordCommand("channeldelete", description = "Deletes given channel", example = "channeldelete #WeirdChan", commandHelp = "channeldelete <#channel>")]
        public async Task Cdel(string ags)
        {
            var aaa = GetChannel(ags);
            if (aaa == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid channel",
                    Description = $"`{aaa}` could not be parsed as channel!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            else
            {
                await aaa.DeleteAsync();
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Deleted Channel Successfully",
                    Description = $"Channel `#{aaa.Name}` was deleted!",
                    Color = Blurple
                }.WithCurrentTimestamp()
                );
            }
        }
    }
}