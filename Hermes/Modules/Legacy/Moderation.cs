using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Public_Bot;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using static RoleX.modules.SqliteClass;
using GuildPermissions = Public_Bot.GuildPermissions;

namespace RoleX.modules
{
    [DiscordCommandClass("Moderation", "Basic Moderation for yer server!")]
    public class Moderator : CommandModuleBase
    {
        [RequiredBotPermission(GuildPermission.ManageNicknames)]
        [GuildPermissions(GuildPermission.ManageNicknames)]
        [DiscordCommand("nick", commandHelp = "nick @User <multi-word-string>", example ="nick @DJ001 Weird Dumbass")]
        [Alt("nickname")]
        public async Task Nick(params string[] args)
        {
            if (args.Length == 0 || args.Length == 1)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Insufficient Parameters!",
                    Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}nick <@User> <new-user-nickname>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (GetUser(args[0]) == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid user",
                    Description = $"`{args[0]}` could not be parsed as user!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var bchname = string.Join(' ', args.Skip(1));
            if (bchname.Length < 0 || bchname.Length > 32)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid user nickname",
                    Description = $"`{bchname}` is an invalid name, as it either ~ \n1) Contains non-allowed characters\n 2) Is too long",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var cha = await GetUser(args[0]);
            if (cha.Hierarchy >= (Context.User as SocketGuildUser).Hierarchy && cha.Id != Context.User.Id)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "They are above you",
                    Description = $"Respect 'em, dont change their nick smh",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (cha.Hierarchy >= Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Oops, that person is above me :(",
                    Description = $"I don't have sufficient permissions to ban them",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            await cha.ModifyAsync(i => i.Nickname = bchname);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Nickname Updated!!",
                Description = $"<@{cha.Id}> is now set!!!",
                Color = Blurple
            }.WithCurrentTimestamp());
            return;
        }
        [Alt("ml")]
        [GuildPermissions(GuildPermission.ManageGuild)]
        [DiscordCommand("modlogs", commandHelp = "modlogs <@user>", description = "Shows all modlogs of a user", example = "modlogs @WeirdMan")]
        public async Task MLogs(params string[] aa)
        {
            if (aa.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "Mention the user you wish to get modlogs of",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            IUser user;
            if (await GetUser(aa[0]) == null)
            {
                var regex = new Regex(@"(\d{18}|\d{17})");
                if (regex.IsMatch(aa[0]))
                {
                    var aadrc = new DiscordRestClient();
                    await aadrc.LoginAsync(TokenType.Bot, Program.token);
                    var aala = await aadrc.GetUserAsync(ulong.Parse(regex.Match(aa[0]).Groups[1].Value));
                    if (aala == null)
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "What user?",
                            Description = "That user isn't valid :(",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                        return;
                    }
                    user = aala;
                }
                else if (Context.Message.MentionedUsers.Any())
                {
                    user = Context.Message.MentionedUsers.First();
                }
                else
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "What user?",
                        Description = "That user isn't valid :(",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
            }
            else
            {
                user = await GetUser(aa[0]);
            }
            EmbedBuilder eb = new EmbedBuilder
            {
                Title = $"Modlogs for user {user.Username}#{user.Discriminator}",
                Color = Blurple,
                ThumbnailUrl = user.GetAvatarUrl(size: 64)
            };

            foreach (Infraction i in await GetUserModlogs(Context.Guild.Id, user.Id))
            {
                eb.AddField($"{Infraction.GetPunishment(i.Punishment)}", $"**Mod:** <@{i.ModeratorID}>\n**Date:** {i.Time.ToUniversalTime().ToShortDateString()}\n**Time: **{(i.Time.ToUniversalTime().TimeOfDay.Hours <= 12 ? i.Time.ToUniversalTime().TimeOfDay.Hours : i.Time.ToUniversalTime().TimeOfDay.Hours - 12)}:{i.Time.ToUniversalTime().TimeOfDay.Minutes} {(i.Time.ToUniversalTime().TimeOfDay.Hours < 12 ? "AM" : "PM")}\n**Reason:** {i.Reason}", true);
            }
            if (eb.Fields.Count == 0)
            {
                eb.Description = "They've been a good user! No modlogs :)";
            }
            await ReplyAsync(embed: eb.WithCurrentTimestamp());
        }
        [RequiredBotPermission(GuildPermission.ManageRoles)]
        [GuildPermissions(GuildPermission.ManageGuild)]
        [DiscordCommand("mute", description = "Mutes the given user", example = "mute @Dumbkid 5m For trying to ping everyone", commandHelp = "mute <@user> <time> <reason>")]
        public async Task Mute(params string[] args)
        {
            if (await MutedRoleIDGetter(Context.Guild.Id) == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "No muted role set",
                    Description = $"Set muted role by running `{await PrefixGetter(Context.Guild.Id)}mutedrole <create/@Role>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            bool isValidTime = false;
            TimeSpan ts;
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "Mention the user you wish to mute",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (args.Length >= 2)
            {
                isValidTime = args[1].Last() switch
                {
                    'm' or 'M' or 'd' or 'D' or 'Y' or 'y' or 's' or 'S' => true,
                    _ => false
                } && int.TryParse(string.Join("", args[1].SkipLast(1)), out int _);
                if (!isValidTime)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "The time parameter is invalid",
                        Description = $"Couldn't parse `{args[1]}` as time, see key below\n```s => seconds\nm => minutes\nh => hours\nd => days```",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
                else if (int.TryParse(string.Join("", args[1].SkipLast(1)), out int timezar))
                {
                    ts = args[1].Last() switch
                    {
                        'h' or 'H' => new TimeSpan(timezar, 0, 0),
                        'm' or 'M' => new TimeSpan(0, timezar, 0),
                        's' or 'S' => new TimeSpan(0, 0, timezar),
                        'd' or 'D' => new TimeSpan(timezar, 0, 0, 0),
                        //Non possible outcome but IDE is boss
                        _ => new TimeSpan()
                    };
                }
                else
                {
                    ts = TimeSpan.Zero;
                }
            }
            else
            {
                ts = TimeSpan.Zero;
            }
            if (await GetUser(args[0]) != null || Context.Message.MentionedUsers.Any())
            {
                var gUser = await GetUser(args[0]);
                if (gUser == null)
                {
                    gUser = Context.Message.MentionedUsers.First() as SocketGuildUser;
                }
                if (gUser.Hierarchy < (Context.User as SocketGuildUser).Hierarchy)
                {
                    if (gUser.Hierarchy >= Context.Guild.CurrentUser.Hierarchy)
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "Oops, that person is above me :(",
                            Description = $"I don't have sufficient permissions to ban them",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                        return;
                    }
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = $"{gUser.Username}#{gUser.Discriminator} Muted {(isValidTime ? $"for {ts.Days}d, {ts.Minutes}m and {ts.Seconds}s" : "indefinitely")}!",
                        Description = $"Reason: {(args.Length > 2 ? string.Join(' ', args.Skip(2)) : $"Requested by { Context.User.Username }#{Context.User.Discriminator}")}",
                        Color = Blurple
                    }.WithCurrentTimestamp());
                    try
                    {
                        await gUser.SendMessageAsync("", false, new EmbedBuilder
                        {
                            Title = "Oops, you were muted!",
                            Description = $"You were muted {(isValidTime ? $"for {ts.Days} days, {ts.Minutes} minutes and {ts.Seconds} seconds" : "indefinitely")} from **{Context.Guild.Name}** by {Context.User.Mention} {(args.Length > 1 ? $"Reason:{args[1]}" : "")}{(await AppealGetter(Context.Guild.Id) == "" ? "" : "\n[Click here to appeal](" + await AppealGetter(Context.Guild.Id))})",
                            Color = Color.Red
                        }.WithCurrentTimestamp().Build());
                    }
                    catch { }
                    string guildName = Context.Guild.Name;
                    await AddToModlogs(Context.Guild.Id, gUser.Id, Context.User.Id, Punishment.Mute, DateTime.Now, args.Length > 2 ? string.Join(' ', args.Skip(2)) : "");
                    await gUser.AddRoleAsync(Context.Guild.GetRole(await MutedRoleIDGetter(Context.Guild.Id)));
                    if (!isValidTime)
                    {
                        return;
                    }
                    Timer tmr = new Timer()
                    {
                        AutoReset = false,
                        Interval = ts.TotalMilliseconds,
                        Enabled = true
                    };
                    tmr.Elapsed += async (object send, ElapsedEventArgs arg) =>
                    {
                        try
                        {
                            await gUser.RemoveRoleAsync(Context.Guild.GetRole(await MutedRoleIDGetter(Context.Guild.Id)));
                            await gUser.SendMessageAsync($"**You have been unmuted on {guildName}**");
                        }
                        catch { }
                    };
                    return;
                }
                else if (gUser.Hierarchy == (Context.User as SocketGuildUser).Hierarchy)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = $"Seriously??",
                        Color = Color.Red,
                        ImageUrl = "https://cdn.discordapp.com/attachments/758922634749542420/760180089870090320/unknown.png"
                    }.WithCurrentTimestamp());
                    return;
                }
                else
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Not gonna happen",
                        Description = "That person is above you!?",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
            }
            else
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "That user isn't valid :(",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
        }
        [GuildPermissions(GuildPermission.BanMembers)]
        [DiscordCommand("ban", commandHelp = "ban <@user> <reason>", example = "ban @Scammer Scamming me friend", description = "Bans the specified user")]
        public async Task Banner(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "Mention the user you wish to ban",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (await GetUser(args[0]) != null || Context.Message.MentionedUsers.Any())
            {
                var gUser = await GetUser(args[0]);
                if (gUser == null)
                {
                    gUser = Context.Message.MentionedUsers.First() as SocketGuildUser;
                }
                if (gUser.Hierarchy < (Context.User as SocketGuildUser).Hierarchy)
                {
                    if (gUser.Hierarchy >= Context.Guild.CurrentUser.Hierarchy)
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "Oops, that person is above me :(",
                            Description = $"I don't have sufficient permissions to ban them",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                        return;
                    }
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = $"{gUser.Username}#{gUser.Discriminator} Banned Successfully!",
                        Description = $"Reason: {(args.Length > 1 ? string.Join(' ', args.Skip(1)) : $"Requested by { Context.User.Username }#{Context.User.Discriminator}")}",
                        Color = Blurple
                    }.WithCurrentTimestamp());
                    try
                    {
                        await gUser.SendMessageAsync("", false, new EmbedBuilder
                        {
                            Title = "Oops, you were banned!",
                            Description = $"You were banned from **{Context.Guild.Name}** by {Context.User.Mention} {(args.Length > 1 ? $"Reason:{args[1]}" : "")}\n[Click here to appeal]({(await AppealGetter(Context.Guild.Id) == "" ? "" : await AppealGetter(Context.Guild.Id))})",
                            Color = Color.Red
                        }.WithCurrentTimestamp().Build());
                    }
                    catch { }
                    await gUser.BanAsync(7, args.Length > 1 ? string.Join(' ', args.Skip(1)) : $"Requested by {Context.User.Username}#{Context.User.Discriminator}");
                    await AddToModlogs(Context.Guild.Id, gUser.Id, Context.User.Id, Punishment.Ban, DateTime.Now, args.Length > 1 ? string.Join(' ', args.Skip(1)) : $"");
                    return;
                }
                else if (gUser.Hierarchy == (Context.User as SocketGuildUser).Hierarchy)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = $"Seriously??",
                        Color = Color.Red,
                        ImageUrl = "https://cdn.discordapp.com/attachments/758922634749542420/760180089870090320/unknown.png"
                    }.WithCurrentTimestamp());
                    return;
                }
                else
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Not gonna happen",
                        Description = "That person is above you!?",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
            }
            else if (ulong.TryParse(args[0], out ulong ide))
            {
                var aadrc = new DiscordRestClient();
                await aadrc.LoginAsync(TokenType.Bot, Program.token);
                var aa = await aadrc.GetUserAsync(ide);
                if (aa == null)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "What user?",
                        Description = "Could not find specified user on Discord",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = $"{aa.Username}#{aa.Discriminator} Banned Successfully!",
                    Description = $"Reason: {(args.Length > 1 ? string.Join(' ', args.Skip(1)) : $"Requested by { Context.User.Username }#{Context.User.Discriminator}")}",
                    Color = Blurple
                }.WithCurrentTimestamp());
                try
                {
                    await aa.SendMessageAsync("", false, new EmbedBuilder
                    {
                        Title = "Oops, you were banned!",
                        Description = $"You were banned from **{Context.Guild.Name}** by {Context.User.Mention} {(args.Length > 1 ? $"Reason:{args[1]}" : "")}\n[Click here to appeal]({(await AppealGetter(Context.Guild.Id) == "" ? "" : await AppealGetter(Context.Guild.Id))})",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                }
                catch { }
                await Context.Guild.AddBanAsync(aa, 7, args.Length > 1 ? string.Join(' ', args.Skip(1)) : $"Requested by {Context.User.Username}#{Context.User.Discriminator}");
                await AddToModlogs(Context.Guild.Id, aa.Id, Context.User.Id, Punishment.Ban, DateTime.Now, args.Length > 1 ? string.Join(' ', args.Skip(1)) : "");
                return;
            }
            else
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "That user isn't valid :(",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
        }
        [GuildPermissions(GuildPermission.KickMembers)]
        [DiscordCommand("kick", commandHelp = "kick <@user> <reason>", example = "kick @NotAScammer Scamming my friend", description = "Kicks the specified user")]
        public async Task Kicker(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "Mention the user you wish to kick",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (await GetUser(args[0]) != null || Context.Message.MentionedUsers.Any())
            {
                var gUser = await GetUser(args[0]);
                if (gUser == null)
                {
                    gUser = Context.Message.MentionedUsers.First() as SocketGuildUser;
                }
                if (gUser.Hierarchy < (Context.User as SocketGuildUser).Hierarchy)
                {
                    if (gUser.Hierarchy >= Context.Guild.CurrentUser.Hierarchy)
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "Oops, that person is above me :(",
                            Description = $"I don't have perms to kick them :/",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                        return;
                    }
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = $"{gUser.Username}#{gUser.Discriminator} Kicked Successfully!",
                        Description = $"Reason: {(args.Length > 1 ? string.Join(' ', args.Skip(1)) : $"Requested by { Context.User.Username }#{Context.User.Discriminator}")}",
                        Color = Blurple
                    }.WithCurrentTimestamp());
                    try
                    {
                        await gUser.SendMessageAsync("", false, new EmbedBuilder
                        {
                            Title = "Oops, you were kicked!",
                            Description = $"You were kicked from **{Context.Guild.Name}** by {Context.User.Mention} {(args.Length > 1 ? $"Reason:{args[1]}" : "")}\n[Click here to appeal]({(await AppealGetter(Context.Guild.Id) == "" ? "" : await AppealGetter(Context.Guild.Id))})",
                            Color = Color.Red
                        }.WithCurrentTimestamp().Build());
                    }
                    catch { }
                    await gUser.KickAsync(args.Length > 1 ? string.Join(' ', args.Skip(1)) : $"Requested by {Context.User.Username}#{Context.User.Discriminator}");
                    await AddToModlogs(Context.Guild.Id, gUser.Id, Context.User.Id, Punishment.Kick, DateTime.Now, args.Length > 1 ? string.Join(' ', args.Skip(1)) : $"");
                    return;
                }
                else if (gUser.Hierarchy == (Context.User as SocketGuildUser).Hierarchy)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = $"Seriously??",
                        Color = Color.Red,
                        ImageUrl = "https://media.discordapp.net/attachments/758922634749542420/760180449749368832/unknown.png"
                    }.WithCurrentTimestamp());
                    return;
                }
                else
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Not gonna happen",
                        Description = "That person is above you!?",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
            }
            else
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "That user isn't valid :(",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
        }
        [Alt("appeal")]
        [GuildPermissions(GuildPermission.ManageGuild)]
        [DiscordCommand("setappeal", commandHelp = "setappeal <link>", example = "setappeal https://gforms.com/bah", description = "Sets the appeal link sent to punished members")]
        public async Task Setappeal(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The current appeal link",
                    Description = $"`{(await AppealGetter(Context.Guild.Id) == "" ? "No appeal set" : await AppealGetter(Context.Guild.Id))}`\n",
                    Color = Blurple,
                    Url = (await AppealGetter(Context.Guild.Id) == "" ? null : await AppealGetter(Context.Guild.Id)),
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"To change it, do `{await PrefixGetter(Context.Guild.Id)}setappeal <link>`, and do `{await PrefixGetter(Context.Guild.Id)}setappeal remove` to remove it"
                    }
                }.WithCurrentTimestamp());
                return;
            }
            else
            {
                if (!Uri.TryCreate(args[0], UriKind.RelativeOrAbsolute, out Uri? _))
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Invalid appeal link!",
                        Description = $"Couldn't parse `{args[0]}` as an URL :sob:",
                        Color = Color.Red,
                        Footer = new EmbedFooterBuilder
                        {
                            Text = $"To change it, do `{await PrefixGetter(Context.Guild.Id)}setappeal <link>`, and do `{await PrefixGetter(Context.Guild.Id)}setappeal remove` to remove it"
                        }
                    }.WithCurrentTimestamp());
                    return;
                }
                if (args[0].ToLower() == "remove")
                {
                    args[0] = "";
                }
                await AppealAdder(Context.Guild.Id, args[0]);
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The updated appeal link",
                    Description = $"`{(await AppealGetter(Context.Guild.Id) == "" ? "Appeal Removed!" : await AppealGetter(Context.Guild.Id))}`\n",
                    Color = Blurple,
                    Url = (await AppealGetter(Context.Guild.Id) == "" ? null : await AppealGetter(Context.Guild.Id)),
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"To change it yet again, do `{await PrefixGetter(Context.Guild.Id)}setappeal <link>`"
                    }
                }.WithCurrentTimestamp());
            }
        }
        [GuildPermissions(GuildPermission.BanMembers)]
        [DiscordCommand("unban", commandHelp = "unban <@user>", example = "unban ForgivenDude", description = "Unbans given user")]
        public async Task Unbn(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "Mention the user you wish to unban",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var bu = await GetBannedUser(args[0]);
            if (bu == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = $"`{args[0]}` isn't a previously banned user!?",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            await Context.Guild.RemoveBanAsync(bu);
            await AddToModlogs(Context.Guild.Id, bu.Id, Context.User.Id, Punishment.Unban, DateTime.Now);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = $"{bu.Username}#{bu.Discriminator} unbanned succesfully!",
                Description = $"Unban successful! Welcome them back :tada:",
                Color = Blurple
            }.WithCurrentTimestamp());
        }
        [GuildPermissions(GuildPermission.BanMembers)]
        [DiscordCommand("softban", commandHelp = "softban <@user> <days>", example = "softban @Dumbass 7", description = "Bans the specified user and unbans immediately, for deletion of messages")]
        public async Task SBanner(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "Mention the user you wish to softban",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (args.Length == 2)
            {
                if (ulong.TryParse(args[1], out ulong idkc))
                {
                    if (idkc > 7)
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "Invalid delete (prune) days parameter!",
                            Description = "It needs to be between 0 and 7 bruh :|",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                        return;
                    }
                }
            }
            if (await GetUser(args[0]) != null || Context.Message.MentionedUsers.Any())
            {
                var gUser = await GetUser(args[0]);
                if (gUser == null)
                {
                    gUser = Context.Message.MentionedUsers.First() as SocketGuildUser;
                }
                if (gUser.Hierarchy < (Context.User as SocketGuildUser).Hierarchy)
                {
                    if (gUser.Hierarchy >= Context.Guild.CurrentUser.Hierarchy)
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "Oops, that person is above me :(",
                            Description = $"I don't have perms to ban them :/",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                        return;
                    }
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = $"{gUser.Username}#{gUser.Discriminator} Softbanned Successfully!",
                        Description = $"Days to delete: {(args.Length == 1 ? "7" : (ulong.TryParse(args[1], out ulong a1) ? a1.ToString() : "7"))}",
                        Color = Blurple
                    }.WithCurrentTimestamp());
                    try
                    {
                        await gUser.SendMessageAsync("", false, new EmbedBuilder
                        {
                            Title = "Oops, you were softbanned!",
                            Description = $"You were softbanned from **{Context.Guild.Name}** by {Context.User.Mention}\n[Click here to appeal]({(await AppealGetter(Context.Guild.Id) == "" ? "" : await AppealGetter(Context.Guild.Id))})",
                            Color = Color.Red
                        }.WithCurrentTimestamp().Build());
                    }
                    catch { }
                    var gUID = gUser.Id;
                    await gUser.BanAsync(args.Length == 1 ? 7 : (ulong.TryParse(args[1], out ulong ak47) ? Convert.ToInt32(ak47) : 7));
                    await Context.Guild.RemoveBanAsync(gUID);
                    await AddToModlogs(Context.Guild.Id, gUser.Id, Context.User.Id, Punishment.Softban, DateTime.Now);
                    return;
                }
                else if (gUser.Hierarchy == (Context.User as SocketGuildUser).Hierarchy)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = $"Seriously??",
                        Color = Color.Red,
                        ImageUrl = "https://cdn.discordapp.com/attachments/758922634749542420/760180089870090320/unknown.png"
                    }.WithCurrentTimestamp());
                    return;
                }
                else
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Not gonna happen",
                        Description = "That person is above you!?",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
            }
            else if (ulong.TryParse(args[0], out ulong ide))
            {
                var aadrc = new DiscordRestClient();
                await aadrc.LoginAsync(TokenType.Bot, Program.token);
                var aa = await aadrc.GetUserAsync(ide);
                if (aa == null)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "What user?",
                        Description = "That user isn't on Discord which world r u in? :(",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = $"{aa.Username}#{aa.Discriminator} Softbanned Successfully!",
                    Description = $"Days to delete: {(args.Length == 1 ? "7" : (ulong.TryParse(args[1], out ulong a1) ? a1.ToString() : "7"))}",
                    Color = Blurple
                }.WithCurrentTimestamp());
                try
                {
                    await aa.SendMessageAsync("", false, new EmbedBuilder
                    {
                        Title = "Oops, you were banned!",
                        Description = $"You were banned from **{Context.Guild.Name}** by {Context.User.Mention}\n[Click here to appeal]({(await AppealGetter(Context.Guild.Id) == "" ? "" : await AppealGetter(Context.Guild.Id))})",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                }
                catch { }
                await Context.Guild.AddBanAsync(aa, args.Length == 1 ? 7 : (ulong.TryParse(args[1], out ulong ak47) ? Convert.ToInt32(ak47) : 7));
                await Context.Guild.RemoveBanAsync(aa);
                await AddToModlogs(Context.Guild.Id, aa.Id, Context.User.Id, Punishment.Softban, DateTime.Now, "");
                return;
            }
            else
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "That user isn't valid :(",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
        }
        [GuildPermissions(GuildPermission.BanMembers)]
        [DiscordCommand("bans", commandHelp = "bans", description = "Shows the bans in the server")]
        public async Task Bans(params string[] _)
        {
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "List of bans in the server",
                Description = string.Join("\n\n", (await Context.Guild.GetBansAsync()).Select((r5, idx) => $"**{idx + 1}) User**: <@{r5.User.Id}>\n**Reason**: {(r5.Reason ?? "Not given")}")),
                Color = Blurple
            }.WithCurrentTimestamp()
            );
        }
        [GuildPermissions(GuildPermission.ManageChannels)]
        [DiscordCommand("lock", commandHelp = "lock <#channel>", description = "locks the mentioned channel", example = "lock #heistchan")]
        public async Task Lockchan(params string[] args)
        {
            SocketGuildChannel lockchnl;
            if (args.Length == 0)
            {
                //Assuming they want to lock the current channel.
                lockchnl = Context.Channel as SocketGuildChannel;
            }
            else
            {
                lockchnl = GetChannel(args[0]);
            }
            if (lockchnl == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid channel",
                    Description = $"`{args[0]}` could not be parsed as channel!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            EmbedBuilder alfa = new EmbedBuilder();
            if (lockchnl is not SocketTextChannel lockMSGchnl)
            {
                var lockVOICE = lockchnl as SocketVoiceChannel;
                var prvo = lockchnl.GetPermissionOverwrite(Context.Guild.EveryoneRole);
                OverwritePermissions xyz;
                if (prvo.HasValue)
                {
                    xyz = prvo.Value.Modify(connect: PermValue.Deny);
                }
                else
                {
                    xyz = new OverwritePermissions(connect: PermValue.Deny, speak: PermValue.Deny);
                }
                await lockVOICE.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, xyz);
                alfa.Title = $"Locked Voice Channel {lockVOICE.Name}";
                alfa.Description = "The aforementioned voice channel has been locked.";
                alfa.Color = Blurple;
                alfa.WithCurrentTimestamp();
            }
            else
            {
                var prvo = lockchnl.GetPermissionOverwrite(Context.Guild.EveryoneRole);
                OverwritePermissions xyz;
                if (prvo.HasValue)
                {
                    xyz = prvo.Value.Modify(sendMessages: PermValue.Deny);
                }
                else
                {
                    xyz = new OverwritePermissions(sendMessages: PermValue.Deny);
                }
                await lockchnl.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, xyz);
                alfa.Title = $"Locked Text Channel {lockMSGchnl.Name}";
                alfa.Description = $"{lockMSGchnl.Mention} has been locked";
                alfa.Color = Blurple;
                alfa.WithCurrentTimestamp();
            }
            await Context.Channel.SendMessageAsync("", false, alfa.Build());
        }
        [GuildPermissions(GuildPermission.ManageChannels)]
        [DiscordCommand("unlock", commandHelp = "unlock <#channel>", description = "Unlocks the mentioned channel", example = "unlock #heistchan")]
        public async Task Unlock(params string[] args)
        {
            SocketGuildChannel lockchnl;
            if (args.Length == 0)
            {
                //Assuming they want to lock the current channel.
                lockchnl = Context.Channel as SocketGuildChannel;
            }
            else
            {
                lockchnl = GetChannel(args[0]);
            }
            if (lockchnl == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid channel",
                    Description = $"`{args[0]}` could not be parsed as channel!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            EmbedBuilder alfa = new EmbedBuilder();
            if (lockchnl is not SocketTextChannel lockMSGchnl)
            {
                var lockVOICE = lockchnl as SocketVoiceChannel;
                var prvo = lockchnl.GetPermissionOverwrite(Context.Guild.EveryoneRole);
                OverwritePermissions xyz;
                if (prvo.HasValue)
                {
                    xyz = prvo.Value.Modify(connect: PermValue.Inherit);
                }
                else
                {
                    xyz = new OverwritePermissions(connect: PermValue.Inherit);
                }
                await lockVOICE.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, xyz);
                alfa.Title = $"Unlocked Voice Channel {lockVOICE.Name}";
                alfa.Description = "The aforementioned voice channel has been unlocked.";
                alfa.WithCurrentTimestamp();
                alfa.Color = Blurple;
            }
            else
            {
                var prvo = lockchnl.GetPermissionOverwrite(Context.Guild.EveryoneRole);
                OverwritePermissions xyz;
                if (prvo.HasValue)
                {
                    xyz = prvo.Value.Modify(sendMessages: PermValue.Inherit);
                }
                else
                {
                    xyz = new OverwritePermissions(sendMessages: PermValue.Inherit);
                }
                await lockchnl.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, xyz);
                alfa.Title = $"Unlocked Text Channel {lockMSGchnl.Name}";
                alfa.Description = $"{lockMSGchnl.Mention} has been unlocked";
                alfa.WithCurrentTimestamp();
                alfa.Color = Blurple;
            }
            await Context.Channel.SendMessageAsync("", false, alfa.Build());
        }
        [GuildPermissions(GuildPermission.ManageGuild)]
        [DiscordCommand("mutedrole", commandHelp = "mutedrole <create/role>", description = "Sets the roles for mutes", example = "mutedrole create`\n`mutedrole @Muted")]
        public async Task SMutedRole(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The current muted role",
                    Description = $"{(await MutedRoleIDGetter(Context.Guild.Id) == 0 ? "No muted role set" : $"<@&{await MutedRoleIDGetter(Context.Guild.Id)}")}>\n",
                    Color = Blurple,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"To change it, do `{await PrefixGetter(Context.Guild.Id)}muted <@MutedRole>`, and do `{await PrefixGetter(Context.Guild.Id)}muted create` to create a novel one"
                    }
                }.WithCurrentTimestamp());
                return;
            }
            else
            {
                if (args[0].ToLower() == "create")
                {
                    var msg = await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Creating muted role.......",
                        Color = Blurple
                    }.WithCurrentTimestamp());
                    var rl = await Context.Guild.CreateRoleAsync("Muted by RoleX", new Discord.GuildPermissions(), new Color(0, 0, 0), false, null);
                    foreach (var chnl in Context.Guild.Channels)
                    {
                        await chnl.AddPermissionOverwriteAsync(rl, new OverwritePermissions(sendMessages: PermValue.Deny, speak: PermValue.Deny));
                    }
                    args[0] = rl.Id.ToString();
                    await msg.DeleteAsync();
                }
                if (GetRole(args[0]) == null)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "What role?",
                        Description = $"Couldn't parse `{args[0]}` as role :(",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
                await MutedRoleIDAdder(Context.Guild.Id, GetRole(args[0]).Id);
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The updated Muted Role!",
                    Description = $"The muted role is now <@&{await MutedRoleIDGetter(Context.Guild.Id)}>",
                    Color = Blurple,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"To change it yet again, do `{await PrefixGetter(Context.Guild.Id)}mutedrole <@Role>`"
                    }
                }.WithCurrentTimestamp());
            }
        }
        [GuildPermissions(GuildPermission.ManageGuild)]
        [Alt("um")]
        [DiscordCommand("unmute", commandHelp = "unmute <@user>", example = "unmute @RegretfulMan", description = "Unmutes given user")]
        public async Task Unmute(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "Mention the user you wish to unmute",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            else if (await GetUser(args[0]) == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = $"Couldn't parse `{args[0]}` as user",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            else
            {
                try
                {
                    await (await GetUser(args[0])).RemoveRoleAsync(Context.Guild.GetRole(await MutedRoleIDGetter(Context.Guild.Id)));
                }
                catch { }
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "User unmuted successfully!",
                    Description = $"{await GetUser(args[0])} was successfully unmuted :)",
                    Color = Blurple
                }.WithCurrentTimestamp());
                return;
            }
        }
        [Alt("sm")]
        [GuildPermissions(new GuildPermission[] { GuildPermission.ManageChannels, GuildPermission.ManageGuild })]
        [DiscordCommand("slowmode", commandHelp = "slowmode <channel/category> <time>`\n`slowmode <time>", description = "Sets the channel or category slowmode", example = "slowmode 10s")]
        public async Task Sm(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Invalid Parameters",
                    Description = $"The way to run this command is `{await PrefixGetter(Context.Guild.Id)}slowmode <time>` to set in the current channel, or `{await PrefixGetter(Context.Guild.Id)}slowmode <#channel-or-category> <time>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var xchnl = GetChannel(args[0]);
            var xcatg = GetCategory(args[0]);
            var isChannel = true;
            var firstIsTime = false;
            if (xchnl == null && xcatg == null)
            {
                // Use the current channel
                xchnl = (SocketGuildChannel)Context.Channel;
                firstIsTime = true;

            }
            else if (xchnl == null)
            {
                // Category is not null
                isChannel = false;
            }
            else if (xcatg == null)
            {
                // Channel is not null
                if (xchnl as SocketTextChannel == null)
                {
                    await ReplyAsync(embed: new EmbedBuilder
                    {
                        Title = "You can't slow down the beat <a:vibing:782998739865305089>",
                        Description = "Voice Channels cannot be put to a slowdown",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
            }
            else
            {
                // Big Problem lmao
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Multiple Possibilities detected",
                    Description = $"Given `{args[0]}`\n**Channel Found:**\n<#{xchnl.Id}>\n**Category Found:**\n{xcatg.Name} (ID: {xcatg.Id})\nTo resolve this conflict, either use the ID, or mention the channel.",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            bool isValidTime;
            TimeSpan ts = new TimeSpan();
            var k = firstIsTime ? 0 : 1;
            if (!firstIsTime && args.Length == 1)
            {
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Invalid Parameters",
                    Description = $"The way to run this command is `{await PrefixGetter(Context.Guild.Id)}slowmode <time>` to set in the current channel, or `{await PrefixGetter(Context.Guild.Id)}slowmode <#channel-or-category> <time>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            isValidTime = args[k].Last() switch
            {
                'm' or 'M' or 's' or 'S' or 'h' or 'H' => true,
                _ => false
            } && int.TryParse(string.Join("", args[k].SkipLast(1)), out int _);
            if (!isValidTime)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The time parameter is invalid",
                    Description = $"Couldn't parse `{args[k]}` as time, see key below\n```s => seconds\nm => minutes\nh => hours```",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            else if (int.TryParse(string.Join("", args[k].SkipLast(1)), out int timezar))
            {
                ts = args[k].Last() switch
                {
                    'm' or 'M' => new TimeSpan(0, timezar, 0),
                    's' or 'S' => new TimeSpan(0, 0, timezar),
                    'h' or 'H' => new TimeSpan(timezar, 0, 0),
                    //Non possible outcome but IDE is boss
                    _ => new TimeSpan()
                };
                if (ts.TotalSeconds >= 21600 || ts.TotalSeconds <= 5 && ts.TotalSeconds != 0)
                {
                    await ReplyAsync(embed: new EmbedBuilder
                    {
                        Title = "Invalid Time",
                        Description = "Only 5 seconds to 6 hours of slowmode is permitted :(",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
            }
            if (isChannel)
            {
                // Let's go
                await (xchnl as SocketTextChannel).ModifyAsync(x => x.SlowModeInterval = Convert.ToInt32(ts.TotalSeconds));
            }
            else
            {
                foreach (var irdk in xcatg.Channels.Where(x => (x as SocketTextChannel) != null))
                {
                    await (irdk as SocketTextChannel).ModifyAsync(x => x.SlowModeInterval = Convert.ToInt32(ts.TotalSeconds));
                }
            }
            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = "Slowmode set!",
                Description = $"In {(isChannel ? $"channel <#{xchnl.Id}>" : $"category {xcatg.Name} (ID: {xcatg.Id})")}, a slowmode of {ts.TotalSeconds} seconds is set!",
                Color = Blurple
            }.WithCurrentTimestamp());
        }
        [Alt("hm")]
        [GuildPermissions(GuildPermission.ManageGuild)]
        [DiscordCommand("hardmute", description = "Mutes the given user after removing all roles", example = "hardmute @Dumbkid 5m For trying to ping everyone", commandHelp = "hardmute <@user> <time> <reason>")]
        public async Task HardMute(params string[] args)
        {
            if (await MutedRoleIDGetter(Context.Guild.Id) == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "No muted role set :|",
                    Description = $"Set muted role by running `{await PrefixGetter(Context.Guild.Id)}mutedrole <create/@Role>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            bool isValidTime = false;
            TimeSpan ts;
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "Mention the user you wish to hardmute",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (args.Length >= 2)
            {
                isValidTime = args[1].Last() switch
                {
                    'h' or 'H' or 'm' or 'M' or 'd' or 'D' or 's' or 'S' => true,
                    _ => false
                } && int.TryParse(string.Join("", args[1].SkipLast(1)), out int _);
                if (!isValidTime)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "The time parameter is invalid",
                        Description = $"Couldn't parse `{args[1]}` as time, see key below\n```s => seconds\nm => minutes\nh => hours\nd => days```",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
                else if (int.TryParse(string.Join("", args[1].SkipLast(1)), out int timezar))
                {
                    ts = args[1].Last() switch
                    {
                        'h' or 'H' => new TimeSpan(timezar, 0, 0),
                        'm' or 'M' => new TimeSpan(0, timezar, 0),
                        's' or 'S' => new TimeSpan(0, 0, timezar),
                        'd' or 'D' => new TimeSpan(timezar, 0, 0, 0),
                        //Non possible outcome but IDE is boss
                        _ => new TimeSpan()
                    };
                }
                else
                {
                    ts = TimeSpan.Zero;
                }
            }
            else
            {
                ts = TimeSpan.Zero;
            }
            if (await GetUser(args[0]) != null || Context.Message.MentionedUsers.Any())
            {
                var gUser = await GetUser(args[0]);
                if (gUser == null)
                {
                    gUser = Context.Message.MentionedUsers.First() as SocketGuildUser;
                }
                if (gUser.Hierarchy < (Context.User as SocketGuildUser).Hierarchy)
                {
                    if (gUser.Hierarchy >= Context.Guild.CurrentUser.Hierarchy)
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "Oops, that person is above me :(",
                            Description = $"I don't have sufficient permissions to hardmute them",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                        return;
                    }
                    try
                    {
                        await gUser.SendMessageAsync("", false, new EmbedBuilder
                        {
                            Title = "Oops, you were muted!",
                            Description = $"You were muted {(isValidTime ? $"for {ts.Days} days, {ts.Minutes} minutes and {ts.Seconds} seconds" : "indefinitely")} from **{Context.Guild.Name}** by {Context.User.Mention} {(args.Length > 1 ? $"Reason:{args[1]}" : "")}{(await AppealGetter(Context.Guild.Id) == "" ? "" : "\n[Click here to appeal]" + (await AppealGetter(Context.Guild.Id)))})",
                            Color = Color.Red
                        }.WithCurrentTimestamp().Build());
                    }
                    catch { }
                    string guildName = Context.Guild.Name;
                    await AddToModlogs(Context.Guild.Id, gUser.Id, Context.User.Id, Punishment.HardMute, DateTime.Now, args.Length > 2 ? string.Join(' ', args.Skip(2)) : "");
                    var formerroles = gUser.Roles.ToList();
                    formerroles.Remove(Context.Guild.EveryoneRole);
                    await gUser.RemoveRolesAsync(formerroles);
                    try
                    {
                        await gUser.AddRoleAsync(Context.Guild.GetRole(await MutedRoleIDGetter(Context.Guild.Id)));
                    } catch
                    {
                        await ReplyAsync("", false, new EmbedBuilder { Title = "okay ur muted role is messed", Description = "wth man.", Color = Color.Red });
                    }
                    if (!isValidTime)
                    {
                        return;
                    }
                    Timer tmr = new Timer()
                    {
                        AutoReset = false,
                        Interval = ts.TotalMilliseconds
                    };
                    Console.WriteLine(ts);
                    tmr.Elapsed += async (object send, ElapsedEventArgs arg) =>
                    {
                        try
                        {
                            await gUser.RemoveRoleAsync(Context.Guild.GetRole(await MutedRoleIDGetter(Context.Guild.Id)));
                            await gUser.AddRolesAsync(formerroles);
                            await gUser.SendMessageAsync($"**You have been unmuted on {guildName}**");
                            return;
                        }
                        catch
                        {
                            Console.WriteLine("ERROR! ERROR!");
                        }
                    };
                    tmr.Enabled = true;
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = $"{gUser.Username}#{gUser.Discriminator} Hardmuted {(isValidTime ? $"for {ts.Days}d, {ts.Minutes}m and {ts.Seconds}s" : "indefinitely")}!",
                        Description = $"Reason: {(args.Length > 2 ? string.Join(' ', args.Skip(2)) : $"Requested by { Context.User.Username }#{Context.User.Discriminator}")}",
                        Color = Blurple
                    }.WithCurrentTimestamp());
                }
                else if (gUser.Hierarchy == (Context.User as SocketGuildUser).Hierarchy)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = $"Seriously??",
                        Color = Color.Red,
                        ImageUrl = "https://cdn.discordapp.com/attachments/758922634749542420/760180089870090320/unknown.png"
                    }.WithCurrentTimestamp());
                    return;
                }
                else
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Not gonna happen",
                        Description = "That person is above you!?",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
            }
            else
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "That user isn't valid :(",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
        }
    }
}
