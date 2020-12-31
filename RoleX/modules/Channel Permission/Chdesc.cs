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
    public class Chdesc : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageChannels)]
        [DiscordCommand("chdesc", commandHelp = "chdesc <#channel> <multi-word-string>")]
        [Alt("topic")]
        [Alt("rchd")]
        [Alt("channeldescription")]
        public async Task ReDescChannel(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Insufficient Parameters!",
                    Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}channeldesc <#channel> <new-channel-name>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var cha = GetChannel(args[0]);
            if (cha == null)
            {
                cha = Context.Channel as SocketGuildChannel;
                var argsL = args.ToList();
                argsL.Insert(0, "");
                args = argsL.ToArray();
            }
            var bchname = string.Join(' ', args.Skip(1));
            await (cha as SocketTextChannel).ModifyAsync(d => d.Topic = bchname);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Channel Description Updated!!",
                Description = $"<#{cha.Id}> is now set with its new topic!!!",
                Color = Blurple
            }.WithCurrentTimestamp());
            return;
        }
    }
}