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
    public class Rawperms : CommandModuleBase
    {
        [DiscordCommand("rawperms",description ="Takes a permission integer and gives the values", example ="rawperms 8")]
        public async Task PermRaw(ulong raw)
        {
            var gp = new Discord.GuildPermissions(raw);
            string x = "";
            x += $"Admin:        {(gp.Administrator ? "✅" : "❌")}\n";
            x += $"Kick:         {(gp.KickMembers ? "✅" : "❌")}\n";
            x += $"Ban:          {(gp.BanMembers ? "✅" : "❌")}\n";
            x += $"Mention:      {(gp.MentionEveryone ? "✅" : "❌")}\n";
            x += $"Manage Guild: {(gp.ManageGuild ? "✅" : "❌")}\n";
            x += $"Messages:     {(gp.ManageMessages ? "✅" : "❌")}\n";
            x += $"Channels:     {(gp.ManageChannels ? "✅" : "❌")}\n";
            x += $"Roles:        {(gp.ManageRoles ? "✅" : "❌")}\n";
            x += $"Webhooks:     {(gp.ManageWebhooks ? "✅" : "❌")}\n";
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Decoding Permission values",
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Description = $"Below is what {raw} means in Discord API language ~",
                Fields = {new EmbedFieldBuilder()
                {
                    Name = "Permissions",
                    Value = $"```{x}```"
                } },
                Color = Blurple,
                Footer = new EmbedFooterBuilder
                {
                    Text = "Useful command, isn't it?"
                }
            }.WithCurrentTimestamp()
            );
        }
    }
}