using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hermes.Utilities;
using MoreLinq.Extensions;

namespace Hermes.Modules.Services
{
    /// <summary>
    /// The Class to interface from.
    /// </summary>
    public class CommandModuleBase
    {
        public static readonly ulong[] devids = {
            701029647760097361,
            615873008959225856
        };
        /// <summary>
        /// Number of aliases allowed for non-premium users
        /// </summary>
        public static readonly ushort AllowedAliasesNonPremium = 20;
        /// <summary>
        /// Number of premium aliases allowed
        /// </summary>
        public static readonly ushort AllowedAliasesPremium = 50;

        private static readonly Random _random = new();

        public static Color Blurple => new (_random.Next(255), _random.Next(255), _random.Next(255));

        /// <summary>
        /// If the user has execute permission based on the <see cref="CustomCommandService.Settings.HasPermissionMethod"/>
        /// </summary>
        public static bool HasExecutePermission { get; set; }
        /// <summary>
        /// The Context of the current command
        /// </summary>
        public SocketCommandContext Context { get; internal set; }

        /// <summary>
        /// Contains all the help messages. Key is the command name, Value is the help message
        /// </summary>
        public static Dictionary<string, string> CommandHelps { get; internal set; }

        /// <summary>
        /// Contains all the help messages. Key is the command name, Value is the Command Description
        /// </summary>
        public static Dictionary<string, string> CommandDescriptions { get; internal set; }
        /// <summary>
        /// The superlist with all the commands
        /// </summary>
        public static List<CustomCommandService.Commands> Commands { get; internal set; }
        public static List<ICommands> ReadCurrentCommands(string prefix)
        {
            List<ICommands> cmds = new List<ICommands>();
            foreach (var cmd in Commands)
            {
                var c = new CustomCommandService.Commands
                {
                    CommandName = cmd.CommandName,
                    CommandDescription = cmd.CommandDescription?.Replace("(PREFIX)", prefix),
                    CommandHelpMessage = cmd.CommandHelpMessage?.Replace("(PREFIX)", prefix),
                    Prefixes = cmd.Prefixes,
                    RequiresPermission = cmd.RequiresPermission,
                    RequireUsrPerm = cmd.RequireUsrPerm,
                    ModuleName = cmd.ModuleName,
                    Alts = cmd.Alts
                };
                cmds.Add(c);
            }
            return cmds;
        }
        public async Task<GuildEmote> GetEmote(string str, SocketGuild Guild = null)
        {
            Guild ??= Context.Guild;
            var replstr = str.Replace("a:", "").Replace("<", "").Replace(">", "").Replace(":", "");
            Console.WriteLine(replstr);
            if (Guild.Emotes.Any(x => String.Equals(replstr, x.Name, StringComparison.CurrentCultureIgnoreCase))) return Guild.Emotes.First(x => String.Equals(replstr, x.Name, StringComparison.CurrentCultureIgnoreCase));
            Console.WriteLine(replstr);
            try
            {
                var resultString = ulong.Parse(Regex.Match(replstr, @"\d+").Value);

                if (resultString == 0 || await Context.Guild.GetEmoteAsync(resultString) == null)
                {
                    return null;
                }
                return await Guild.GetEmoteAsync(resultString);
            }
            catch { return null; }

        }
        public SocketGuildChannel GetChannel(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            var regex = new Regex(@"(\d{18}|\d{17})");
            if (regex.IsMatch(name))
            {
                var u = Context.Guild.GetChannel(ulong.Parse(regex.Match(name).Groups[1].Value));
                return u is SocketCategoryChannel ? null : u;
            }
            if (ulong.TryParse(name, out var res))
            {
                var x = Context.Guild?.Channels?.FirstOrDefault(ch => ch.Id == res);
                return x is SocketCategoryChannel ? null : x;
            }
            else
            {
                var x = Context.Guild?.Channels?.FirstOrDefault(ch => ch.Name.ToLower().StartsWith(name.ToLower()));
                return x is SocketCategoryChannel ? null : x;
            }
        }
        public SocketCategoryChannel GetCategory(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            var regex = new Regex(@"(\d{18}|\d{17})");
            if (!regex.IsMatch(name))
                return ulong.TryParse(name, out var res)
                    ? Context.Guild.CategoryChannels.FirstOrDefault(x => x.Id == res)
                    : Context.Guild.CategoryChannels.FirstOrDefault(x => x.Name.ToLower().StartsWith(name.ToLower()));
            var u = Context.Guild.GetCategoryChannel(ulong.Parse(regex.Match(name).Groups[1].Value));
            return u;
        }
        public async Task<SocketGuildUser> GetUser(string user)
        {
            if (user.Length < 3) return null;
            await Context.Guild.DownloadUsersAsync();
            var regex = new Regex(@"(\d{18}|\d{17})");
            if (regex.IsMatch(user))
            {
                var u = Context.Guild.GetUser(ulong.Parse(regex.Match(user).Groups[1].Value));
                return u;
            }

            user = user.ToLower();
            if (Context.Message.MentionedUsers.Any())
            {
                return Context.Message.MentionedUsers.First() as SocketGuildUser;
            }

            if (Context.Guild.Users.Any(x => x.Username.ToLower().StartsWith(user)))
            {
                return Context.Guild.Users.First(x => x.Username.ToLower().StartsWith(user));
            }
            return Context.Guild.Users.Any(x => x.ToString().ToLower().StartsWith(user)) ? Context.Guild.Users.First(x => x.ToString().ToLower().StartsWith(user)) : Context.Guild.Users.FirstOrDefault(x => x.Nickname != null && x.Nickname.ToLower().StartsWith(user));
        }

