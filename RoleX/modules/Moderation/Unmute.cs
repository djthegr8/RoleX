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
    public class Unmute : CommandModuleBase
    {
        [DiscordCommand("unmute", commandHelp = "unmute <@user>", example = "unmute @RegretfulMan", description = "Unmutes given user")]
        public async Task RUnmute(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "Mention the user you wish to unmute",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            else if (await GetUser(args[0]) == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = $"Couldn't parse `{args[0]}` as user",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            else
            {
                try
                {
                    await (await GetUser(args[0])).RemoveRoleAsync(Context.Guild.GetRole(await MutedRoleIDGetter(Context.Guild.Id)));
                }
                catch { }
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "User unmuted successfully!",
                    Description = $"{await GetUser(args[0])} was successfully unmuted :)",
                    Color = Blurple
                }.WithCurrentTimestamp());
                return;
            }
        }
    }
}