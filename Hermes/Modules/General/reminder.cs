using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Hermes.Modules.Services;

namespace Hermes.Modules.General
{
    [DiscordCommandClass("General", "General commands for all!")]
    internal class Reminder : CommandModuleBase
    {
        [Alt("rm")]
        [DiscordCommand("reminder", commandHelp = "reminder <time> <reason>`\n`reminder list`\n`reminder remove <id>", example = "reminder 10m Think about life", description = "Reminds the user to do something after specified time!")]
        public async Task ReminderCommand(params string[] args)
        {
            if (args.Length == 0 || args[0] == "list")
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Your current reminders!",
                    Description = $"{((await SqliteClass.GetReminders($"SELECT * FROM reminders WHERE UserID = {Context.User.Id} AND Finished IS 0;")).Count == 0 ? "User has no reminders" : ("```\n" + string.Join('\n',(await SqliteClass.GetReminders($"SELECT * FROM reminders WHERE UserID = {Context.User.Id} AND Finished IS 0;")).Select(x => $"ID: {x.Id}\tTime: {x.TimeS:U}\tReason:{x.Reason}")) + "```"))}",
                    Color = Blurple
                }.WithCurrentTimestamp());
                return;
            }
            if (args[0] == "remove" || args[0] == "-")
            {
                if (args.Length == 1)
                {
                    await ReplyAsync(embed: new EmbedBuilder
                    {
                        Title = "That's not how you run this command",
                        Description = $"Command Syntax: `{await SqliteClass.PrefixGetter(Context.Guild.Id)}reminder remove <id>`\nYou can find the ID using list.",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                }
                bool cudFind = false;
                var lor = await SqliteClass.GetReminders($"SELECT * FROM reminders WHERE UserID = {Context.User.Id} AND Finished IS 0;");
                if (lor != new List<SqliteClass.Reminder>())
                {
                    if (lor.Any(k => k.Id == args[1]))
                    {
                        await SqliteClass.ReminderFinished(lor.First(k => k.Id == args[1]));
                        cudFind = true;
                    }
                }
                if (cudFind)
                {
                    await ReplyAsync("Reminder removed successfully!");
                } else
                {
                    await ReplyAsync("Could not find a reminder with that ID.\nUse `reminder list` to find all active reminders");
                }
                return;
            }
            var ts = new TimeSpan();
            var isValidTime = args[0].Last() switch
            {
                'h' or 'H' or 'm' or 'M' or 'd' or 'D' or 's' or 'S' => true,
                _ => false
            } && int.TryParse(string.Join("", args[0].SkipLast(1)), out int _);
            if (!isValidTime)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The time parameter is invalid",
                    Description = $"Couldn't parse `{args[0]}` as time, see key below\n```s => seconds\nm => minutes\nh => hours\nd => days```",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            if (int.TryParse(string.Join("", args[0].SkipLast(1)), out int timezar))
            {
                ts = args[0].Last() switch
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
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The time parameter is invalid",
                    Description = $"Couldn't parse `{args[0]}` as time, see key below\n```s => seconds\nm => minutes\nh => hours\nd => days```",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            await SqliteClass.AddReminder(new SqliteClass.Reminder { UserId = Context.User.Id, TimeS = DateTime.UtcNow.Add(ts), Reason = args.Length > 1 ? string.Join("", string.Join(' ',args.Skip(1)).Take(25)) : "Not given" });
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Reminder added!",
                Description = $"We shall remind you after `{ts.Days}`d `{ts.Hours}`h, `{ts.Minutes}`m and `{ts.Seconds}`s",
                Color = Blurple
            });
        }
    }
}
