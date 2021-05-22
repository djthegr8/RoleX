using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using RoleX.Modules.Services;
using static RoleX.Modules.Services.SqliteClass;

namespace RoleX.Modules.Moderation
{
    [DiscordCommandClass("Moderation", "Basic Moderation for yer server!")]
    public class Softban : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.BanMembers)]
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
                            Description = "Parameters are from 0-7 days",
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
                            Description = "I don't have perms to ban them :/",
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

            if (ulong.TryParse(args[0], out ulong ide))
            {
                var aadrc = new DiscordRestClient();
                await aadrc.LoginAsync(TokenType.Bot, Program.token);
                var aa = await aadrc.GetUserAsync(ide);
                if (aa == null)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "What user?",
                        Description = "That user isn't on Discord",
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
                await AddToModlogs(Context.Guild.Id, aa.Id, Context.User.Id, Punishment.Softban, DateTime.Now);
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
