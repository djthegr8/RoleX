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
    }
}