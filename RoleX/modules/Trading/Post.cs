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
    public class Post : CommandModuleBase
    {
        [DiscordCommand("post", commandHelp ="post", description ="Posts the set Trading Embed in all Mutual Servers")]
        public async Task RPost(params string[] args)
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
    }
}