        public async Task<IUser> GetBannedUser(string uname)
        {
            var alr = await Context.Guild.GetBansAsync();
            var regex = new Regex(@"(\d{18}|\d{17})");
            return regex.IsMatch(uname) ? alr.FirstOrDefault(aa => aa.User.Id == ulong.Parse(uname))?.User : alr.FirstOrDefault(x => x.User.Username.ToLower().Contains(uname.ToLower()))?.User;
        }
        public static OverwritePermissions GetOP(ChannelPermission cp, PermValue pv, OverwritePermissions eop)
        {
            var x = cp switch
            {
                ChannelPermission.AddReactions => eop.Modify(addReactions: pv),
                ChannelPermission.AttachFiles => eop.Modify(attachFiles: pv),
                ChannelPermission.Connect => eop.Modify(connect: pv),
                ChannelPermission.CreateInstantInvite => eop.Modify(createInstantInvite: pv),
                ChannelPermission.DeafenMembers => eop.Modify(deafenMembers: pv),
                ChannelPermission.EmbedLinks => eop.Modify(embedLinks: pv),
                ChannelPermission.ManageChannels => eop.Modify(manageChannel: pv),
                ChannelPermission.ManageMessages => eop.Modify(manageMessages: pv),
                ChannelPermission.ManageRoles => eop.Modify(manageRoles: pv),
                ChannelPermission.ManageWebhooks => eop.Modify(manageWebhooks: pv),
                ChannelPermission.MentionEveryone => eop.Modify(mentionEveryone: pv),
                ChannelPermission.MoveMembers => eop.Modify(moveMembers: pv),
                ChannelPermission.MuteMembers => eop.Modify(muteMembers: pv),
                ChannelPermission.ReadMessageHistory => eop.Modify(readMessageHistory: pv),
                ChannelPermission.ViewChannel => eop.Modify(viewChannel: pv),
                ChannelPermission.SendMessages => eop.Modify(sendMessages: pv),
                ChannelPermission.SendTTSMessages => eop.Modify(sendTTSMessages: pv),
                ChannelPermission.Speak => eop.Modify(speak: pv),
                ChannelPermission.UseExternalEmojis => eop.Modify(useExternalEmojis: pv),
                ChannelPermission.UseVAD => eop.Modify(useVoiceActivation: pv),
                ChannelPermission.PrioritySpeaker => eop,
                _ => throw new ArgumentOutOfRangeException(nameof(cp), cp, null)
            };
            return x;
        }

