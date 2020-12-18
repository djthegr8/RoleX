using Discord;
using Discord.WebSocket;
using System;
using System.Timers;
using Public_Bot;
using System.Collections.Generic;
using static RoleX.Program;
using System.Text.RegularExpressions;
using GuildPermissions = Public_Bot.GuildPermissions;
using System.Linq;
using System.Threading.Tasks;
using static RoleX.modules.SqliteClass;
namespace RoleX.modules
{
    [DiscordCommandClass("Trading", "Class with trading related commands")]
    class TradingClass : CommandModuleBase
    {
        [DiscordCommand("post", commandHelp ="post", description ="Posts the set Trading Embed in all Mutual Servers")]
        public async Task Post(params string[] args)
        {
            Embed mbed = new EmbedBuilder
            {
                Title = $"**{Context.User.Username}#{Context.User.Discriminator}**'s Trading List",
                Color = Blurple,
                Description = $"Ping or DM {Context.User.Mention} if interested.",
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
            }.WithCurrentTimestamp().Build();
            var mes = await Context.Channel.SendMessageAsync(embed: new EmbedBuilder
            {
                Title = "Posting your trades...",
                Description = "Starting the post",
                Color = Blurple
            }.WithCurrentTimestamp().Build());

            foreach(var gld in (await CL2.GetGuildsAsync()).Where(k => (k.GetUserAsync(Context.User.Id)).GetAwaiter().GetResult() != null))
            {
                string msg;
                if (await TradingChanGetter(gld.Id) == 0)
                {
                    msg = $"Posting failed in **{gld.Name}**...";
                } else if (await CooldownGetter(gld.Id, Context.User.Id)) {
                    msg = $"On cooldown in **{gld.Name}**...";
                } else
                {
                    var chID = await TradingChanGetter(gld.Id);
                    var chn = await gld.GetTextChannelAsync(chID);
                    await chn.SendMessageAsync($"Trading post from {Context.User.Username}#{Context.User.Discriminator}!", embed: mbed);
                    await CooldownAdder(gld.Id, Context.User.Id);
                    Timer timer = new Timer
                    {
                        AutoReset = false,
                        Interval = await SlowdownTimeGetter(gld.Id) * 60000,
                        Enabled = true
                    };
                    timer.Elapsed += async (_, _) => {
                        // Remove the user from Cooldown from server.
                        await CooldownRemover(gld.Id, Context.User.Id);
                    };
                    msg = $"Posting Completed in **{gld.Name}**";
                }
                await mes.ModifyAsync(k => k.Embed = new EmbedBuilder { Title = mes.Embeds.First().Title, Description = mes.Embeds.First().Description + "\n" + msg, Color = Blurple }.WithCurrentTimestamp().Build());
            }
            await mes.ModifyAsync(k => k.Embed = new EmbedBuilder { Title = "Posting Complete", Description = mes.Embeds.First().Description, Color = Blurple }.WithCurrentTimestamp().Build());
        }
        [Alt("tlist")]
        [DiscordCommand("tradinglist", commandHelp = "tradinglist", description = "Shows the user's trading list")]
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
            else
            {
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
        [Alt("tbuy")]
        [DiscordCommand("buying", commandHelp = "buying add/remove <item1> <price1>| <item2> <price2> (etc)", description = "Adds/Removes items on the users' buying lists", example = "buying add 1 pepe 60k| 3 kn DM Offer| All ur fish`\n`buying remove 3")]
        public async Task BuyingList(params string[] args)
        {
            if (args.Length < 2 || (args[0].ToLower() != "add" && args[0].ToLower() != "remove"))
            {
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "I don't know what to do <:facepalm:713027249426792498>",
                    Description = $"The way to run this command is `{await PrefixGetter(Context.Guild.Id)}buying add/remove Item1 and stuff about it, Item2 and...`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var ubl = await StringGetter(Context.User.Id, TradeTexts.Buying);
            if (ubl.Length != 0) ubl = ubl.Remove(0,1);
            var Count = ubl.Split(';').Length;
            var breh = string.Join(' ', args.Skip(1)).Split('|');
            ; switch (args[0].ToLower())
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
                    await TradeEditor(Context.User.Id, ubl +';' + string.Join(';', breh), TradeTexts.Buying);
                    await ShowTradingList();
                    break;
                case "remove":
                    if (!ushort.TryParse(args[1], out ushort _))
                    {
                        await ReplyAsync(embed: new EmbedBuilder
                        {
                            Title = "Only indices needed",
                            Description = $"For the removal, kindly use the index of the item, which can be found using the `{await PrefixGetter(Context.Guild.Id)}tradinglist` command\nFor example, `{await PrefixGetter(Context.Guild.Id)}buying remove 3`",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                        return;
                    }
                    var lis = ubl.Split(';').ToList();
                    lis.RemoveAt(int.Parse(args[1]) - 1);
                    await TradeEditor(Context.User.Id, string.Join(';', lis), TradeTexts.Buying);
                    await ShowTradingList();
                    break;
            }
        }
        [Alt("tsm")]
        [GuildPermissions(GuildPermission.ManageGuild)]
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
        [Alt("tchan")]
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