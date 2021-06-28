using System.Threading.Tasks;
using Discord;
using Hermes.Modules.Services;
using static Hermes.Modules.Services.SqliteClass;

namespace Hermes.Modules.General
{
    [DiscordCommandClass("General", "General commands for all!")]
    public class Altchan : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageGuild)]
        [DiscordCommand("altchan", commandHelp = "altchan #channel", description = "Sets the channel for alt alerts", example = "altchan #staff-announcements`\n`altchan remove")]
        public async Task AltChan(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The current alt alerts channel",
                    Description = $"{(await AlertChanGetter(Context.Guild.Id) == 0 ? "No alert channel set" : $"<#{await AlertChanGetter(Context.Guild.Id)}>")}\n",
                    Color = Blurple,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"To change it, do `{await PrefixGetter(Context.Guild.Id)}altchan #channel`"
                    }
                }.WithCurrentTimestamp());
                return;
            }

            if (args[0].ToLower() == "remove")
            {
                await AlertChanAdder(Context.Guild.Id, 0);
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Alerts Disabled!",
                    Description = "The alert channel has now been terminated.",
                    Color = Blurple,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"To change it, do `{await PrefixGetter(Context.Guild.Id)}alertchan #channel`"
                    }
                });
                return;
            }

            if (GetChannel(args[0]) == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What channel?",
                    Description = $"Couldn't parse `{args[0]}` as a channel ",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            await AlertChanAdder(Context.Guild.Id, GetChannel(args[0]).Id);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "The updated Alert Channel!",
                Description = $"The alert channel is now <#{await AlertChanGetter(Context.Guild.Id)}>",
                Color = Blurple,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"To change it again, run `{await PrefixGetter(Context.Guild.Id)}alertchan #channel`"
                }
            }.WithCurrentTimestamp());
        }
    }
}