        public static GuildPermissions EditPerm(SocketRole roleA, GuildPermission perm, bool add = true)
        {
            Console.WriteLine(roleA.Name);
            Console.WriteLine(perm);
            Console.WriteLine(add);
            var gp = perm switch
            {
                GuildPermission.ViewGuildInsights => roleA.Permissions.Modify(viewGuildInsights:add),
                GuildPermission.AddReactions => roleA.Permissions.Modify(addReactions: add),
                GuildPermission.Administrator => roleA.Permissions.Modify(administrator: add),
                GuildPermission.AttachFiles =>
                    roleA.Permissions.Modify(attachFiles: add)
                ,
                GuildPermission.BanMembers =>
                    roleA.Permissions.Modify(banMembers: add)
                ,
                GuildPermission.ChangeNickname =>
                    roleA.Permissions.Modify(changeNickname: add)
                ,
                GuildPermission.Connect =>
                    roleA.Permissions.Modify(connect: add)
                ,
                GuildPermission.CreateInstantInvite =>
                    roleA.Permissions.Modify(createInstantInvite: add)
                ,
                GuildPermission.DeafenMembers =>
                    roleA.Permissions.Modify(deafenMembers: add)
                ,
                GuildPermission.EmbedLinks =>
                    roleA.Permissions.Modify(embedLinks: add)
                ,
                GuildPermission.KickMembers =>
                    roleA.Permissions.Modify(kickMembers: add)
                ,
                GuildPermission.ManageChannels =>
                    roleA.Permissions.Modify(manageChannels: add)
                ,
                GuildPermission.ManageEmojis =>
                    roleA.Permissions.Modify(manageEmojis: add)
                ,
                GuildPermission.ManageGuild =>
                    roleA.Permissions.Modify(manageGuild: add)
                ,
                GuildPermission.ManageMessages =>
                    roleA.Permissions.Modify(manageMessages: add)
                ,
                GuildPermission.ManageNicknames =>
                    roleA.Permissions.Modify(manageNicknames: add)
                ,
                GuildPermission.ManageRoles =>
                    roleA.Permissions.Modify(manageRoles: add)
                ,
                GuildPermission.ManageWebhooks =>
                    roleA.Permissions.Modify(manageWebhooks: add)
                ,
                GuildPermission.MentionEveryone =>
                    roleA.Permissions.Modify(mentionEveryone: add)
                ,
                GuildPermission.MoveMembers =>
                    roleA.Permissions.Modify(moveMembers: add)
                ,
                GuildPermission.MuteMembers =>
                    roleA.Permissions.Modify(muteMembers: add)
                ,
                GuildPermission.PrioritySpeaker =>
                    roleA.Permissions.Modify(prioritySpeaker: add)
                ,
                GuildPermission.ReadMessageHistory =>
                    roleA.Permissions.Modify(readMessageHistory: add)
                ,
                GuildPermission.ViewChannel =>
                    roleA.Permissions.Modify(viewChannel: add)
                ,
                GuildPermission.SendMessages =>
                    roleA.Permissions.Modify(sendMessages: add)
                ,
                GuildPermission.SendTTSMessages =>
                    roleA.Permissions.Modify(sendTTSMessages: add)
                ,
                GuildPermission.Speak =>
                    roleA.Permissions.Modify(speak: add)
                ,
                GuildPermission.Stream =>
                    roleA.Permissions.Modify(stream: add)
                ,
                GuildPermission.UseExternalEmojis =>
                    roleA.Permissions.Modify(useExternalEmojis: add)
                ,
                GuildPermission.UseVAD =>
                    roleA.Permissions.Modify(useVoiceActivation: add)
                ,
                GuildPermission.ViewAuditLog =>
                    roleA.Permissions.Modify(viewAuditLog: add),

            };
            return gp;
        }
        public static Tuple<GuildPermission, bool> GetPermission(string perm)
        {
            perm = perm.Replace("admin", "administrator");
            return Enum.TryParse(perm, true, out GuildPermission Gp) ? new Tuple<GuildPermission, bool>(Gp, true) : new Tuple<GuildPermission, bool>(GuildPermission.AddReactions, false);
        }

