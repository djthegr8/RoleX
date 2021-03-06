using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using RoleX.Modules.Services;

namespace RoleX
{
    internal class Program
    {
        private readonly static string fpath = string.Join(Path.DirectorySeparatorChar, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Split(Path.DirectorySeparatorChar).SkipLast(1)) + Path.DirectorySeparatorChar + "Data" + Path.DirectorySeparatorChar + "token.txt";
        public static string token = File.ReadAllLines(fpath)[0];
        public static void Main(string[] _)
        {
            new Program().MainlikeAsync().GetAwaiter().GetResult();
        }
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
        public static DiscordShardedClient Client;
        public static DiscordRestClient CL2;
        public static CustomCommandService _service = new(new Settings());

        public async Task MainlikeAsync()
        {
            //Console.WriteLine("The list of databases on this server is: ");
            //foreach (var db in dbList)
            //{

            //    Console.WriteLine(db);
            //}
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            Client = new DiscordShardedClient(new DiscordSocketConfig { AlwaysDownloadUsers = true, LargeThreshold = 250, GuildSubscriptions = true, TotalShards = 3 });
            CL2 = new DiscordRestClient(new DiscordSocketConfig { AlwaysDownloadUsers = true, LargeThreshold = 250, GuildSubscriptions = true });
            Client.Log += Log;

            Client.MessageReceived += HandleCommandAsync;

            Client.JoinedGuild += HandleGuildJoinAsync;

            Client.LeftGuild += LeftGuildAsync;

            Client.ShardReady += HandleReadyAsync;

            Client.UserJoined += AltAlertAsync;

            Client.ReactionAdded += HandleReactionAsync;

            var __ = new Timer(async _ =>
            {
                if (Client.LoginState != LoginState.LoggedIn)
                {
                    return;
                }
                // Reminder thingies
                var currTime = DateTime.UtcNow;
                var allRems = await SqliteClass.GetReminders($"Select * from reminders where Finished = 0 and Time = \"{currTime:u}\"");
                if (allRems.Count > 0)
                {
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
                }
                // Giveaway thingies
                /*
                var logs = await SqliteClass.GetGiveaways("SELECT * from giveaways WHERE Running = 1;");
                if (logs.Count > 0)
                {
                    if (logs.Any(f =>
                        f.EndingTime.Hour == currTime.Hour && f.EndingTime.Minute == currTime.Minute &&
                        f.EndingTime.Second == currTime.Second))
                    {
                        var whereGiveaways = logs.Where(f =>
                            f.EndingTime.Hour == currTime.Hour && f.EndingTime.Minute == currTime.Minute &&
                            f.EndingTime.Second == currTime.Second);
                        foreach (var k in whereGiveaways)
                        {
                            await SqliteClass.GiveawayRemover(k);
                            var guild = Client.GetGuild(k.GuildID);
                            var channel = guild?.GetTextChannel(k.ChannelID);
                            var msg = Client.GetGuild(k.GuildID)?.GetTextChannel(k.ChannelID)
                                ?.GetMessageAsync(k.MessageID);
                            if (msg == null) return;
                            if (await msg is not IUserMessage mymsg) return;
                            
                            var noc = mymsg.GetReactionUsersAsync(new Emoji("🎉"), int.MaxValue);
                                if (noc == null) return;
                                var allWhoReacted = await noc.FlattenAsync();
                            if (guild == null) return;
                            var allWhoReactedButDidntLeave = allWhoReacted
                                .Where(user => guild.GetUser(user.Id) is not null && !user.IsBot)
                                .Select(o => guild.GetUser(o.Id));
                            var andAllWhoMetRequirements = allWhoReactedButDidntLeave.Where(kden =>
                                k.RoleReqs.All(role => kden.Roles.Any(m => m.Id == role.Id))).ToList();

                            if (k.WeeklyAmariRequirement != 0 || k.AmariLevelRequirement != 0)
                            {
                                do
                                {
                                    await Task.Delay(5000);
                                } while (!(lastAmariRequest.AddSeconds(5).CompareTo(DateTime.UtcNow) < 0));

                                lastAmariRequest = DateTime.UtcNow;
                                var amariString = await GetAsync($"http://litochee.com:3000/api/guild/{k.GuildID}");

                                var obj = JsonConvert.DeserializeObject<AmariWeeklyParser>(amariString);
                                foreach (var ReactingUser in andAllWhoMetRequirements)
                                {
                                    var user = obj.data.FirstOrDefault(
                                        i => i.userID.ToString() == ReactingUser.Id.ToString());
                                    if (k.AmariLevelRequirement != 0)
                                    {

                                        if (user == null || user.uLevel < k.AmariLevelRequirement)
                                        {
                                            andAllWhoMetRequirements.Remove(ReactingUser);

                                        }
                                    }

                                    if (k.WeeklyAmariRequirement == 0) continue;
                                    if (user == null || int.Parse(user.weeklyPoints) < k.WeeklyAmariRequirement)
                                    {
                                        andAllWhoMetRequirements.Remove(ReactingUser);

                                    }

                                }

                                Random rnd = new();
                                string mentions = "";
                                if (k.Winners == 0 || !andAllWhoMetRequirements.Any())
                                {
                                    mentions = "nobody";
                                }
                                else
                                {
                                    for (int i = 0; i < k.Winners; i++)
                                    {
                                        var usr = andAllWhoMetRequirements.ElementAt(rnd.Next(0,
                                            andAllWhoMetRequirements.Count() - 1));
                                        await usr.SendMessageAsync("", false, new EmbedBuilder
                                        {
                                            Title = $"You have won **{k.Title}** in {guild.Name}!",
                                            Description = $"DM <@{k.StarterID}> for your prize",
                                            Color = CommandModuleBase.Blurple,
                                            Url =
                                                $"https://discord.com/channels/{k.GuildID}/{k.ChannelID}/{k.MessageID}"
                                        }.WithCurrentTimestamp().Build());
                                        mentions += mentions == "" ? usr.Mention : $", {usr.Mention}";

                                    }
                                }
                                await mymsg.ModifyAsync(msgprop =>
                                {
                                    msgprop.Content = mymsg.Content.Replace("GIVEAWAY", "GIVEAWAY ENDED");
                                    var existingEmbed = mymsg.Embeds.First();
                                    msgprop.Embed = new EmbedBuilder
                                    {
                                        Title = existingEmbed.Title,
                                        Description = existingEmbed.Description.Split('\n')[0] + "\n" + $"Winner(s): {mentions}\n" +
                                                      string.Join('\n', existingEmbed.Description.Split('\n').Skip(2)),
                                        Color = existingEmbed.Color
                                    }.WithCurrentTimestamp().Build();
                                });
                                await mymsg.Channel.SendMessageAsync(
                                    $"Congratulations {mentions}. You have won {k.Title}");
                                var strt = Client.GetUser(k.StarterID);
                                if (strt == null) return;
                                await strt.SendMessageAsync("", false, new EmbedBuilder
                                {
                                    Title = "Your giveaway ended.",
                                    Description = $"Your winner(s) are {mentions}",
                                    Color = CommandModuleBase.Blurple,
                                    Url = $"https://discord.com/channels/{k.GuildID}/{k.ChannelID}/{k.MessageID}"
                                }.WithCurrentTimestamp().Build());
                            }
                        }

                    }
                    else {
                        logs.ForEach(async k =>
                        {
                            if (k.EndingTime.CompareTo(currTime) < 0)
                            {
                                await SqliteClass.GiveawayRemover(k);
                                return;
                            }
                            var guild = Client.GetGuild(k.GuildID);
                            var channel = guild?.GetTextChannel(k.ChannelID);
                            var msg = Client.GetGuild(k.GuildID)?.GetTextChannel(k.ChannelID)
                                ?.GetMessageAsync(k.MessageID);
                            if (msg == null) return;
                            if (await msg is not IUserMessage mymsg) return;
                            await mymsg.ModifyAsync(msgprop =>
                            {
                                var existingEmbed = mymsg.Embeds.First();
                                try
                                {
                                    var es = existingEmbed.Description.Split('\n')[1].Replace("Time left:", "").Replace(" ", "").Replace("and", "").Replace("*", "");

                                    if (es.Contains("hour(s)"))
                                    {
                                        var pH = es.IndexOf("hour(s)", StringComparison.Ordinal);
                                        var numHours = string.Join("", es.Take(pH));
                                        if ((k.EndingTime - currTime).Hours > int.Parse(numHours)) return;
                                        if ((k.EndingTime - currTime).Hours == int.Parse(numHours))
                                        {
                                            es = es.Replace("hour(s)", "");
                                            var pM = es.IndexOf("minutes", StringComparison.Ordinal);
                                            var numMinutes = string.Join("", es.Take(pM));
                                            if ((k.EndingTime - currTime).Minutes > int.Parse(numMinutes)) return;
                                        }
                                    }
                                    else if (es.Contains("minutes"))
                                    {
                                        var pM = es.IndexOf("minutes", StringComparison.Ordinal);
                                        var numMinutes = string.Join("", es.Take(pM));
                                        if ((k.EndingTime - currTime).Minutes > int.Parse(numMinutes)) return;
                                        if ((k.EndingTime - currTime).Minutes == int.Parse(numMinutes))
                                        {
                                            es = es.Replace("minutes", "");
                                            var pS = es.IndexOf("seconds", StringComparison.Ordinal);
                                            var numSeconds = string.Join("", es.Take(pS));
                                            if ((k.EndingTime - currTime).Seconds > int.Parse(numSeconds)) return;
                                        }
                                    }
                                    else
                                    {
                                        int pS = es.IndexOf("seconds", StringComparison.Ordinal);

                                        var numSeconds = string.Join("", es.Take(pS));
                                        if ((k.EndingTime - currTime).Seconds > int.Parse(numSeconds))
                                        {
                                            return;
                                        }
                                    }
                                    msgprop.Embed = new EmbedBuilder
                                    {
                                        Title = existingEmbed.Title,
                                        Description = existingEmbed.Description.Split('\n')[0] +
                                                      $"\nTime left: {(((k.EndingTime - currTime).TotalHours >= 1) ? $"**{(k.EndingTime - currTime).Hours}** hour(s) and **{(k.EndingTime - currTime).Minutes}** minutes" : ((k.EndingTime - currTime).Minutes >= 1 ? $"**{(k.EndingTime - currTime).Minutes}** minutes and **{(k.EndingTime - currTime).Seconds}** seconds" : $"**{(k.EndingTime - currTime).Seconds}** seconds"))}" + "\n" + string.Join("\n", existingEmbed.Description.Split('\n').Skip(2)),
                                        Color = existingEmbed.Color,
                                        ThumbnailUrl = existingEmbed.ToEmbedBuilder().ThumbnailUrl,
                                        Timestamp = existingEmbed.Timestamp,
                                        Footer = new EmbedFooterBuilder { Text = existingEmbed.ToEmbedBuilder().Footer.Text }
                                    }.Build();
                
                                }
                                catch
                                { // ignore
                                }
                            });

                        });
            }
                }*/
            }, null, 0, 1000);

            Client.GuildMemberUpdated += async (previous, later) =>
            {
                try
                {
                    new Thread(async () =>
                    {
                        if (previous.Status != later.Status && later.Status != UserStatus.Offline && await SqliteClass.TrackCdAllUlongIDs($"select UserID from track_cd where TUserID = {later.Id};") != new List<ulong>())
                        {
                            var lis = await SqliteClass.TrackCdAllUlongIDs($"select UserID from track_cd where TUserID = {later.Id};");
                            foreach (var user in lis)
                            {
                                await Client.GetUser(user)
                                            .SendMessageAsync($"<@{later.Id}> is now {later.Status}! Chat with them now!");
                                await SqliteClass.Track_CDRemover(user, later.Id);
                            }
                        }
                    }).Start();
                }
                catch { }
            };
            await CL2.LoginAsync(TokenType.Bot, token);
            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();
            await Client.SetGameAsync("Supervising Roles!");
            await Task.Delay(-1);
        }

