using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using RoleX.Modules.Services;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using MongoDB.Driver;

namespace RoleX.Modules.Channel_Permission
{
    [DiscordCommandClass("Channel Editor", "Edit channel-wise perms of a channel using these commands")]
    internal class ChannelMove : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageChannels)]
        [Alt("mtcat")]
        [DiscordCommand("movetocat", commandHelp = "movetocat <channel> <channel>", description = "Moves channel to the category of second channel", example = "chmove #weirdchan #weird2chan", IsPremium = true)]
        public async Task RCreate(params string[] args)
        {
            switch (args.Length)
            {
                case 0 or 1:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters",
                        Description =
                            $"The way to use the command is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}movetocat <channel> <channel>`",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                default:
                    if (GetChannel(args[0]) is not INestedChannel chan)
                    {
                        await InvalidChannel(args[0]);
                        return;
                    } 
                    if (GetChannel(args[1]) is not INestedChannel chan2)
                    {
                        await InvalidChannel(args[1]);
                        return;
                    }
                    await chan.ModifyAsync(channel =>
                    {
                        channel.CategoryId = chan2.CategoryId;
                    });
                    // Console.WriteLine(string.Join('\n', Context.Guild.Channels.OrderBy(k => k.Position).Select(k => k.Name)));
                    await ReplyAsync("", false, new EmbedBuilder()
                    {
                        Title = "Successfully moved channel!",
                        Description = $"Channel <#{chan.Id}> was successfully moved below <#{chan2.Id}>",
                        Color = Blurple
                    }.WithCurrentTimestamp());
                    break;
                    
            }

            async Task InvalidChannel(string channel)
        {
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Invalid channel",
                Description =
                    $"Couldn't parse {channel} as channel",
                Color = Color.Red
            }.WithCurrentTimestamp());
        }
    }
    }
}
