using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Hermes.Modules.Services;
using static Hermes.Modules.Services.SqliteClass;

namespace Hermes.Modules.Moderation
{
    [DiscordCommandClass("Moderation", "Basic Moderation for yer server!")]
    public class Ban : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.BanMembers)]
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
                var gUser = await GetUser(args[0]) ?? Context.Message.MentionedUsers.First() as SocketGuildUser;
                if (gUser.Hierarchy < (Context.User as SocketGuildUser).Hierarchy)
                {
                    if (gUser.Hierarchy >= Context.Guild.CurrentUser.Hierarchy)
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "Oops, that person is above me :(",
                            Description = "I don't have sufficient permissions to ban them",
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
                    await AddToModlogs(Context.Guild.Id, gUser.Id, Context.User.Id, Punishment.Ban, DateTime.Now, args.Length > 1 ? string.Join(' ', args.Skip(1)) : "");
                    return;
                }

                if (gUser.Hierarchy == (Context.User as SocketGuildUser).Hierarchy)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Seriously??",
                        Color = Color.Red,
                        ImageUrl = "https://imgur.com/HaDzAbG"
                    }.WithCurrentTimestamp());
                    return;
                }
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Not gonna happen",
                    Description = "That person is above you!?",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            if (ulong.TryParse(args[0].Replace("<@","").Replace(">",""), out ulong ide))
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
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "What user?",
                Description = "That user isn't valid ",
                Color = Color.Red
            }.WithCurrentTimestamp());
        }
    }
}