        public DateTime lastAmariRequest = DateTime.MinValue;

        private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            try
            {
                new Thread(async () =>
                {
                    try
                    {
                        var msgid = arg1.Id;
                        var chnlId = arg2.Id;
                        /*
                        if (arg3.Emote.Name == "🎉" && arg2 is SocketTextChannel channel && arg3.User.IsSpecified &&
                            !arg3.User.Value.IsBot)
                        {
                            var gld = channel.Guild;
                            var mes = await (arg2 as SocketTextChannel).GetMessageAsync(msgid);
                            if (mes == null) return;
                            var log = (await SqliteClass.GetGiveaways(
                                    $"SELECT * from giveaways WHERE ChannelID = {chnlId} AND MessageID = {msgid} AND GuildID = {gld.Id} AND Running = 1;")
                                ).FirstOrDefault();
                            if (log != null)
                            {
                                var roleReqs = log.RoleRequirementString == ""
                                    ? new List<SocketRole>()
                                    : log.RoleRequirementString.Split('|')
                                        .Select(whatever => gld.GetRole(ulong.Parse(whatever)));
                                SocketGuildUser ReactingUser = gld.GetUser(arg3.UserId);
                                if (ReactingUser != null)
                                {
                                    var all = roleReqs.Any()
                                        ? roleReqs.All(f => ReactingUser.Roles.Any(k => k.Id == f.Id))
                                        : true;
                                    if (!all)
                                    {
                                        var rrw = roleReqs.Where(f => ReactingUser.Roles.All(k => k.Id != f.Id));
                                        await DontMeetRequirements(arg3, mes, ReactingUser, log,
                                            $"Role `{rrw.First().Name}` {(rrw.Count() > 1 ? $" and {rrw.Count() - 1} others " : "")}are missing");
                                        return;
                                    }

                                    if (log.AmariLevelRequirement != 0 || log.WeeklyAmariRequirement != 0)
                                    {
                                        do
                                        {
                                            await Task.Delay(5000);
                                        } while (!(lastAmariRequest.AddSeconds(5).CompareTo(DateTime.UtcNow) < 0));

                                        lastAmariRequest = DateTime.UtcNow;
                                        var amariString =
                                            await GetAsync($"http://litochee.com:3000/api/guild/{gld.Id}");

                                        var obj = JsonConvert.DeserializeObject<AmariWeeklyParser>(amariString);
                                        var user = obj.data.FirstOrDefault(
                                            i => i.userID.ToString() == ReactingUser.Id.ToString());
                                        if (log.AmariLevelRequirement != 0)
                                        {

                                            if (user == null || user.uLevel < log.AmariLevelRequirement)
                                            {
                                                await DontMeetRequirements(arg3, mes, ReactingUser, log,
                                                    $"Amari Level Requirement {log.AmariLevelRequirement} is more than your current {user.uLevel} by {log.AmariLevelRequirement - user.uLevel}");
                                                return;

                                            }

                                        }

                                        if (log.WeeklyAmariRequirement != 0)
                                        {
                                            if (user == null ||
                                                int.Parse(user
                                                    .weeklyPoints) < log.AmariLevelRequirement)
                                            {
                                                await DontMeetRequirements(arg3, mes, ReactingUser, log,
                                                    $"Weekly Amari Requirement {log.WeeklyAmariRequirement} is more than your current {user.weeklyPoints} by {log.WeeklyAmariRequirement - int.Parse(user.weeklyPoints)}");
                                                return;

                                            }
                                        }
                                    }
                                }
                            }
                        }

                        */
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
                            if (usr.Roles.All(x => x.Id == role.Id)) await usr.AddRoleAsync(role);
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
                }).Start();
            }
            catch
            {
                // continue to ignore
            }
        }
        public async Task<string> GetAsync(string uri)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using var response = (HttpWebResponse)await request.GetResponseAsync();
            await using var stream = response.GetResponseStream();
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
        private enum RequirementType
        {
            RoleReq,
            WeeklyReq,
            AmariLevelReq
        }
        /*
        private static async Task DontMeetRequirements(SocketReaction arg3, IMessage smg, SocketGuildUser ReactingUser, gstart.Giveaway log, string Info = "")
        {
            await smg.RemoveReactionAsync(arg3.Emote, ReactingUser);
            await ReactingUser.SendMessageAsync("", false, new EmbedBuilder
            {
                Title = "You do not meet the requirements for the giveaway!",
                Description = $"The {Info}, which makes an unmet requirement for the `{log.Title}` giveaway!",
                Color = Color.Red
            }.WithCurrentTimestamp().Build());
        }*/

