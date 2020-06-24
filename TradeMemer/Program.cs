﻿using System;
using System.Linq;
using System.Threading.Tasks;
using BetterCommandService;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DMCG_Answer.modules;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualBasic;
using Discord.Net;
using System.IO;
using System.Runtime.InteropServices;
namespace DMCG_Answer
{ 
    class Program
    {
        IEmote Dealdone = new Emoji("🇩");
        IEmote tick = new Emoji("✅");
        static string fpath = Directory.GetCurrentDirectory() + "" + "/token.txt";
        public static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
        private DiscordSocketClient _client;
        CustomCommandService _service = new CustomCommandService(new Settings()
        {
            DefaultPrefix = '!'
        });
        public async Task MainAsync()
        {
            //Console.WriteLine("The list of databases on this server is: ");
            //foreach (var db in dbList)
            //{
            //    Console.WriteLine(db);
            //}
            _client = new DiscordSocketClient();

            _client.Log += Log;

            _client.MessageReceived += HandleCommandAsync;

            _client.ReactionAdded += HandleReactionAsync;

            _client.JoinedGuild += HandleJoinAsync;
            Console.WriteLine(fpath);
            var token = File.ReadAllLines(fpath)[0];
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await _client.SetGameAsync("!trade help",null,ActivityType.Playing);
            await Task.Delay(-1);
        }
        internal async Task HandleJoinAsync(SocketGuild guild) {
             new Thread(async () => {
                 try
                 {
                     var ownerMbed = new EmbedBuilder();
                     ownerMbed.Title = "Permissions required for Trade Memer";
                     ownerMbed.Description = "\nAll the below permissions can easily be granted by giving Admin to the bot, however the bot needs ~\n\n`Read, Write and React`\nThis is for reading commands, writing trades and reacting\n\n`Manage Messages`\nThis is for controlling trade reactions\n\n`Create channel` (optional)\nIf this is given, the bot automatically creates a channel called marketplace, if not the Admins of the channel have to do so for the bot.\n\n`Embed Links`\nThis is for support cmds\n\n*Thank you for using Trade Memer, we hope u like it!*";
                     ownerMbed.Color = Color.Red;
                     await guild.Owner.SendMessageAsync("", false, ownerMbed.Build());
                     await Task.Delay(20000);
                     EmbedBuilder embed = new EmbedBuilder();
                     embed.Color = Color.Purple;
                     embed.Title = $"Thanks for inviting me, o people of {guild.Name}";
                     embed.Description = "Do !trade to start right off!!!\nAnd if you feel like, do !vote for helping us help more servers";
                     var chn = guild.SystemChannel as SocketTextChannel;
                     if (guild.SystemChannel == null)
                     {
                         chn = guild.DefaultChannel;
                     }
                     if (chn == null) { chn = guild.TextChannels.First(); }
                     if (!guild.TextChannels.Any()) return;
                     await chn.SendMessageAsync("", false, embed.Build());
                 } 
                 catch (HttpException)
                 {
                     await guild.Owner.SendMessageAsync("I do not have perms!!! Please give them to me!");
                 }
            }).Start();
        }
        internal async Task HandleCommandResult(ICommandResult result, SocketUserMessage msg)
        {
            string completed = resultformat(result.IsSuccess);
            if (result.IsSuccess)
            {
                new Thread(async () => {
                    EmbedBuilder eb = new EmbedBuilder();
                    eb.Color = Color.Green;
                    eb.Title = "**Command Log**";
                    eb.Description = $"The Command {msg.Content.Split(' ').First()} was used in {msg.Channel.Name} by {msg.Author.Username + "#" + msg.Author.Discriminator} \n\n **Full Message** \n `{msg.Content}`\n\n **Result** \n {completed}";
                    eb.Footer = new EmbedFooterBuilder();
                    eb.Footer.Text = "Command Autogen";
                    eb.Footer.IconUrl = _client.CurrentUser.GetAvatarUrl();
                    await _client.GetGuild(591660163229024287).GetTextChannel(712144160383041597).SendMessageAsync("", false, eb.Build());
                }).Start();  
            }
        }
        internal static string resultformat(bool isSuccess)
        {
            if (isSuccess)
                return "Success";
            if (!isSuccess)
                return "Failed";
            return "Unknown";
        }

        public async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            try
            {
                if (msg == null) return;
                if (msg.Channel.GetType() == typeof(SocketDMChannel)) {
                    return; 
                }
                //var ca = msg.Content.ToCharArray();
                //if (ca.Length == 0) return;
                var context = new SocketCommandContext(_client, msg);
                if (_service.ContainsUsedPrefix(msg.ToString()))
                {
                    if (!context.User.IsBot)
                    {
                        Console.WriteLine("User is not a bot.");
                        new System.Threading.Thread(async () =>
                        {
                            try
                            {
                                var result = await _service.ExecuteAsync(context);
                                Console.WriteLine(context.User.Username + ": " + result.Result + " in channel " + context.Channel.Name + "(" + msg.Channel.GetType().ToString() + ")");
                                await HandleCommandResult(result, msg);
                            } catch (HttpException)
                            {
                                await context.Guild.Owner.SendMessageAsync("I do not have perms!!! Please give them to me!");
                            }
                        }).Start();
                    }
                    else Console.WriteLine("User is bot");
                }
                else if (msg.ToString().Contains("show me da guildz"))
                {
                    var res = int.TryParse(msg.ToString()[0].ToString(), out int resultant);
                    if (!res) return;
                    try
                    {
                        await context.Channel.SendMessageAsync(_client.Guilds.ElementAt(resultant).Name);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        await context.Channel.SendMessageAsync($"We are not in {resultant + 1} servers yet :grin:");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"We have encountered an error {e}");
                await msg.Channel.SendMessageAsync("Uhh there was an error. I have DMed my creator, DJ001 and he will solve this as soon as possible.");
                await Task.Delay(2000);
            }
        }
        public async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var msg = await arg1.GetOrDownloadAsync();
            if (arg3 == null || !msg.Embeds.Any()) return;
            var mBed = msg.Embeds.First();
            if (arg2.Name == "marketplace" && mBed.Color == Color.Green)
            {
                new Thread(async () =>
                {
                    var userSellerStr = Regex.Match(mBed.Description, @"\d+").Value;
                    ulong userIdSeller = ulong.Parse(userSellerStr);
                    var userSeller = await arg2.GetUserAsync(userIdSeller);
                    var userIDReacter = arg3.UserId;
                    var userReacter = await arg2.GetUserAsync(userIDReacter);
                    if (userReacter.IsBot) return;
                    if (userSeller.Id == userReacter.Id && arg3.Emote.Name == Dealdone.Name)
                    {
                        await msg.DeleteAsync();
                        return;
                    }
                    Console.WriteLine("First if passed");
                    if (arg3.Emote.Name == tick.Name)
                    {
                        Console.WriteLine("Second if passed");
                        var DMCReacter = await userReacter.GetOrCreateDMChannelAsync();
                        var DMCSeller = await userSeller.GetOrCreateDMChannelAsync();
                        await DMCReacter.SendMessageAsync($"You have accepted a sale and a DM has been sent to {userSeller.Username}.\nYou can expect a reply shortly.");
                        await DMCSeller.SendMessageAsync($"{userReacter} has accepted your deal! Contact them for finalizing.");
                        await Task.Delay(5000);
                    }
                    else
                    {
                        await msg.RemoveReactionAsync(arg3.Emote, userReacter);
                    }
                }).Start();
            }          
        }
    }
}