using System;
using System.Linq;
using Discord;
using RoleX.Modules.Services;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace RoleX.Modules.Channel_Permission
{
    [DiscordCommandClass("Channel Editor", "Edit channel-wise perms of a channel using these commands")]
    internal class ChannelMove : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageChannels)]
        [Alt("chmove")]
        [DiscordCommand("channelmove", commandHelp = "chmove <channel> <channel>", description = "Moves channel BELOW the second given channel (is imperfect, but i guess you could navigate a bit)", example = "chmove #weirdchan #weird2chan")]
        public async Task RCreate(params string[] args)
        {
            switch (args.Length)
            {
                case 0 or 1:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters",
                        Description =
                            $"The way to use the command is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}chmove <channel> <channel>`",
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
                    Console.WriteLine(chan.Position == chan2.Position);

                    await chan.ModifyAsync(um =>
                    {
                        um.CategoryId = chan2.CategoryId;
                        um.Position = chan2.Position;
                    });
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