        private async Task LeftGuildAsync(SocketGuild arg)
        {
            try
            {
                await Client.GetUser(701029647760097361).SendMessageAsync($"I left {arg.Name}, a Guild of {arg.MemberCount} members.");
                await Client.GetUser(615873008959225856).SendMessageAsync($"I left {arg.Name}, a Guild of {arg.MemberCount} members.");
                await TopGG.topGGUPD(Client.Guilds.Count);
            }
            catch { }
        }

        private async Task AltAlertAsync(SocketGuildUser arg)
        {
            try
            {
                new Thread(async () =>
                {
                    var aca = await SqliteClass.AlertChanGetter(arg.Guild.Id);
                    if (aca != 0 && arg.CreatedAt.UtcDateTime.CompareTo(DateTime.UtcNow.AddMonths(Convert.ToInt32(-await SqliteClass.AltTimePeriodGetter(arg.Guild.Id)))) > 0)
                    {
                        var hopefullyValidChannel = arg.Guild.GetTextChannel(aca);
                        if (hopefullyValidChannel != null)
                        {
                            await hopefullyValidChannel.SendMessageAsync("", false, new EmbedBuilder { Title = "Suspicious User Detected!!!!!", Description = $"**Name:** <@{arg.Id}>\n**ID: **{arg.Id}\n**Date Created: ** `{arg.CreatedAt:D}`, which seems sus to me...", Color = Color.Red }.WithCurrentTimestamp().Build());
                        }
                    }
                }).Start();
            }
            catch { }
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
                {
                    if (idek.TimeS.CompareTo(DateTime.UtcNow) > 0)
                    {
                        await SqliteClass.ReminderFinished(idek);
                        Console.WriteLine($"Ignoring reminder {idek.Id}, was offline.");
                    }
                }
            }
            catch { }
        }

