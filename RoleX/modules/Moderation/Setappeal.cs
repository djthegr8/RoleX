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
    public class Setappeal : CommandModuleBase
    {
        [DiscordCommand("setappeal", commandHelp = "setappeal <link>", example = "setappeal https://gforms.com/bah", description = "Sets the appeal link sent to punished members")]
        public async Task R_Setappeal(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The current appeal link",
                    Description = $"`{(await AppealGetter(Context.Guild.Id) == "" ? "No appeal set" : await AppealGetter(Context.Guild.Id))}`\n",
                    Color = Blurple,
                    Url = (await AppealGetter(Context.Guild.Id) == "" ? null : await AppealGetter(Context.Guild.Id)),
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"To change it, do `{await PrefixGetter(Context.Guild.Id)}setappeal <link>`, and do `{await PrefixGetter(Context.Guild.Id)}setappeal remove` to remove it"
                    }
                }.WithCurrentTimestamp());
                return;
            }
            else
            {
                if (!Uri.TryCreate(args[0], UriKind.RelativeOrAbsolute, out Uri? _))
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Invalid appeal link!",
                        Description = $"Couldn't parse `{args[0]}` as an URL :sob:",
                        Color = Color.Red,
                        Footer = new EmbedFooterBuilder
                        {
                            Text = $"To change it, do `{await PrefixGetter(Context.Guild.Id)}setappeal <link>`, and do `{await PrefixGetter(Context.Guild.Id)}setappeal remove` to remove it"
                        }
                    }.WithCurrentTimestamp());
                    return;
                }
                if (args[0].ToLower() == "remove")
                {
                    args[0] = "";
                }
                await AppealAdder(Context.Guild.Id, args[0]);
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The updated appeal link",
                    Description = $"`{(await AppealGetter(Context.Guild.Id) == "" ? "Appeal Removed!" : await AppealGetter(Context.Guild.Id))}`\n",
                    Color = Blurple,
                    Url = (await AppealGetter(Context.Guild.Id) == "" ? null : await AppealGetter(Context.Guild.Id)),
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"To change it yet again, do `{await PrefixGetter(Context.Guild.Id)}setappeal <link>`"
                    }
                }.WithCurrentTimestamp());
            }
        }
    }
}