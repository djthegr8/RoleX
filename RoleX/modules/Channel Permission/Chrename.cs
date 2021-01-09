using Discord;
using Discord.WebSocket;
using Public_Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleX.Modules
{
    [DiscordCommandClass("Channel Editor", "Edit Channel-wise perms of a Role using these commands!")]
    public class Chrename : CommandModuleBase
    {
        [DiscordCommand("chrename", commandHelp = "chrename <#channel> <multi-word-string>")]
        [Alt("channelrename")]
        [Alt("chre")]
        [RequiredUserPermissions(new[] { GuildPermission.ManageChannels})]
        public async Task RenameChannel(params string[] args)
        {
            if (args.Length == 0 || args.Length == 1)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Insufficient Parameters!",
                    Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}channelrename <#channel> <new-channel-name>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (GetChannel(args[0]) == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid channel",
                    Description = $"`{args[0]}` could not be parsed as channel!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var bchname = string.Join('-', args.Skip(1));
            if (!System.Text.RegularExpressions.Regex.IsMatch(bchname, "[a-z0-9-_]{2,100}"))
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid channel re-name",
                    Description = $"`{bchname}` is an invalid channel name, as it either ~ \n1) Contains non-allowed characters\n 2) Is too long",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var cha = GetChannel(args[0]);
            await cha.ModifyAsync(i => i.Name = bchname);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Channel Name Updated!!",
                Description = $"<#{cha.Id}> is now set!!!",
                Color = Blurple
            }.WithCurrentTimestamp());
            return;
        }
    }
}