        private async Task HandleGuildJoinAsync(SocketGuild arg)
        {
            try
            {
                await TopGG.topGGUPD(Client.Guilds.Count);
                // <@701029647760097361> or <@615873008959225856>
                await Client.GetUser(701029647760097361).SendMessageAsync($"I joined {arg.Name}, a Guild of {arg.MemberCount} members.");
                await Client.GetUser(615873008959225856).SendMessageAsync($"I joined {arg.Name}, a Guild of {arg.MemberCount} members.");
                //try block so no errors :)
                try
                {
                    await Client.GetUser(701029647760097361).SendMessageAsync($"Here's an invite!\n{(await arg.GetInvitesAsync()).First()}");
                    await Client.GetUser(615873008959225856).SendMessageAsync($"Here's an invite!\n{(await arg.GetInvitesAsync()).First()}");
                }
                catch { }
                try
                {
                    await arg.CurrentUser.ModifyAsync(async idk => idk.Nickname = $"[{await SqliteClass.PrefixGetter(arg.Id)}] RoleX");
                }
                catch { }
            }
            catch { }
        }

        internal static async Task HandleCommandResult(CustomCommandService.ICommandResult result, SocketUserMessage msg, string prefi)
        {
            try
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
                            Description = $"The Command {msg.Content[prefi.Length..]} was used in {msg.Channel.Name} of {(msg.Channel as SocketTextChannel).Guild.Name} by {msg.Author.Username + "#" + msg.Author.Discriminator}",
                            Footer = new EmbedFooterBuilder()
                        };
                        eb.Footer.Text = "Command Autogen";
                        eb.Footer.IconUrl = Client.CurrentUser?.GetAvatarUrl();
                        try
                        {
                            if (Client == null) break;
                            var g = Client.GetGuild(755076971041652786);
                            if (g == null) break;
                            await g.GetTextChannel(758230822057934878).SendMessageAsync("", false, eb.Build());
                        }
                        catch
                        { // ignore
                        }

