using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Public_Bot;
using System.Threading.Tasks;
using Discord;
using GuildPermissions = Public_Bot.GuildPermissions;
namespace TradeMemer.modules
{
    [DiscordCommandClass("Channel Permission","Edit Channel-wise perms of a Role using these commands!")]
    class ChannelPermission: CommandModuleBase
    {
        [GuildPermissions(GuildPermission.ManageChannels)]
        
        [DiscordCommand("channelperms",commandHelp ="channelperms <#channel> <@role/@user> <Permission> <yes,no,inherit>",description ="Edits the Channel-wise perms of the given Role or Member",example ="channelperms @Moderator viewChannel no")]
        public async Task ChannelPermEdit(params string[] args)
        {
            bool roleOrNot;
            PermValue ovr;
            Discord.WebSocket.SocketRole srl;
            Discord.WebSocket.SocketUser sus;
            switch (args.Length)
            {
                case 0 or 1 or 2 or 3:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters!",
                        Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}channelperms <#channel> <@role/@member> <Permission> <yes,no,inherit>`",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
            }
            var channe = GetChannel(args[0]);
            if (channe == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid channel name",
                    Description = $"`{args[0]}` could not be parsed as channel!",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            if (Context.Message.MentionedRoles.Any())
            {
                roleOrNot = true;
                srl = Context.Message.MentionedRoles.First();
            } else if (Context.Message.MentionedUsers.Any())
            {
                roleOrNot = false;
                sus = Context.Message.MentionedUsers.First();
            }
            else
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "No User/Role Mentioned",
                    Description = $"We couldn't find any role or user mentioned!",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            var prm_ = GetChannelPermission(args[2]);
            if (prm_.Item2 == false)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "That permission is invalid",
                    Description = $"The list of permissions is ~ ```{string.Join('\n', Enum.GetNames(typeof(ChannelPermissions)))}```",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            var prm = prm_.Item1;
            Console.WriteLine(prm);
            var inh = args[3].ToLower();
            switch (inh)
            {
                case "yes" or "true" or "postive" or "y":
                    ovr = PermValue.Allow;
                    break;
                case "no" or "false" or "negative" or "n":
                    ovr = PermValue.Deny;
                    break;
                case "inherit" or "i":
                    ovr = PermValue.Inherit;
                    break;
                default:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "That overwrite type is invalid",
                        Description = $"For giving the permission, use `y`, `yes`, `positive` or `true`.\nFor Inheriting use `i` or `inherit`\nAnd for revoking use `n`, `no`, `negative` or `false` as the last parameter for the command",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
            }
            Console.WriteLine("todo");
        }
    }
}
