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
    public class Overwrites : CommandModuleBase
    {
        [DiscordCommand("overwrites", commandHelp = "overwrites <#channel>", description = "Shows the Channel-wise overwrites", example = "overwrites #channel")]
        public async Task Os(params string[] args)
        {
            /*switch (args.Length)
            {
                case 0 or 1:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters!",
                        Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}overwrites <#channel> <@role/@member>`",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
            }*/
            if (args.Length == 0)
            {
                var Embed = new EmbedBuilder
                {
                    Title = $"Overwrites for <#{Context.Channel.Id}>",
                    Color = Blurple,

                };
            }
            else
            {
                var channe = Context.Channel as SocketGuildChannel;
                var channez = GetChannel(args[0]);
                if (channez != null) channe = channez;
                var pos = channe.PermissionOverwrites;
                string rpos = "```";
                foreach (var ov in pos.Where(x => x.TargetType == PermissionTarget.Role))
                {
                    rpos += $"<@&{ov.TargetId}>\n";
                    rpos += ov.Permissions.ToAllowList().Count > 0 ? "✅ " : "" + string.Join("\n✅", ov.Permissions.ToAllowList()) + "\n" + (ov.Permissions.ToDenyList().Count > 0 ? "❌ " : "") + string.Join("\n❌ ", ov.Permissions.ToDenyList());
                }
                rpos += "```";
                string upos = "```";
                foreach (var ov in pos.Where(x => x.TargetType == PermissionTarget.User))
                {
                    upos += $"<@{ov.TargetId}>\n";
                    upos += ov.Permissions.ToAllowList().Count > 0 ? "✅ " : "" + string.Join("\n✅", ov.Permissions.ToAllowList()) + "\n" + (ov.Permissions.ToDenyList().Count > 0 ? "❌ " : "") + string.Join("\n❌ ", ov.Permissions.ToDenyList());
                }
                upos += "```";
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Permission Overwrites",
                    Color = Blurple
                }.AddField("Channel", $"<#{channe.Id}>")
                .AddField("Role Overwrites", rpos)
                .AddField("User Overwrites", upos)
                .WithCurrentTimestamp());
            }
        }
    }
}