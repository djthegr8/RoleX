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
    public class Tradeslowmode : CommandModuleBase
    {
        [DiscordCommand("tradeslowmode", commandHelp = "tradeslowmode num_minutes", description = "Sets the number of minutes for trading slowmode", example = "alttime 35")]
        public async Task Alttime(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The current trading slowmode",
                    Description = $"We will allow RoleX users to post trade ads every {await SlowdownTimeGetter(Context.Guild.Id)} mins",
                    Color = Blurple,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"To change it, do `{await PrefixGetter(Context.Guild.Id)}tsm num_mins`"
                    }
                }.WithCurrentTimestamp());
                return;
            }
            else
            {
                Regex regex = new Regex("[a-gi-zA-GI-Z]");
                
                args[0] = regex.Replace(args[0], "");
                if (!ushort.TryParse(args[0].Replace("h", "", System.StringComparison.OrdinalIgnoreCase), out ushort t) || t > 180)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "How many minutes?",
                        Description = $"Either `{args[0]}` is an invalid number or its above 180.",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
                var hH = System.Convert.ToUInt64(args[0].Any(x => x == 'h' || x == 'H')) * 60 + System.Convert.ToUInt64(!args[0].Any(x => x == 'h' || x == 'H'));
                await SlowdownTimeAdder(Context.Guild.Id, ulong.Parse(args[0]) * hH);
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The updated Trading Slowmode!",
                    Description = $"We will now allow users to post ads every {await SlowdownTimeGetter(Context.Guild.Id)} minutes",
                    Color = Blurple,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"To change it yet again, do `{await PrefixGetter(Context.Guild.Id)}tsm 3`"
                    }
                }.WithCurrentTimestamp());
            }
        }
    }
}