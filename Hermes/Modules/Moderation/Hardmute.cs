using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;
using Hermes.Modules.Services;
using static Hermes.Modules.Services.SqliteClass;

namespace Hermes.Modules.Moderation
{
    [DiscordCommandClass("Moderation", "Basic Moderation for yer server!")]
    public class Hardmute : CommandModuleBase
    {
        [Alt("hm")]
        [RequiredUserPermissions(GuildPermission.ManageRoles)]
        [DiscordCommand("hardmute", description = "Mutes the given user after removing all roles",
            example = "hardmute @Dumbkid 5m For trying to ping everyone",
            commandHelp = "hardmute <@user> <time> <reason>")]
        public async Task HardMute(params string[] args)
        {
            if (await MutedRoleIdGetter(Context.Guild.Id) == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "No muted role set ",
                    Description =
                        $"Set muted role by running `{await PrefixGetter(Context.Guild.Id)}mutedrole <create/@Role>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            var isValidTime = false;
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
                } && int.TryParse(string.Join("", args[1].SkipLast(1)), out var _);
                if (!isValidTime)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "The time parameter is invalid",
                        Description =
                            $"Couldn't parse `{args[1]}` as time, see key below\n```s => seconds\nm => minutes\nh => hours\nd => days```",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }

                if (int.TryParse(string.Join("", args[1].SkipLast(1)), out var timezar))
                    ts = args[1].Last() switch
                    {
                        'h' or 'H' => new TimeSpan(timezar, 0, 0),
                        'm' or 'M' => new TimeSpan(0, timezar, 0),
                        's' or 'S' => new TimeSpan(0, 0, timezar),
                        'd' or 'D' => new TimeSpan(timezar, 0, 0, 0),
                        //Non possible outcome but IDE is boss
                        _ => new TimeSpan()
                    };
                else
                    ts = TimeSpan.Zero;
            }
            else
            {
                ts = TimeSpan.Zero;
            }

            if (await GetUser(args[0]) != null || Context.Message.MentionedUsers.Any())
            {
                var gUser = await GetUser(args[0]);
                if (gUser == null) gUser = Context.Message.MentionedUsers.First() as SocketGuildUser;
                if (gUser.Hierarchy < (Context.User as SocketGuildUser).Hierarchy)
                {
                    if (gUser.Hierarchy >= Context.Guild.CurrentUser.Hierarchy)
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "Oops, that person is above me :(",
                            Description = "I don't have sufficient permissions to hardmute them",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                        return;
                    }

                    try
                    {
                        await gUser.SendMessageAsync("", false, new EmbedBuilder
                        {
                            Title = "You were muted!",
                            Description =
                                $"You were muted {(isValidTime ? $"for {ts.Days} days, {ts.Minutes} minutes and {ts.Seconds} seconds" : "indefinitely")} from **{Context.Guild.Name}** by {Context.User.Mention} Reason: {(args.Length > 2 ? string.Join(' ', args.Skip(2)) : "Not given")} {(await AppealGetter(Context.Guild.Id) == "" ? "" : "\n[Click here to appeal](" + await AppealGetter(Context.Guild.Id))})",
                            Color = Color.Red
                        }.WithCurrentTimestamp().Build());
                    }
                    catch
                    {
                    }

                    var guildName = Context.Guild.Name;
                    await AddToModlogs(Context.Guild.Id, gUser.Id, Context.User.Id, Punishment.HardMute, DateTime.Now,
                        args.Length > 2 ? string.Join(' ', args.Skip(2)) : "");
                    var formerroles = gUser.Roles.ToList();
                    formerroles.Remove(Context.Guild.EveryoneRole);
                    await gUser.RemoveRolesAsync(formerroles);
                    try
                    {
                        await gUser.AddRoleAsync(Context.Guild.GetRole(await MutedRoleIdGetter(Context.Guild.Id)));
                    }
                    catch
                    {
                        await ReplyAsync("", false,
                            new EmbedBuilder
                                {Title = "okay ur muted role is messed", Description = "wth man.", Color = Color.Red});
                    }

                    if (!isValidTime) return;
                    var tmr = new Timer
                    {
                        AutoReset = false,
                        Interval = ts.TotalMilliseconds
                    };
                    Console.WriteLine(ts);
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title =
                            $"{gUser.Username}#{gUser.Discriminator} Hardmuted {(isValidTime ? $"for {ts.Days}d, {ts.Minutes}m and {ts.Seconds}s" : "indefinitely")}!",
                        Description =
                            $"Reason: {(args.Length > 2 ? string.Join(' ', args.Skip(2)) : $"Requested by {Context.User.Username}#{Context.User.Discriminator}")}",
                        Color = Blurple
                    }.WithCurrentTimestamp());
                    tmr.Elapsed += async (send, arg) =>
                    {
                        try
                        {
                            await gUser.RemoveRoleAsync(
                                Context.Guild.GetRole(await MutedRoleIdGetter(Context.Guild.Id)));
                            await gUser.AddRolesAsync(formerroles);
                            await gUser.SendMessageAsync($"**You have been unmuted on {guildName}**");
                        }
                        catch
                        {
                            Console.WriteLine("E RROR! ERROR!");
                        }
                    };
                    tmr.Enabled = true;
                }
                else if (gUser.Hierarchy == (Context.User as SocketGuildUser).Hierarchy)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Seriously? <a:clapjohn:785371886695612427>",
                        Color = Color.Red,
                        ImageUrl = "https://i.imgur.com/RBC7KUt.png"
                    }.WithCurrentTimestamp());
                }
                else
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Not gonna happen",
                        Description = "That person is above you!?",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                }
            }
            else
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "That user isn't valid ",
                    Color = Color.Red
                }.WithCurrentTimestamp());
            }
        }
    }
}