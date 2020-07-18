﻿using System;
using System.Linq;
using System.Threading.Tasks;
using BetterCommandService;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using TradeMemer.modules;
using System.Collections.Generic;

namespace TradeMemer
{
    class Program
    {
        readonly Embed ownerMbed = new EmbedBuilder
                     {
                         Title = "Permissions required for Trade Memer",
                         Description = "\nAll the below permissions can easily be granted by giving Admin to the bot, however the bot needs ~\n\n`Read, Write and React`\nThis is for reading commands, writing trades and reacting\n\n`Manage Messages`\nThis is for controlling trade reactions\n\n`Create channel` (optional)\nIf this is given, the bot automatically creates a channel called marketplace, if not the Admins of the channel have to do so for the bot.\n\n`Embed Links`\nThis is for support cmds\n\n*Thank you for using Trade Memer, we hope u like it!*",
                         Color = Color.Red
                     }.Build();
        readonly IEmote Dealdone = new Emoji("❌");
        readonly IEmote tick = new Emoji("✅");
        readonly static string fpath = Directory.GetCurrentDirectory() + "" + "/token.txt";
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
        public CustomCommandService _service = new CustomCommandService(new Settings()
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

            _client.LatencyUpdated += StatusUpdateAsync;
            Console.WriteLine(fpath);
            var token = File.ReadAllLines(fpath)[0];
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await _client.SetGameAsync("!trade help",null,ActivityType.Playing);
            await Task.Delay(-1);
        }
        internal async Task HandleJoinAsync(SocketGuild guild) {
            await Task.Delay(10);
             new Thread(async () => {
                 try
                 {
                     await guild.Owner.SendMessageAsync("", false, ownerMbed);
                     await Task.Delay(20000);
                     EmbedBuilder embed = new EmbedBuilder
                     {
                         Color = Color.Purple,
                         Title = $"Thanks for inviting me, o people of {guild.Name}",
                         Description = "Do !trade help to start right off!!!\nAnd if you feel like, do !invite and !vote for helping us help more servers"
                     };
                     var chn = guild.SystemChannel;
                     if (guild.SystemChannel == null)
                     {
                         chn = guild.DefaultChannel;
                     }
                     if (chn == null) { chn = guild.TextChannels.First(); }
                     if (!guild.TextChannels.Any()) return;
                     await chn.SendMessageAsync("", false, embed.Build());
                 } 
                 catch (Exception)
                 {
                     await guild.Owner.SendMessageAsync("I do not have perms!!! Please give them to me!");
                 }
            }).Start();
        }
        internal async Task HandleCommandResult(ICommandResult result, SocketUserMessage msg)
        {
            await Task.Delay(10);
            string completed = Resultformat(result.IsSuccess);
            if (result.IsSuccess)
            {
                new Thread(async () =>
                {
                    EmbedBuilder eb = new EmbedBuilder
                    {
                        Color = Color.Green,
                        Title = "**Command Log**",
                        Description = $"The Command {msg.Content.Split(' ').First()} was used in {msg.Channel.Name} of {(msg.Channel as SocketTextChannel).Guild.Name} by {msg.Author.Username + "#" + msg.Author.Discriminator} \n\n **Full Message** \n `{msg.Content}`\n\n **Result** \n {completed}",
                        Footer = new EmbedFooterBuilder()
                    };
                    eb.Footer.Text = "Command Autogen";
                    eb.Footer.IconUrl = _client.CurrentUser.GetAvatarUrl();
                    await _client.GetGuild(732300342888497263).GetTextChannel(732300343655923807).SendMessageAsync("", false, eb.Build());
                }).Start();  
            }
        }
        internal async Task StatusUpdateAsync(int arg1, int arg2)
        {
            ulong alusr = 0;
            _client.Guilds.ToList().ForEach(x => alusr += (ulong)x.Users.Count);
            string[] status = new string[]
            {
                $"Trading in {_client.Guilds.Count} unique servers!",
                $"Serving {alusr} Users in {_client.Guilds.Count} Different Guilds",
                $"Executing trades..",
                $"Running the batch file",
                $"Handling exceptions",
                $"Trading with {alusr} different users!",
                $"Opening a new portal to the parallel universe",
                $"Searching for buyers",
                $"Crying in binary",
                $"Programming in TensorFlow",
                $"Gambling with Dank Memer",
                $"Thanking Swiss001 devs",
                $"Searching for the singularity",
                $"Chilling with my Bro",
                $"Preparing for the Bot Oscars!"
            };
            await _client.SetGameAsync($"!trade help - {status[new Random().Next(0, status.Length)]}", null, ActivityType.Playing);
        }
        internal static string Resultformat(bool isSuccess)
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
                if (msg.Channel.GetType() == typeof(SocketDMChannel))
                {
                    return;
                }
                
                    var ca = msg.Content.ToCharArray();
                    if (ca.Length == 0) return;
                    var context = new SocketCommandContext(_client, msg);
                
