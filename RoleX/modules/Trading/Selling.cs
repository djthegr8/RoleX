using System.Linq;
using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;
using static RoleX.Modules.Services.SqliteClass;
namespace RoleX.Modules.Trading
{
    // Commented as not required in Partial class
    // [DiscordCommandClass("Trading", "Class with trading related commands")]
    public partial class Tradinglist : CommandModuleBase
    {
        [Alt("tsell")]
        [DiscordCommand("selling", commandHelp = "selling add/remove <item1> <price1>| <item2> <price2> (etc)", description = "Adds/Removes items on the users' selling lists", example = "selling add 1 pepe 60k| 3 kn DM Offer| All ur fish`\n`selling remove 3")]
        public async Task SellingList(params string[] args)
        {
            if (args.Length < 2 || (args[0].ToLower() != "add" && args[0].ToLower() != "remove"))
            {
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "I don't know what to do <:facepalm:713027249426792498>",
                    Description = $"The way to run this command is `{await PrefixGetter(Context.Guild.Id)}selling add/remove Item1 and stuff about it, Item2 and...`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var ubl = await StringGetter(Context.User.Id, TradeTexts.Selling);
            if (ubl.Length != 0) ubl = ubl.Remove(0,1);
            var Count = ubl.Split(';').Length;
            var breh = string.Join(' ', args.Skip(1)).Split('|');
            switch (args[0].ToLower())
            {
                case "add":
                    if (Count == 7 || 7 - Count < breh.Length)
                    {
                        await ReplyAsync(embed: new EmbedBuilder
                        {
                            Title = "That's above the maximum trades!",
                            Description = $"Only 7 items are allowed! If you want more, then wait for RoleX Premium to release!",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                        return;
                    }
                    else if (breh.Any(kden => kden.Length > 69))
                    {
                        await ReplyAsync(embed: new EmbedBuilder
                        {
                            Title = "That's above the maximum characters!",
                            Description = $"Only 69 characters are allowed per item!",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                        return;
                    }
                    await TradeEditor(Context.User.Id, ubl + ';' + string.Join(';', breh), TradeTexts.Selling);
                    await ShowTradingList();
                    break;
                case "remove":
                    if (!ushort.TryParse(args[1], out ushort _))
                    {
                        await ReplyAsync(embed: new EmbedBuilder
                        {
                            Title = "Only indices needed",
                            Description = $"For the removal, kindly use the index of the item, which can be found using the `{await PrefixGetter(Context.Guild.Id)}tradinglist` command\nFor example, `{await PrefixGetter(Context.Guild.Id)}selling remove 3`",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                        return;
                    }
                    var lis = ubl.Split(';').ToList();
                    lis.RemoveAt(int.Parse(args[1]) - 1);
                    await TradeEditor(Context.User.Id, string.Join(';', lis), TradeTexts.Selling);
                    await ShowTradingList();
                    break;
            }
        }
    }
}