                        break;
                    case CommandStatus.BotMissingPermissions:
                        await msg.Channel.SendMessageAsync("", false, new EmbedBuilder
                        {
                            Title = $"I require the {result.ResultMessage} permission",
                            Description = $"For this command to run, we require the `{result.ResultMessage}` permission.\n To understand all our required permissions, run `{await SqliteClass.PrefixGetter((msg.Channel as SocketGuildChannel).Guild.Id)}setup`",
                            Color = Color.Red
                        }.WithCurrentTimestamp()
                        .Build()
                        );
                        break;
                    case CommandStatus.Error:
                        if (result.Exception.GetType().ToString() == "System.AggregateException" && result.Exception.InnerException.GetType().ToString() == "Discord.Net.HttpException")
                        {
                            EmbedBuilder ella = new EmbedBuilder
                            {
                                Color = Color.Red,
                                Title = "**I don't have permissions!!!**",
                                Description = "RoleX does not have the permission to do execute your command...\nThis may be because: \n1) You haven't given RoleX the needed permission for the command\n2) The user you want to mute/ban/kick is above RoleX"
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
                        EmbedBuilder emb = new EmbedBuilder
                        {
                            Color = Color.Red,
                            Title = $"**An error occured in <#{msg.Channel.Id}> of ${(msg.Channel as SocketGuildChannel).Guild.Id}**",
                            Description = "We are on towards fixing it! In case of any problem, DM <@701029647760097361> or <@615873008959225856>" + $"\nRefer to the below error message: ```{ string.Join("", result.Exception.Message.Take(1000))}```",
                        }.WithCurrentTimestamp();
                        await msg.Channel.SendMessageAsync(embed: emb.Build());
                        await Client.GetUser(701029647760097361).SendMessageAsync(embed: emb.WithDescription("We are on towards fixing it! In case of any problem, DM <@701029647760097361> or <@615873008959225856>" + $"\nRefer to the below error message: ```{ string.Join("", result.Exception.ToString())}```").Build());
                        break;
                    case CommandStatus.MissingGuildPermission:
                        await msg.Channel.SendMessageAsync("", false, new EmbedBuilder
                        {
                            Title = "**:lock: You're Missing Permissions :lock:**",
                            Color = Color.Red,
                            Description = $"Hey {msg.Author.Mention}, you're missing these permissions:\n{result.ResultMessage}"
                        }.WithCurrentTimestamp().Build());
                        break;
                    case CommandStatus.NotEnoughParams or CommandStatus.InvalidParams:
                        var pref = await SqliteClass.PrefixGetter((msg.Channel as SocketGuildChannel).Guild.Id);
                        await msg.Channel.SendMessageAsync("", false, new EmbedBuilder
                        {
                            Title = "**That isn't how to use that command**",
                            Color = Color.Red,
                            Description = $"Do `{pref}help {msg.Content.Split(' ')[0].Remove(0, pref.Length)}` to know how!"
                        }.WithCurrentTimestamp().Build());
                        break;
                    case CommandStatus.NotFound:
                        break;
                    default:
                        await Client.GetUser(701029647760097361).SendMessageAsync($"See kid Idk what happened but here it is {result.Result}\n{result.ResultMessage}\n{result.Exception}");
                        break;
                }
            }
            catch { }
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
            SocketUserMessage msg = s as SocketUserMessage;
#pragma warning restore IDE0019 // Use pattern matching
            try
            {
                if (msg == null) return;
                if (msg.Channel.GetType() == typeof(SocketDMChannel))
                {
                    return;
                }
                var ca = msg.Content.ToCharArray();
                if (ca.Length == 0) return;
                var context = new ShardedCommandContext(Client, msg);
                var prefu = await SqliteClass.PrefixGetter(context.Guild.Id);
                try
                {
                    if (context.Client.CurrentUser != null && (msg.Content == $"<@{context.Client.CurrentUser.Id}>" || msg.Content == $"<@!{context.Client.CurrentUser.Id}>"))
                    {
                        await context.Message.Channel.SendMessageAsync("", false, new EmbedBuilder
                        {
                            Title = "Hi! I am RoleX",
                            Description = $"The prefix of your favourite role editor bot is {prefu}\nTo see documentation, come up [here](https://tiny.cc/rolexgit)",
                            Color = CommandModuleBase.Blurple,
                            ThumbnailUrl = context.Client.CurrentUser.GetAvatarUrl()
                        }.WithCurrentTimestamp().Build()
                        );
                        return;
                    }
                }
                catch { }
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
                            catch (Exception ex)
                            {
                                try
                                {
                                    await Client.GetUser(701029647760097361).SendMessageAsync($"There was an error in {(msg.Channel as SocketGuildChannel).Guild.Name}\n{ex}");
                                }
                                catch { }
                            }
                        }).Start();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"We have encountered an error {e}");
                await Client.GetUser(701029647760097361).SendMessageAsync($"There was an error in {(msg.Channel as SocketGuildChannel).Guild.Name}\n{e}");
                await Task.Delay(2000);
            }
        }
    }
}