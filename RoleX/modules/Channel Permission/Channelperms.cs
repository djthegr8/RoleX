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
    public class Channelperms : CommandModuleBase
    {
        [Alt("chperms")]
        [RequiredUserPermissions(GuildPermission.ManageChannels)]
        [DiscordCommand("channelperms", commandHelp = "channelperms <#channel> <@role/@user> <Permission> <yes,no,inherit>", description = "Edits the Channel-wise perms of the given Role or Member", example = "channelperms @Moderator viewChannel no")]
        public async Task ChannelPermEdit(params string[] args)
        {
            bool roleOrNot;
            PermValue ovr;
            SocketRole srl;
            SocketUser sus;
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Insufficient Parameters!",
                    Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}channelperms <#channel> <@role/@member> <Permission> <yes,no,inherit>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var channe = GetChannel(args[0]);
            if (channe == null)
            {
                /*await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid channel name",
                    Description = $"`{args[0]}` could not be parsed as channel!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;*/
                channe = Context.Channel as SocketGuildChannel;
                var argsL = args.ToList();
                argsL.Insert(0, "");
                args = argsL.ToArray();
            }
            switch (args.Length)
            {
                case 0 or 1 or 2 or 3:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters!",
                        Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}channelperms <#channel> <@role/@member> <Permission> <yes,no,inherit>`",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
            }
            sus = await GetUser(args[1]);
            srl = GetRole(args[1]);
            if (sus == null && srl == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid Role/User",
                    Description = $"We couldn't find any role or user from `{args[1]}`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            else if (sus == null)
            {
                roleOrNot = true;
            }
            else if (srl == null)
            {
                roleOrNot = false;
            }
            else
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Multiple Possibilities Detected",
                    Description = $"Given `{args[1]}`, we found both a Role and a User.\n**Role Found:**\n{srl.Mention}\n**User Found**\n{sus.Mention}\nPlease use a mention instead of a search query!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            /*
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
                }.WithCurrentTimestamp());
                return;
            }*/
            var prm_ = GetChannelPermission(args[2]);
            if (prm_.Item2 == false)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "That permission is invalid",
                    Description = $"The list of permissions is ~ ```{string.Join('\n', Enum.GetNames(typeof(Discord.ChannelPermission)))}```",
                    Color = Color.Red
                }.WithCurrentTimestamp());
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
                        Description = $"For assigning the permission, use `y`, `yes`, `positive` or `true`.\nFor Inheriting use `i` or `inherit`\nAnd for revoking use `n`, `no`, `negative` or `false` as the last parameter for the command",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
            }
            OverwritePermissions op;
            if (roleOrNot)
            {
                op = channe.GetPermissionOverwrite(srl) != null ? (OverwritePermissions)channe.GetPermissionOverwrite(srl) : new OverwritePermissions();
                op = GetOP(prm, ovr, op);
                await channe.AddPermissionOverwriteAsync(srl, op);
            }
            else
            {
                op = channe.GetPermissionOverwrite(sus) != null ? (OverwritePermissions)channe.GetPermissionOverwrite(sus) : new OverwritePermissions(); ;
                op = GetOP(prm, ovr, op);
                await channe.AddPermissionOverwriteAsync(sus, op);
            }
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Overwrite added successfully!",
                Description = $"Channel Overwrite added for <#{channe.Id}>",
                Color = Blurple
            }.AddField("Overwrite Details", $"For: {(roleOrNot ? srl.Mention : sus.Mention)}\nPermission: {prm}\nValue: {ovr}")
            .WithCurrentTimestamp());
            return;
        }
    }
}