        public static Tuple<ChannelPermission, bool> GetChannelPermission(string perm)
        {
            return Enum.TryParse(perm, true, out ChannelPermission Gp) ? new Tuple<ChannelPermission, bool>(Gp, true) : new Tuple<ChannelPermission, bool>(ChannelPermission.AddReactions, false);
        }
        public SocketRole GetRole(string role)
        {
            if (role.Length < 3)
            {
                return Context.Guild.Roles.FirstOrDefault(k => k.Name.ToLower() == role);
            }
            var regex = new Regex(@"(\d{18}|\d{17})");
            if (regex.IsMatch(role))
            {
                var u = Context.Guild.GetRole(ulong.Parse(regex.Match(role).Groups[1].Value));
                return u is {IsEveryone: false} ? u : null;
            }

            if (Context.Guild.Roles.Any(x => !x.IsEveryone && x.Name.ToLower().StartsWith(role.ToLower())))
                return Context.Guild.Roles.First(x => x.Name.ToLower().StartsWith(role.ToLower()));
            return Context.Guild.Roles.Any(x => !x.IsEveryone && x.Name.ToLower().StartsWith(role.ToLower())) ? Context.Guild.Roles.First(x => x.Name.ToLower().StartsWith(role.ToLower())) : null;
        }
        /// <summary>
        /// Takes an Context, and sends a message to the channel it was sent in, while customizing the embed to fit parameters.
        /// </summary>
        /// <param name="message">The actual message</param>
        /// <param name="isTTS">Whether to speak the <paramref name="message"/> or not</param>
        /// <param name="embed">A <c>Discord.EmbedBuilder</c> for editing and making it work</param>
        /// <param name="options">Just a useless param to me ig</param>
        /// <returns>The msg</returns>
        public async Task<IUserMessage> ReplyAsync(string message = null, bool isTTS = false, EmbedBuilder embed = null, MessageComponent mcom = null, RequestOptions options = null)
        {
            var msgcontent = Context.Message.Content;
            if (!msgcontent.Split(' ')[0].Contains("alias"))
            {
                var alss = await SqliteClass.GuildAliasGetter(Context.Guild.Id);
                foreach (var (aliasName, aliasContent) in alss)
                {
                    if (!msgcontent.Contains(aliasName)) continue;
                    msgcontent = msgcontent.Replace(aliasName, aliasContent);
                    break;

                }
            }
            if (msgcontent.EndsWith("-q"))
            {
                return null;
            }
            if (message?.Length >= 2000)
            {
                const string filePath = "message.txt";
                await using (var sw = File.CreateText(filePath))
                {
                    await sw.WriteLineAsync(message);
                }
                await Context.Channel.SendFileAsync(filePath);
                message = null;
            }
            if (message == null && embed == null)
            {
                return null;
            }
            // Embed editing time!
            if (embed?.Description?.Length >= EmbedBuilder.MaxDescriptionLength ||
                embed?.Title?.Length >= EmbedBuilder.MaxTitleLength ||
                embed?.Fields?.Count >= EmbedBuilder.MaxFieldCount ||
                embed?.Length >= EmbedBuilder.MaxEmbedLength
            )
            {
                //Require some editing eh...
                if (embed?.Description?.Length >= EmbedBuilder.MaxDescriptionLength)
                {
                    //devids.Select(async x => await (await GetUser(x.ToString())).SendMessageAsync($"yet another too long description. ```{Context.Message.Content}```"));
                    const int chunkSize = 2000;
                    var chunks = BatchExtension.Batch(embed.Description, chunkSize).Select(r => new string(r.ToArray()));
                    IUserMessage xyz = null;
                    try
                    {
                        xyz = await Context.Channel.SendMessageAsync(message, isTTS, component:mcom);
                    }
                    catch
                    {
                        // ignore
                    }
                    foreach (var chunk in chunks)
                    {
                        xyz = await Context.Channel.SendMessageAsync("", false, embed.WithDescription(chunk).Build());
                        await Task.Delay(500);
                    }
                    return xyz;
                }

                if (embed?.Title?.Length >= EmbedBuilder.MaxTitleLength)
                {
                    return await Context.Channel.SendMessageAsync(embed: embed.WithTitle(embed.Title.Substring(0, EmbedBuilder.MaxTitleLength - 5) + "...").Build(), component:mcom);
                }
                if (embed?.Fields?.Count >= 6)
                {
                    var pM = new PaginatedMessage(PaginatedAppearanceOptions.Default, Context.Channel);
                    var lofb = embed.Fields;
                    pM.SetPages(embed.Description, lofb, 5);
                    await pM.Resend();
                }
                else if (embed?.Length >= EmbedBuilder.MaxEmbedLength)
                {
                    IUserMessage xyz = null;
                    try
                    {
                        xyz = await Context.Channel.SendMessageAsync(message, isTTS, component: mcom);
                    }
                    catch { }
                    var batches = embed.Fields.Batch(10);
                    foreach (var batch in batches)
                    {
                        embed.Fields = batch.ToList();
                        xyz = await Context.Channel.SendMessageAsync(message, isTTS, embed.Build());
                        await Task.Delay(500);

                    }
                    return xyz;
                }
            }
            if (embed is { Color: null }) embed.Color = Blurple;
            var here = await Context.Channel.SendMessageAsync(message, isTTS, embed?.Build(),  options, component: mcom).ConfigureAwait(false);
            if (await SqliteClass.PremiumOrNot(Context.Guild.Id)) return here;
            var ranjom = new Random();
            var irdk = ranjom.Next(8);
            if (irdk != 1 || await TopGG.HasVoted(Context.User.Id)) return here;
            var idk = ranjom.Next(2);
            if (idk == 1 || (await Program.Client.Rest.GetGuildsAsync()).Any(x => x.Id == 591660163229024287 && x.GetUserAsync(Context.User.Id) != null)) await Context.Channel.SendMessageAsync("", false, new EmbedBuilder { Title = "Vote for Hermes!", Url = "https://tiny.cc/rolexdsl", Description = "Support Hermes by [voting](http:/tiny.cc/rolexdsl) for it in top.gg!", ImageUrl = "https://media.discordapp.net/attachments/745266816179241050/808311320373624832/B22rOemKFGmIAAAAAElFTkSuQmCC.png", Color = Blurple }.WithCurrentTimestamp().Build());
            else await Context.Channel.SendMessageAsync("", false, new EmbedBuilder { Title = "Join our support server!", Url = "https://tiny.cc/rolexdsl", Description = "Support Hermes by [voting](http:/tiny.cc/rolexdsl) for it on top.gg!", Color = Blurple }.WithCurrentTimestamp().Build());
            return here;
        }

    }
}