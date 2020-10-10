using static Public_Bot.CustomCommandService;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using Discord.Rest;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using TradeMemer.modules;
using Newtonsoft.Json;
using System.IO;
using static TradeMemer.modules.SqliteClass;
using Public_Bot;
using GuildPermissions = Public_Bot.GuildPermissions;
using System.Security.Cryptography.X509Certificates;

namespace TradeMemer.modules
{
    [DiscordCommandClass("Moderation", "Basic Moderation for yer server!")]
    public class Moderator : CommandModuleBase
    {
        [GuildPermissions(GuildPermission.BanMembers)]
        [DiscordCommand("ban",commandHelp ="ban <@user> <reason>", example ="ban @Scammer Scamming me friend", description ="Bans the specified user")]
        public async Task Banner(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "Mention the user you wish to ban",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            if (GetUser(args[0]) != null)
            {
                var gUser = GetUser(args[0]);
                if (gUser.Hierarchy < (Context.User as SocketGuildUser).Hierarchy)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = $"{gUser.Username}#{gUser.Discriminator} Banned Successfully!",
                        Description = $"Reason: {(args.Length > 1 ? string.Join(' ', args.Skip(1)) : $"Requested by { Context.User.Username }#{Context.User.Discriminator}")}",
                        Color = Blurple
                    }.WithCurrentTimestamp().Build());
                    try
                    {
                        await gUser.SendMessageAsync("", false, new EmbedBuilder
                        {
                            Title = "Oops, you were banned!",
                            Description = $"You were banned from **{Context.Guild.Name}** by {Context.User.Mention} {(args.Length > 1 ? $"Reason:{args[1]}" : "")}\n{(await AppealGetter(Context.Guild.Id) == "" ? "" : await AppealGetter(Context.Guild.Id))}",
                            Color = Color.Red
                        }.WithCurrentTimestamp().Build());
                    }
                    catch { }
                    await gUser.BanAsync(7, args.Length > 1 ? string.Join(' ', args.Skip(1)) : $"Requested by {Context.User.Username}#{Context.User.Discriminator}");
                    return;
                } else if (gUser.Hierarchy == (Context.User as SocketGuildUser).Hierarchy)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = $"Seriously??",
                        Color = Color.Red,
                        ImageUrl = "https://cdn.discordapp.com/attachments/758922634749542420/760180089870090320/unknown.png"
                    }.WithCurrentTimestamp().Build());
                    return;
                }
                else
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Not gonna happen",
                        Description = "That person is above you!?",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
                }
            } else if (ulong.TryParse(args[0], out ulong ide))
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
                    }.WithCurrentTimestamp().Build());
                    return;
                }
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = $"{aa.Username}#{aa.Discriminator} Banned Successfully!",
                    Description = $"Reason: {(args.Length > 1 ? string.Join(' ', args.Skip(1)) : $"Requested by { Context.User.Username }#{Context.User.Discriminator}")}",
                    Color = Blurple
                }.WithCurrentTimestamp().Build());
                try
                {
                    await aa.SendMessageAsync("", false, new EmbedBuilder
                    {
                        Title = "Oops, you were banned!",
                        Description = $"You were banned from **{Context.Guild.Name}** by {Context.User.Mention} {(args.Length > 1 ? $"Reason:{args[1]}" : "")}\n[Click here to appeal]({(await AppealGetter(Context.Guild.Id) == "" ? "" : await AppealGetter(Context.Guild.Id))})",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                } catch { }
                await Context.Guild.AddBanAsync(aa, 7, args.Length > 1 ? string.Join(' ', args.Skip(1)) : $"Requested by {Context.User.Username}#{Context.User.Discriminator}");
                return;
            } else
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "That user isn't valid :(",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
        }
        [GuildPermissions(GuildPermission.KickMembers)]
        [DiscordCommand("kick", commandHelp = "kick <@user> <reason>", example = "kick @Scammer Scamming me friend", description = "Kicks the specified user")]
        public async Task Kicker(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "Mention the user you wish to kick",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            if (GetUser(args[0]) != null)
            {
                var gUser = GetUser(args[0]);
                if (gUser.Hierarchy < (Context.User as SocketGuildUser).Hierarchy)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = $"{gUser.Username}#{gUser.Discriminator} Kicked Successfully!",
                        Description = $"Reason: {(args.Length > 1 ? string.Join(' ', args.Skip(1)) : $"Requested by { Context.User.Username }#{Context.User.Discriminator}")}",
                        Color = Blurple
                    }.WithCurrentTimestamp().Build());
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
                    return;
                }
                else if (gUser.Hierarchy == (Context.User as SocketGuildUser).Hierarchy)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = $"Seriously??",
                        Color = Color.Red,
                        ImageUrl = "https://media.discordapp.net/attachments/758922634749542420/760180449749368832/unknown.png"
                    }.WithCurrentTimestamp().Build());
                    return;
                }
                else
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Not gonna happen",
                        Description = "That person is above you!?",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
                }
            } else
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "That user isn't valid :(",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
        }
        [GuildPermissions(GuildPermission.ManageGuild)]
        [DiscordCommand("setappeal", commandHelp ="setappeal <link>", example ="setappeal https://gforms.com/bah", description ="Sets the appeal link sent to punished members")]
        public async Task setappeal(params string[] args)
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
                }.WithCurrentTimestamp().Build());
                return;
            }
            else
            {
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
                }.WithCurrentTimestamp().Build());
            }
        }
        [GuildPermissions(GuildPermission.BanMembers)]
        [DiscordCommand("unban", commandHelp ="unban <@user>", example = "unban ForgivenDude", description ="Unbans given user")]
        public async Task Unbn(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "Mention the user you wish to unban",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            var bu = await GetBannedUser(args[0]);
            if (bu == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = $"`{args[0]}` isn't a past banned user!?",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            await Context.Guild.RemoveBanAsync(bu);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = $"{bu.Username}#{bu.Discriminator} unbanned succesfully!",
                Description = $"Unban done! Welcome him back :tada:",
                Color = Blurple
            }.WithCurrentTimestamp().Build());
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
                }.WithCurrentTimestamp().Build());
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
                        }.WithCurrentTimestamp().Build());
                        return;
                    }
                }
            }
            if (GetUser(args[0]) != null)
            {
                var gUser = GetUser(args[0]);
                if (gUser.Hierarchy < (Context.User as SocketGuildUser).Hierarchy)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = $"{gUser.Username}#{gUser.Discriminator} Softbanned Successfully!",
                        Description = $"Days to delete: {(args.Length == 1 ? "7" : (ulong.TryParse(args[1], out ulong a1) ? a1.ToString() : "7"))}",
                        Color = Blurple
                    }.WithCurrentTimestamp().Build());
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
                    return;
                }
                else if (gUser.Hierarchy == (Context.User as SocketGuildUser).Hierarchy)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = $"Seriously??",
                        Color = Color.Red,
                        ImageUrl = "https://cdn.discordapp.com/attachments/758922634749542420/760180089870090320/unknown.png"
                    }.WithCurrentTimestamp().Build());
                    return;
                }
                else
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Not gonna happen",
                        Description = "That person is above you!?",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
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
                    }.WithCurrentTimestamp().Build());
                    return;
                }
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = $"{aa.Username}#{aa.Discriminator} Softbanned Successfully!",
                    Description = $"Days to delete: {(args.Length == 1 ? "7" : (ulong.TryParse(args[1], out ulong a1) ? a1.ToString() : "7"))}",
                    Color = Blurple
                }.WithCurrentTimestamp().Build());
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
                return;
            }
            else
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "That user isn't valid :(",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
        }
        [GuildPermissions(GuildPermission.BanMembers)]
        [DiscordCommand("bans",commandHelp ="bans",description ="Shows the bans in the server")]
        public async Task Bans()
        {
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "List of bans in the server",
                Description = string.Join("\n\n", (await Context.Guild.GetBansAsync()).Select((r5, idx) => $"**{idx + 1}) User**: <@{r5.User.Id}>\n**Reason**: {(r5.Reason == null ? "Not given" : r5.Reason)}")),
                Color = Blurple
            }.WithCurrentTimestamp().Build()
            );
        }
        [GuildPermissions(GuildPermission.ManageChannels)]
        [DiscordCommand("lock", RequiredPermission = true, commandHelp = "lock <#channel>", description = "locks the mentioned channel", example ="lock #heistchan")]
        public async Task lockchan(params string[] args)
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
                }.WithCurrentTimestamp().Build());
                return;
            }
            var lockMSGchnl = lockchnl as SocketTextChannel;
            EmbedBuilder alfa = new EmbedBuilder();
            if (lockMSGchnl == null)
            {
                var lockVOICE = lockchnl as SocketVoiceChannel;
                var xyz = new OverwritePermissions(connect: PermValue.Deny);
                await lockVOICE.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, xyz);
                alfa.Title = $"Locked Voice Channel {lockVOICE.Name}";
                alfa.Description = "The aforementioned voice channel has been locked.";
                alfa.Color = Blurple;
                alfa.WithCurrentTimestamp();
            }
            else
            {
                var sry = new OverwritePermissions(sendMessages: PermValue.Deny);
                await lockchnl.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, sry);
                alfa.Title = $"Locked Text Channel {lockMSGchnl.Name}";
                alfa.Description = $"{lockMSGchnl.Mention} has been locked";
                alfa.Color = Blurple;
                alfa.WithCurrentTimestamp();
            }
            await Context.Channel.SendMessageAsync("", false, alfa.Build());
        }
        [GuildPermissions(GuildPermission.ManageChannels)]
        [DiscordCommand("unlock", commandHelp = "unlock <#channel>", description = "Unlocks the mentioned channel", example="unlock #heistchan")]
        public async Task unlock(params string[] args)
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
                }.WithCurrentTimestamp().Build());
                return;
            }
            var lockMSGchnl = lockchnl as SocketTextChannel;
            EmbedBuilder alfa = new EmbedBuilder();
            if (lockMSGchnl == null)
            {
                var lockVOICE = lockchnl as SocketVoiceChannel;
                var xyz = new OverwritePermissions(connect: PermValue.Inherit);
                await lockVOICE.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, xyz);
                alfa.Title = $"Unlocked Voice Channel {lockVOICE.Name}";
                alfa.Description = "The aforementioned voice channel has been unlocked.";
                alfa.WithCurrentTimestamp();
                alfa.Color = Blurple;
            }
            else
            {
                var sry = new OverwritePermissions(sendMessages: PermValue.Inherit);
                await lockchnl.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, sry);
                alfa.Title = $"Unlocked Text Channel {lockMSGchnl.Name}";
                alfa.Description = $"{lockMSGchnl.Mention} has been unlocked";
                alfa.WithCurrentTimestamp();
                alfa.Color = Blurple;
            }
            await Context.Channel.SendMessageAsync("", false, alfa.Build());
        }
    }
}