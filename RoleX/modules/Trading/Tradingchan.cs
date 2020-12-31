using Discord;
using Discord.WebSocket;
using System;
using System.Timers;
using Public_Bot;
using System.Collections.Generic;
using static RoleX.Program;
using System.Text.RegularExpressions;

using System.Linq;
using System.Threading.Tasks;
using static RoleX.Modules.SqliteClass;
namespace RoleX.Modules
{
    [DiscordCommandClass("Trading", "Class with trading related commands")]
    public class Tradingchan : CommandModuleBase
    {
        [DiscordCommand("tradingchan", description ="Set's the guild's trading channel for users to post trades in.", example ="tradingchan #trading-ads", commandHelp ="tradingchan #channel")]
        public async Task TradingChan(params string[] args)
        {
            if (args.Length == 0 || !(Context.User as SocketGuildUser).GuildPermissions.Administrator)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The current trading channel",
                    Description = $"{(await TradingChanGetter(Context.Guild.Id) == 0 ? "No trading channel set" : $"<#{await TradingChanGetter(Context.Guild.Id)}>")}\n",
                    Color = Blurple,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"To change it, do `{await PrefixGetter(Context.Guild.Id)}tradingchan #channel`"
                    }
                }.WithCurrentTimestamp());
                return;
            }
            else
            {
                if (args[0].ToLower() == "remove" || args[0] == "0")
                {
                    await AlertChanAdder(Context.Guild.Id, 0);
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Trading Disabled!",
                        Description = $"The trading channel has now been terminated.",
                        Color = Blurple,
                        Footer = new EmbedFooterBuilder
                        {
                            Text = $"To change it, do `{await PrefixGetter(Context.Guild.Id)}tradingchan #channel`"
                        }
                    });
                    return;
                }
                else if (GetChannel(args[0]) == null)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "What channel?",
                        Description = $"Couldn't parse `{args[0]}` as channel :(",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
                await TradingChanAdder(Context.Guild.Id, GetChannel(args[0]).Id);
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The updated Alert Channel!",
                    Description = $"The alert channel is now <#{await TradingChanGetter(Context.Guild.Id)}>",
                    Color = Blurple,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"To change it yet again, do `{await PrefixGetter(Context.Guild.Id)}tradingchan #channel`"
                    }
                }.WithCurrentTimestamp());
            }
        }
    }
}