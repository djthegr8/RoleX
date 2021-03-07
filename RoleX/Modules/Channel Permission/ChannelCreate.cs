using System.Linq;
using Discord;
using RoleX.Modules.Services;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RoleX.Modules.Channel_Permission
{
    [DiscordCommandClass("Channel Editor", "Edit channel-wise perms of a channel using these commands")]
    internal class ChannelCreate : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageChannels)]
        [Alt("channelcreate")]
        [Alt("chadd")]
        [DiscordCommand("chcreate", commandHelp = "chcreate <category?> <name>", description = "Creates channel with synced perms in given category, or no category", example = "chcreate WeirdCategory Weird channel brr")]
        public async Task RCreate(params string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters",
                        Description =
                            $"The way to use the command is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}chcreate <category?> <name>`",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                case 1:
                    if (!Regex.IsMatch(args[0], "[a-zA-Z0-9-_]{2,100}"))
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "Invalid channel re-name",
                            Description =
                                $"`{args[0]}` is an invalid channel name, as it either ~ \n1) Contains non-allowed characters\n 2) Is too long",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                        return;
                    }

                    var channel = await Context.Guild.CreateTextChannelAsync(args[0]);
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Channel creation successful!",
                        Description = $"Successfully created channel <#{channel.Id}> (ID: {channel.Id})",
                        Color = Blurple
                    }.WithCurrentTimestamp());
                    return;
                default:
                    var cat = GetCategory(args[0]);
                    if (cat == null)
                    {
                        var bchname = string.Join('-', args);
                        if (!Regex.IsMatch(bchname, "[a-zA-Z0-9-_]{2,100}"))
                        {
                            await ReplyAsync("", false, new EmbedBuilder
                            {
                                Title = "Invalid channel re-name",
                                Description =
                                    $"`{bchname}` is an invalid channel name, as it either ~ \n1) Contains non-allowed characters\n 2) Is too long",
                                Color = Color.Red
                            }.WithCurrentTimestamp());
                            return;
                        }

                        var _rchannel = await Context.Guild.CreateTextChannelAsync(bchname);
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "Channel creation successful!",
                            Description = $"Successfully created channel <#{_rchannel.Id}> (ID: {_rchannel.Id})",
                            Color = Blurple
                        }.WithCurrentTimestamp());
                        return;
                    }
                    var _bchname = string.Join('-', args.Skip(1));
                    if (!Regex.IsMatch(_bchname, "[a-zA-Z0-9-_]{2,100}"))
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "Invalid channel re-name",
                            Description =
                                $"`{_bchname}` is an invalid channel name, as it either ~ \n1) Contains non-allowed characters\n 2) Is too long",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                        return;
                    }

                    var po = cat.PermissionOverwrites;
                    var _channel = await Context.Guild.CreateTextChannelAsync(_bchname, properties => properties.CategoryId = cat.Id);
                    await _channel.SyncPermissionsAsync();
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Channel creation successful!",
                        Description = $"Successfully created channel <#{_channel.Id}> (ID: {_channel.Id})",
                        Color = Blurple
                    }.WithCurrentTimestamp());
                    break;
            }

        }
    }
}
