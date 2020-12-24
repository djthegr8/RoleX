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
    public class Alttime : CommandModuleBase
    {
        [DiscordCommand("alttime", commandHelp ="alttime num_months", description ="Sets the number of months for flagging as alt", example ="alttime 4")]
        public async Task Alttimes(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The current alt flagging timespan",
                    Description = $"We will flag an account as an alt if it's {await AltTimePeriodGetter(Context.Guild.Id)} months or younger on Discord.",
                    Color = Blurple,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"To change it, do `{await PrefixGetter(Context.Guild.Id)}alttime num_months`"
                    }
                }.WithCurrentTimestamp());
                return;
            }
            else
            {
                if (!ushort.TryParse(args[0], out ushort t) || t > 12)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "How many months?",
                        Description = $"Either `{args[0]}` is an invalid number or its >12.",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
                await AltTimePeriodAdder(Context.Guild.Id, long.Parse(args[0]));
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The updated Alert Flagging Timespan!",
                    Description = $"We will now flag an account as an Alt if it's {await AltTimePeriodGetter(Context.Guild.Id)} months or younger on Discord",
                    Color = Blurple,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"To change it yet again, do `{await PrefixGetter(Context.Guild.Id)}alttime <months>`"
                    }
                }.WithCurrentTimestamp());
            }
        }
    }
}