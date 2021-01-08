using Discord;
using Discord.WebSocket;
using Public_Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoleX.Modules
{
    [DiscordCommandClass("General", "General commands for all!")]
    class Reminder : CommandModuleBase
    {
        [Alt("rm")]
        [DiscordCommand("reminder", commandHelp = "reminder <time> <reason>", example = "reminder 10m Think about life", description = "Reminds the user to do something after specified time!")]
        public async Task ReminderCommand(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Insufficient Parameters",
                    Description = $"The way to use the command is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}reminder <time> <optional-reason>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
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
            else if (int.TryParse(string.Join("", args[0].SkipLast(1)), out int timezar))
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
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The time parameter is invalid",
                    Description = $"Couldn't parse `{args[0]}` as time, see key below\n```s => seconds\nm => minutes\nh => hours\nd => days```",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            await SqliteClass.AddReminder(new SqliteClass.Reminder { UserID = Context.User.Id, TimeS = ts, Reason = args.Length > 1 ? args[1] : "Not given" });
        }
    }
}
