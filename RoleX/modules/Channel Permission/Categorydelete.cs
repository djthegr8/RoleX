using Discord;
using Discord.WebSocket;
using Public_Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuildPermissions = Public_Bot.GuildPermissions;
namespace RoleX.Modules{
    [DiscordCommandClass("Channel Editor", "Edit Channel-wise perms of a Role using these commands!")]
    public class Categorydelete : CommandModuleBase
    {
        [DiscordCommand("categorydelete", commandHelp = "categorydelete <category-name>", description = "Deletes given category and all its channels", example = "categorydelete Useless")]
        public async Task CatDel(string aa)
        {
            var alf = GetCategory(aa);
            if (alf == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid category",
                    Description = $"`{aa}` could not be parsed as category!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            foreach (var ch in alf.Channels)
            {
                await ch.DeleteAsync();
            }
            await alf.DeleteAsync();
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Delete successful!",
                Description = $"Your category was deleted along with all its channels",
                Color = Blurple
            }.WithCurrentTimestamp());
            return;
        }
    }
}