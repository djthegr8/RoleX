using System;
using static RoleX.Modules.SqliteClass;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Public_Bot;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using GuildPermissions = Public_Bot.GuildPermissions;
namespace RoleX.Modules
{
    [DiscordCommandClass("General", "General commands for all!")]
    public class Altchan : CommandModuleBase
    {
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
            else
            {
                if (args[0].ToLower() == "remove")
                {
                    await AlertChanAdder(Context.Guild.Id, 0);
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Alerts Disabled!",
                        Description = $"The alert channel has now been terminated.",
                        Color = Blurple,
                        Footer = new EmbedFooterBuilder
                        {
                            Text = $"To change it, do `{await PrefixGetter(Context.Guild.Id)}alertchan #channel`"
                        }
                    });
                }
                else if (GetChannel(args[0]) == null)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "What channel?",
                        Description = $"Couldn't parse `{args[0]}` as channel :(",
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
                        Text = $"To change it yet again, do `{await PrefixGetter(Context.Guild.Id)}alertchan #channel`"
                    }
                }.WithCurrentTimestamp());
            }
        }
    }
}