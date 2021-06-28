using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hermes.Modules.Services;
using static Hermes.Modules.Services.SqliteClass;

namespace Hermes.Modules.Moderation
{
    [DiscordCommandClass("Moderation", "Basic Moderation for yer server!")]
    public class Kick : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.KickMembers)]
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
                            Title = "Oops, that person is above me",
                            Description = "I don't have perms to kick them ",
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
                    await AddToModlogs(Context.Guild.Id, gUser.Id, Context.User.Id, Punishment.Kick, DateTime.Now, args.Length > 1 ? string.Join(' ', args.Skip(1)) : "");
                    return;
                }

                if (gUser.Hierarchy == (Context.User as SocketGuildUser).Hierarchy)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Seriously??",
                        Color = Color.Red,
                        ImageUrl = "https://imgur.com/2tNqJwZ"
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

            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "What user?",
                Description = "That user isn't valid ",
                Color = Color.Red
            }.WithCurrentTimestamp());
        }
    }
}
