using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using RoleX.Modules.Services;
using static RoleX.Modules.Services.SqliteClass;

namespace RoleX.Modules.Moderation
{
    [DiscordCommandClass("Moderation", "Basic Moderation for yer server!")]
    public class Slowmode : CommandModuleBase
    {
        [RequiredUserPermissions(new[] {GuildPermission.ManageChannels})]
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
                if (!(xchnl is SocketTextChannel))
                {
                    await ReplyAsync(embed: new EmbedBuilder
                    {
                        Title = "You can't slow down the beat <a:vibing:782998739865305089>",
                        Description = "Voice Channels have a slowmode",
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

            if (int.TryParse(string.Join("", args[k].SkipLast(1)), out int timezar))
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
                        Description = "You can only set a slowmode from 1 second to 6 hours",
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
    }
}