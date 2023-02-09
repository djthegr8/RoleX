using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Hermes.Modules.Developer;
using Hermes.Modules.Services;

namespace Hermes
{
    public class Program
    {
        private static readonly IEmote Cooldown = new Emoji("⏳");
        private static readonly string[] al = @"I am like an angel
Mankind’s angel
My heart is like a rose
Mankind’s own
There is creation
By God
Heaven on earth
Mankind’s Angel
Lonely for you
Like an angel
Your love is mine
True angel
Without you
I always live alone
As the heart beats
Without a soul
I fall in love
With you always
Like angels
Love other angels
Angel, angel
Mankind’s angels
La la la la
La la la la
[SOLO]
I and you
Are both like angels
Within a human
Is an angel
Love is the
Love of an angel
You are my lovely 
sweet angel
Without you
My love stays alone
Like a mermaid
Lives alone
When I found you
I was so glad
As I came
Lovely heaven
Angel, angel
Mankind’s angel
La la la la
La la la la
[SPOKEN]
We can all be angels
A human like an angel
Shines as a star
The angel’s character
speaks like a flower
The stars are 
decorated by angels
Like the dew 
feels petals 
of a flower
All angels remain quiet
Like those flowers 
that resist silent
But spread their essence 
and always reside jovial
[SUNG]
You are beautiful
As the universe
Charming angel
You are beautiful
Oh angel
You are so special
As you are 
my lovely reflection
Son you are
My honor my angel
You are my faith my soul
My angel
Your life is like
A pretty flower
Live is so graceful
Like an angel
Angel, angel
Mankind’s angel
La la la la
La la la la".Split('\n');
        private int lc = 0;
        private static readonly string fpath =
            string.Join(Path.DirectorySeparatorChar,
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Split(Path.DirectorySeparatorChar)
                    .SkipLast(1)) + Path.DirectorySeparatorChar + "Data" + Path.DirectorySeparatorChar + "token.txt";

        public static readonly string token = File.ReadAllLines(fpath)[0];

        public static DiscordShardedClient Client;

        // public static DiscordRestClient CL2;
        public static CustomCommandService _service = new(new Settings());

        public DateTime lastAmariRequest = DateTime.MinValue;

        public static void Main(string[] _)
        {
            new Program().MainlikeAsync().GetAwaiter().GetResult();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public async Task MainlikeAsync()
        {
            //Console.WriteLine("The list of databases on this server is: ");
            //foreach (var db in dbList)
            //{

            //    Console.WriteLine(db);
            //}
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            Client = new DiscordShardedClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true, LargeThreshold = 250,
                GatewayIntents = GatewayIntents.GuildMessageReactions|GatewayIntents.GuildMembers|GatewayIntents.GuildMessages|GatewayIntents.Guilds|GatewayIntents.MessageContent, TotalShards = 1
            });
            // CL2 = new DiscordRestClient(new DiscordSocketConfig { AlwaysAcknowledgeInteractions = false, AlwaysDownloadUsers = true, LargeThreshold = 250, GatewayIntents = GatewayIntents.All });
            Client.Log += Log;

            Client.MessageReceived += HandleCommandAsync;

            Client.JoinedGuild += HandleGuildJoinAsync;

            Client.ShardReady += HandleReadyAsync;

            Client.UserJoined += AltAlertAsync;

            Client.ReactionAdded += HandleReactionAsync;
            
            Client.SlashCommandExecuted += SlashCommandHandler;

