using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;
using static RoleX.Modules.Services.SqliteClass;
namespace RoleX.Modules.Trading
{
    [DiscordCommandClass("Trading", "Class with trading related commands")]
    public partial class Tradinglist : CommandModuleBase
    {
        [Alt("tlist")]
        [DiscordCommand("tradinglist", commandHelp = "tradinglist", description = "Shows your trading list")]
        public async Task ShowTradingList(params string[] lc)
        {
            if (lc.Length == 0 || GetUser(lc[0]) == null)
            {
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = $"**{Context.User.Username}#{Context.User.Discriminator}**'s Trading List",
                    Color = Blurple,
                    Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Buying",
                        Value = $"{(await StringGetter(Context.User.Id, TradeTexts.Buying) == "" ? "*None*": string.Join('\n',(await StringGetter(Context.User.Id, TradeTexts.Buying)).Remove(0,1).Split(';').Select((al, idx) => $"{idx+1}) {al}")))}"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Selling",
                        Value = $"{(await StringGetter(Context.User.Id, TradeTexts.Selling) == "" ? "*None*": string.Join('\n',(await StringGetter(Context.User.Id, TradeTexts.Selling)).Remove(0,1).Split(';').Select((al, idx) => $"{idx+1}) {al}")))}"
                    }
                }
                }.WithCurrentTimestamp());
                return;
            }

            var gu = await GetUser(lc[0]);
            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = $"**{gu.Username}#{gu.Discriminator}**'s Trading List",
                Color = Blurple,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Buying",
                        Value = $"{(await StringGetter(gu.Id, TradeTexts.Buying) == "" ? "*None*": string.Join('\n',(await StringGetter(gu.Id, TradeTexts.Buying)).Remove(0,1).Split(';').Select((al, idx) => $"{idx+1}) {al}")))}"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Selling",
                        Value = $"{(await StringGetter(gu.Id, TradeTexts.Selling) == "" ? "*None*": string.Join('\n',(await StringGetter(gu.Id, TradeTexts.Selling)).Remove(0,1).Split(';').Select((al, idx) => $"{idx+1}) {al}")))}"
                    }
                }
            }.WithCurrentTimestamp());
        }
    }
}