                if (_service.ContainsUsedPrefix(msg.Content))
                {
                    if (!context.User.IsBot)
                    {
                        
                        if (msg.Content.Split(' ')[0].Contains("report-info"))
                        {
                            if (msg.Content.Split(' ').Length == 1)
                            {
                                await context.Channel.SendMessageAsync("You have to tell the ID to find the report info of!");
                            }
                            Console.WriteLine("Run");
                            bool xztest = Guid.TryParse(msg.Content.Split(' ')[1], out Guid alpha);
                            if (!xztest) return;
                            else
                            {
                                var eb = await Class3.GuidInfoGetter(alpha);
                                await context.Channel.SendMessageAsync("", false, eb.Build());
                                return;
                            }
                        }

                        //if (!MyCommandClass.Commands.Any(x => x.CommandName == msg.Content.Skip(1).ToString()) && !MySecondCommandClass.Commands.Any(x => x.CommandName == msg.Content.Skip(1).ToString())) return;
                        var tup = await Class3.SpeedCheck(context.User.Id);
                        if (tup.Item1 >= 3)
                        {
                            return;
                        }
                        new Thread(async () =>
                        {
                            try
                            {
                                var result = await _service.ExecuteAsync(context);
                                Console.WriteLine(context.User.Username + ": " + result.Result + " in channel " + context.Channel.Name + " of guild " + context.Guild.Name);
                                await HandleCommandResult(result, msg);
                            }
                            catch (Exception)
                            {
                                await context.Guild.Owner.SendMessageAsync("I do not have perms!!! Please give them to me!");
                            }
                        }).Start();
                    }
                    else Console.WriteLine("User is bot");
                }
                else if (msg.ToString().Contains("show me da guildz") && (msg.Author.Id == 541998151716962305 || msg.Author.Id == 701029647760097361) )
                {
                    string st = "```";
                    foreach(var srver in _client.Guilds)
                    {
                        st += $"{srver.Name}\t{srver.MemberCount}\n";
                    }
                    st += "```";
                    await msg.Channel.SendMessageAsync(st);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"We have encountered an error {e}");
                await msg.Channel.SendMessageAsync("Uhh there was an error. I have DMed my creator, DJ001 and he will solve this as soon as possible.");
                await _client.GetUser(701029647760097361).SendMessageAsync($"There was an error in {(msg.Channel as SocketGuildChannel).Guild.Name}\n{e}");
                await Task.Delay(2000);
            }
        }
        public async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var msg = await arg1.GetOrDownloadAsync();
            if (msg == null) return;
            if (arg3 == null || !msg.Embeds.Any()) return;
            var mBed = msg.Embeds.First();
            if (arg2.Name.ToLower().Contains("marketplace"))
            {
                new Thread(async () =>
                {
                    var userSellerStr = Regex.Match(mBed.Description, @"\d+").Value;
                    ulong userIdSeller = ulong.Parse(userSellerStr);
                    var userSeller = await arg2.GetUserAsync(userIdSeller);
                    var userIDReacter = arg3.UserId;
                    var userReacter = await arg2.GetUserAsync(userIDReacter);
                    if (userReacter.IsBot) return;
                    if ((userSeller.Id == userReacter.Id || userReacter.Id == 541998151716962305 || userReacter.Id == 701029647760097361) && arg3.Emote.Name == Dealdone.Name)
                    {
                        try
                        {
                            await msg.DeleteAsync();
                            return;
                        } catch (Exception)
                        {
                            return;
                        }
                    }
                    if (userSeller.Id == userReacter.Id) { 
                        await msg.RemoveReactionAsync(arg3.Emote, userReacter);
                        return;
                    }
                    Console.WriteLine("First if passed");
                    if (arg3.Emote.Name == tick.Name)
                    {
                        Console.WriteLine("Second if passed");
                        var DMCReacter = await userReacter.GetOrCreateDMChannelAsync();
                        var DMCSeller = await userSeller.GetOrCreateDMChannelAsync();
                        var itemFull = mBed.Title.Split(' ')[0];
                        if (mBed.Color == Color.Green)
                        {
                            await DMCReacter.SendMessageAsync($"You have accepted the sale of {itemFull} and a DM has been sent to {userSeller.Username}.\nYou can expect a reply shortly.");
                            await DMCSeller.SendMessageAsync($"{userReacter.Username} has accepted your deal of {itemFull} in {(arg2 as SocketGuildChannel).Guild.Name}! Contact them for finalizing.");
                        }
                        else if (mBed.Color == Color.Blue)
                        {
                            await DMCReacter.SendMessageAsync($"You have offered to sell {itemFull} to {userSeller.Username} and a DM has been sent to them!\nYou can expect a reply shortly.");
                            await DMCSeller.SendMessageAsync($"{userReacter.Username} has {itemFull} to sell in {(arg2 as SocketGuildChannel).Guild.Name}!! Contact them for finalizing.");
                        }
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