            var tname = new System.Timers.Timer(45000);
            tname.AutoReset = true;
            tname.Elapsed += LyricUpdater
            var __ = new Timer(async _ =>
            {
                if (Client.LoginState != LoginState.LoggedIn) return;
                // Reminder thingies
                var currTime = DateTime.UtcNow;
                var allRems =
                    await SqliteClass.GetReminders(
                        $"Select * from reminders where Finished = 0 and Time = \"{currTime:u}\"");
                if (allRems.Count > 0)
                    allRems.ForEach(async x =>
                    {
                        try
                        {
                            await Client.GetUser(x.UserId).SendMessageAsync("", false, new EmbedBuilder
                            {
                                Title = "The time has come.",
                                Description = $"You asked to be reminded about `{x.Reason}...`, and it's time!",
                                Color = CommandModuleBase.Blurple
                            }.WithCurrentTimestamp().Build());
                        }
                        catch
                        {
                        }

                        await SqliteClass.ReminderFinished(x);
                    });
            }, null, 0, 1000);
            Client.GuildMemberUpdated += async (_previous, later) =>
            {
                try
                {
                    await Task.Run(async () =>
                    {
                        var previous = await _previous.GetOrDownloadAsync();
                        if (previous.Status != later.Status && later.Status != UserStatus.Offline &&
                            await SqliteClass.TrackCdAllUlongIDs(
                                $"select UserID from track_cd where TUserID = {later.Id};") != new List<ulong>())
                        {
                            var lis = await SqliteClass.TrackCdAllUlongIDs(
                                $"select UserID from track_cd where TUserID = {later.Id};");
                            foreach (var user in lis)
                            {
                                await Client.GetUser(user)
                                    .SendMessageAsync($"<@{later.Id}> is now {later.Status}! Chat with them now!");
                                await SqliteClass.Track_CDRemover(user, later.Id);
                            }
                        }
                    });
                }
                catch
                {
                }
            };
            // await CL2.LoginAsync(TokenType.Bot, token);
            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();
            await Client.SetGameAsync("Supervising Roles!");
            await Task.Delay(-1);
        }
        private async Task LyricUpdater()
        {
            try {
                var sch = Client.GetGuild(591660163229024287).GetTextChannel(1073247018618716191);
                await sch.ModifyAsync(k => k.Name = al[lc])
                lc = lc + 1;
            } catch { 
            }
        }
        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            await command.RespondAsync($"Bro stop just stop. Ur as worthless as this command.");
        }
        private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> arg1,
            Cacheable<IMessageChannel, ulong> _arg2, SocketReaction arg3)
        {
            try
            {
               await Task.Run(async () =>
                {
                    try
                    {
                        var arg2 = await _arg2.GetOrDownloadAsync();
                        var msgid = arg1.Id;
                        var chnlId = arg2.Id;
                        var stc = arg2 as SocketTextChannel;
                        var usr = stc?.GetUser(arg3.UserId);
                        if (usr == null) return;
                        try
                        {
                            if (usr.Id == Client?.CurrentUser?.Id) return;
                            var msg = await stc.GetMessageAsync(msgid);
                            if (msg == null) return;
                            var reros = await SqliteClass.GetReactRoleAsync(
                                $"SELECT * FROM reactroles WHERE ChannelID = {chnlId} AND MessageID = {msgid};");
                            if (reros.Count == 0) return;
                            var rero = reros[0];
                            var index = rero.Emojis.IndexOf(arg3.Emote.ToString());
                            if (index == -1) return;
                            var role = stc.Guild.GetRole(rero.Roles[index]);
                            if (role == null) return;
                            if (rero.BlackListedRoles.Length != 0 &&
                                usr.Roles.Any(k => rero.BlackListedRoles.Any(l => l == k.Id))) return;
                            if (rero.WhiteListedRoles.Length != 0 &&
                                !usr.Roles.Any(k => rero.WhiteListedRoles.Any(l => l == k.Id))) return;
                            // Just add the role i guess.
                            if (usr.Roles.All(x => x.Id != role.Id)) await usr.AddRoleAsync(role);
                            else await usr.RemoveRoleAsync(role);
                            await msg.RemoveReactionAsync(arg3.Emote, usr);
                        }
                        catch
                        {
                            //ignore
                        }
                    }
                    catch
                    {
                    }
                });
            }
            catch
            {
                // continue to ignore
            }
        }

        private async Task AltAlertAsync(SocketGuildUser arg)
        {
            // i dont know what is going on
        }

        private async Task HandleReadyAsync(DiscordSocketClient _)
        {
            try
            {
                var g = Client.Guilds.First(x => x.Id == 755076971041652786);
                var ch = g.GetTextChannel(762554740491026444);
                if (ch == null) return;
                await ch.SendMessageAsync("We're back on bois!");
                var lor = await SqliteClass.GetReminders("Select * from reminders where Finished = 0;");
                foreach (var idek in lor)
                    if (idek.TimeS.CompareTo(DateTime.UtcNow) > 0)
                    {
                        await SqliteClass.ReminderFinished(idek);
                        Console.WriteLine($"Ignoring reminder {idek.Id}, was offline.");
                    }
            }
            catch
            {
            }
        }

        private async Task HandleGuildJoinAsync(SocketGuild arg)
        {
            try
            {

                foreach (var devid in CommandModuleBase.devids)
                {
                    var user = Client.GetUser(devid);
                    await user.SendMessageAsync(
                        $"I joined {arg.Name}, a Guild of {arg.MemberCount} members, making the count at {Client.Guilds.Count}.",
                        false);

                    try
                    {
                        try
                        {
                            var rim = (await arg.GetInvitesAsync()).FirstOrDefault() ??
                                      await arg.DefaultChannel.CreateInviteAsync();

                            await user.SendMessageAsync(
                                $"Here's an invite!\n{rim}");
                        }
                        catch
                        {
                            // idc
                        }
                    }
                    catch
                    {
                        // let it be, let it beeeeee
                    }
                }
                //try block so no errors :)

                try
                {
                    await arg.CurrentUser.ModifyAsync(async idk =>
                        idk.Nickname = $"[{await SqliteClass.PrefixGetter(arg.Id)}] Hermes");
                }
                catch
                {
                }
                // Register slash commands
                var testcmd = new SlashCommandBuilder().WithName("why").WithDescription("Questions the user's existence.");
                await arg.CreateApplicationCommandAsync(testcmd.Build());

            }
            catch
            {
            }

        }

        internal static async Task HandleCommandResult(CustomCommandService.ICommandResult result,
            SocketUserMessage msg, string prefi)
        {
            try
            {
                await Task.Delay(10);
                var completed = Resultformat(result.IsSuccess);
                switch (result.Result)
                {
                    case CommandStatus.Success:
                        var eb = new EmbedBuilder
                        {
                            Color = Color.Green,
                            Title = "**Command Log**",
                            Description =
                                $"The Command {msg.Content[prefi.Length..]} was used in {msg.Channel.Name} of {(msg.Channel as SocketTextChannel).Guild.Name} by {msg.Author.Username + "#" + msg.Author.Discriminator}",
                            Footer = new EmbedFooterBuilder
                            {
                                Text = "Command Autogen",
                                IconUrl = Client.CurrentUser?.GetAvatarUrl()
                            }
                        };
                        try
                        {
                            var g = Client?.GetGuild(858687271308754985);
                            if (g == null) break;
                            await g.GetTextChannel(859029290070704128).SendMessageAsync("", false, eb.Build());
                        }
                        catch
                        {
                            // ignore
                        }

                        break;
                    case CommandStatus.BotMissingPermissions:
                        await msg.Channel.SendMessageAsync("", false, new EmbedBuilder
                            {
                                Title = $"I require the {result.ResultMessage} permission",
                                Description =
                                    $"For this command to run, we require the `{result.ResultMessage}` permission.\n To understand all our required permissions, run `{await SqliteClass.PrefixGetter((msg.Channel as SocketGuildChannel).Guild.Id)}setup`",
                                Color = Color.Red
                            }.WithCurrentTimestamp()
                            .Build()
                        );
                        break;
                    case CommandStatus.Error:
                        if (result.Exception.GetType() == typeof(AggregateException) &&
                            result.Exception.InnerException.GetType() == typeof(HttpException))
                        {
                            var ella = new EmbedBuilder
                            {
                                Color = Color.Red,
                                Title = "**I don't have permissions!!!**",
                                Description =
                                    "Hermes does not have the permission to do execute your command...\nThis may be because: \n1) You haven't given Hermes the needed permission for the command\n2) The user you want to mute/ban/kick is above Hermes"
                            }.WithCurrentTimestamp();
                            try
                            {
                                await msg.Author.SendMessageAsync(embed: ella.Build());
                            }
                            catch (Exception)
                            {
                                await msg.Channel.SendMessageAsync(embed: ella.Build());
                            }

                            return;
                        }
                        else if (result.Exception.GetType() == typeof(AggregateException) &&
                                 result.Exception.InnerException.GetType() == typeof(ArgumentException))
                        {
                            var embed = new EmbedBuilder
                            {
                                Title = "That operation is not allowed!",
                                Description = "You cannot add or remove the `@everyone` role from a user",
                                Color = Color.Red
                            }.WithCurrentTimestamp();
                            await msg.Channel.SendMessageAsync(embed: embed.Build());
                            return;
                        }

                        var emb = new EmbedBuilder
                        {
                            Color = Color.Red,
                            Title =
                                $"**An error occured in <#{msg.Channel.Id}> of Guild (ID: {(msg.Channel as SocketGuildChannel).Guild.Id})**",
                            Description =
                                "We are on towards fixing it! In case of any problem, DM <@701029647760097361> or <@615873008959225856>" +
                                $"\nRefer to the below error message: ```{string.Join("", result.Exception.Message.Take(1000))}```"
                        }.WithCurrentTimestamp();
                        await msg.Channel.SendMessageAsync(embed: emb.Build());
                        await Client.GetUser(701029647760097361).SendMessageAsync(embed: emb
                            .WithDescription(
                                "We are on towards fixing it! In case of any problem, DM <@701029647760097361> or <@615873008959225856>" +
                                $"\nRefer to the below error message: ```{string.Join("", result.Exception.ToString())}```")
                            .Build());
                        break;
                    case CommandStatus.MissingGuildPermission:
                        await msg.Channel.SendMessageAsync("", false, new EmbedBuilder
                        {
                            Title = "**:lock: You're Missing Permissions :lock:**",
                            Color = Color.Red,
                            Description =
                                $"Hey {msg.Author.Mention}, you're missing these permissions:\n{result.ResultMessage}"
                        }.WithCurrentTimestamp().Build());
                        break;
                    case CommandStatus.NotEnoughParams or CommandStatus.InvalidParams:
                        var pref = await SqliteClass.PrefixGetter((msg.Channel as SocketGuildChannel).Guild.Id);
                        await msg.Channel.SendMessageAsync("", false, new EmbedBuilder
                        {
                            Title = "**That isn't how to use that command**",
                            Color = Color.Red,
                            Description =
                                $"Do `{pref}help {msg.Content.Split(' ')[0].Remove(0, pref.Length)}` to know how!"
                        }.WithCurrentTimestamp().Build());
                        break;
                    case CommandStatus.NotFound:
                        break;
                    case CommandStatus.ServerNotPremium:
                        await msg.Channel.SendMessageAsync("", false,
                            new EmbedBuilder
                            {
                                Title = "Your server isn't Premium",
                                Description =
                                    "Support us on [Patreon](https://patreon.com/rolexbot) to make this server a Premium server!",
                                Url = "https://patreon.com/rolexbot",
                                Color = Color.Red
                            }.WithCurrentTimestamp().Build());
                        break;
                    case CommandStatus.OnCooldown:
                        await msg.AddReactionAsync(Cooldown);
                        break;
                    default:
                        await Client.GetUser(701029647760097361).SendMessageAsync(
                            $"See kid Idk what happened but here it is {result.Result}\n{result.ResultMessage}\n{result.Exception}");
                        break;
                }
            }
            catch
            {
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
#pragma warning disable IDE0019 // Use pattern matching
            var msg = s as SocketUserMessage;
#pragma warning restore IDE0019 // Use pattern matching
            try
            {
                if (msg == null) return;
                if (msg.Channel.GetType() == typeof(SocketDMChannel)) return;
                var ca = msg.Content.ToCharArray();
                if (ca.Length == 0) return;
                var context = new ShardedCommandContext(Client, msg);
                var prefu = await SqliteClass.PrefixGetter(context.Guild.Id);
                try
                {
                    if (context.Client.CurrentUser != null && (msg.Content == $"<@{context.Client.CurrentUser.Id}>" ||
                                                               msg.Content == $"<@!{context.Client.CurrentUser.Id}>"))
                    {
                        await context.Message.Channel.SendMessageAsync("", false, new EmbedBuilder
                            {
                                Title = "Hi! I am Hermes",
                                Description =
                                    $"The prefix of your favourite role editor bot is {prefu}\nTo see documentation, come up [here](https://tiny.cc/rolexgit)",
                                Color = CommandModuleBase.Blurple,
                                ThumbnailUrl = context.Client.CurrentUser.GetAvatarUrl()
                            }.WithCurrentTimestamp().Build()
                        );
                        return;
                    }
                }
                catch
                {
                }

                if (msg.Content.Length <= prefu.Length) return;
                if (msg.Content[..prefu.Length] == prefu)
                    if (!context.User.IsBot)
                        new Thread(async () =>
                        {
                            try
                            {
                                var x = await _service.ExecuteAsync(context, prefu);
                                await HandleCommandResult(x, msg, prefu);
                                Console.WriteLine(context.User.Username + ": " + x.Result + " in channel " +
                                                  context.Channel.Name + " of guild " + context.Guild.Name);
                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    await Client.GetUser(701029647760097361)
                                        .SendMessageAsync(
                                            $"There was an error in {(msg.Channel as SocketGuildChannel).Guild.Name}\n{ex}");
                                }
                                catch
                                {
                                    // i hope this doesnt get hit :
                                }
                            }
                        }).Start();
            }
            catch (Exception e)
            {
                Console.WriteLine($"We have encountered an error {e}");
                await Client.GetUser(701029647760097361)
                    .SendMessageAsync($"There was an error in {(msg.Channel as SocketGuildChannel).Guild.Name}\n{e}");
                await Task.Delay(2000);
            }
        }

        private enum RequirementType
        {
            RoleReq,
            WeeklyReq,
            AmariLevelReq
        }
    }
}
