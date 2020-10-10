using System;
using System.Linq;
using System.Threading.Tasks;
using Public_Bot;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.IO;
using TradeMemer.modules;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace TradeMemer
{
    class Program
    {
        readonly static string fpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "Data" + Path.DirectorySeparatorChar + "token.txt";
        public static string token = File.ReadAllLines(fpath)[0];
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
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            _client = new DiscordSocketClient();

            _client.Log += Log;

            _client.MessageReceived += HandleCommandAsync;

            _client.JoinedGuild += HandleGuildJoinAsync;

            //Console.WriteLine(fpath);
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await _client.SetGameAsync("Supervising Roles!",null,ActivityType.Playing);
            //await _client.StopAsync();
            await Task.Delay(-1);
        }

        private async Task HandleGuildJoinAsync(SocketGuild arg)
        {
            // <@701029647760097361> or <@615873008959225856>
            await _client.GetUser(701029647760097361).SendMessageAsync($"I joined {arg.Name}, a Guild of {arg.MemberCount} members.");
            await _client.GetUser(615873008959225856).SendMessageAsync($"I joined {arg.Name}, a Guild of {arg.MemberCount} members.");
            try
            {
                await _client.GetUser(701029647760097361).SendMessageAsync($"Here's an invite!\n{(await arg.GetInvitesAsync()).First(aa => aa.IsTemporary == false && aa.MaxUses.GetValueOrDefault(1) > 1)}");
                await _client.GetUser(615873008959225856).SendMessageAsync($"Here's an invite!\n{(await arg.GetInvitesAsync()).First(aa => aa.IsTemporary == false && aa.MaxUses.GetValueOrDefault(1) > 1)}");
            }
            catch { }
            try
            {
                await arg.CurrentUser.ModifyAsync(idk => idk.Nickname = "[r] RoleX");
            }
            catch { }
        }

        internal async Task HandleCommandResult(CustomCommandService.ICommandResult result, SocketUserMessage msg, string prefi)
        { 
            await Task.Delay(10);
            string completed = Resultformat(result.IsSuccess);
            switch (result.Result)
            {
                case CommandStatus.Success:
                    EmbedBuilder eb = new EmbedBuilder
                    {
                        Color = Color.Green,
                        Title = "**Command Log**",
                        Description = $"The Command {msg.Content.Substring(prefi.Length)} was used in {msg.Channel.Name} of {(msg.Channel as SocketTextChannel).Guild.Name} by {msg.Author.Username + "#" + msg.Author.Discriminator} \n\n **Full Message** \n `{msg.Content}`\n\n **Result** \n {completed}",
                        Footer = new EmbedFooterBuilder()
                    };
                    eb.Footer.Text = "Command Autogen";
                    eb.Footer.IconUrl = _client.CurrentUser.GetAvatarUrl();
                    await _client.GetGuild(755076971041652786).GetTextChannel(758230822057934878).SendMessageAsync("", false, eb.Build());
                    break;
                case CommandStatus.Error:
                    if (result.Exception.GetType().ToString() == "System.AggregateException" && result.Exception.InnerException.GetType().ToString() == "Discord.Net.HttpException")
                    {
                        EmbedBuilder ella = new EmbedBuilder
                        {
                            Color = Color.Red,
                            Title = "**I don't have permissions!!!**",
                            Description = $"RoleX does not have the permission to do execute your command...\nThis may be because: \n1) You haven't given RoleX the needed permission for the command\n2) The user you want to mute/ban/kick is above RoleX"
                        }.WithCurrentTimestamp();
                        try
                        {
                            await msg.Author.SendMessageAsync(embed: ella.Build());
                        } catch (Exception)
                        {
                            await msg.Channel.SendMessageAsync(embed: ella.Build());
                        }
                        return;
                    }
                    EmbedBuilder emb = new EmbedBuilder
                    {
                        Color = Color.Red,
                        Title = "**An error occured...**",
                        Description = $"We are on towards fixing it! In case of any problem, DM <@701029647760097361> or <@615873008959225856> \nRefer to the below error message: ```{result.Exception}```"
                    }.WithCurrentTimestamp();
                    await msg.Channel.SendMessageAsync(embed: emb.Build());
                    await _client.GetUser(701029647760097361).SendMessageAsync(embed: emb.Build());
                    break;
                case CommandStatus.MissingGuildPermission:
                    await msg.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "**:lock: You're Missing Permissions :lock:**",
                        Color = Color.Red,
                        Description = $"Hey {msg.Author.Mention}, you're missing these permissions:\n{result.ResultMessage}"
                    }.WithCurrentTimestamp().Build());
                    break;
                case CommandStatus.NotEnoughParams or CommandStatus.InvalidParams:
                    var pref = await SqliteClass.PrefixGetter((msg.Channel as SocketGuildChannel).Guild.Id);
                    await msg.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "**That isn't how to use that command**",
                        Color = Color.Red,
                        Description = $"Do `{pref}{msg.Content.Split(' ')[0].Replace(pref,"")}` to know how!"
                    }.WithCurrentTimestamp().Build());
                    break;
                case CommandStatus.NotFound:
                    break;
                default:
                    await _client.GetUser(701029647760097361).SendMessageAsync($"See kid Idk what happened but here it is {result.Result}\n{result.ResultMessage}\n{result.Exception}");
                    break;
            }
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
                var prefu = await SqliteClass.PrefixGetter(context.Guild.Id);
                if (msg.MentionedUsers.Any(x => x.Id == _client.CurrentUser.Id))
                {
                    await context.Message.Channel.SendMessageAsync("", false, new EmbedBuilder
                    {
                        Title = "Hi! I am RoleX",
                        Description = $"The prefix of your favourite role editor bot is {prefu}",
                        Color = CommandModuleBase.Blurple
                    }.WithCurrentTimestamp().Build()
                    );
                    return;
                }
                if (msg.Content.Length <= prefu.Length) return;
                if (msg.Content.Substring(0, prefu.Length) == prefu)
                {
                    if (!context.User.IsBot)
                    {
                        new Thread(async () =>
                        {
                            try
                            {
                                var x = await _service.ExecuteAsync(context, prefu);
                                await HandleCommandResult(x, msg, prefu);
                                Console.WriteLine(context.User.Username + ": " + x.Result + " in channel " + context.Channel.Name + " of guild " + context.Guild.Name);
                            }
                            catch (Discord.Net.HttpException)
                            {
                                await context.Guild.Owner.SendMessageAsync("I do not have perms!!! Please give them to me!");
                            }
                        }).Start();
                    }
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
    }
}