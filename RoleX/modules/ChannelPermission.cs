using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Public_Bot;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using GuildPermissions = Public_Bot.GuildPermissions;
namespace TradeMemer.modules
{
    [DiscordCommandClass("Channel Editor","Edit Channel-wise perms of a Role using these commands!")]
    class ChannelPermission: CommandModuleBase
    {
        [GuildPermissions(GuildPermission.ManageChannels)]
        [Alt("catrename")]
        [DiscordCommand("categoryrename",commandHelp ="categoryrename <old-category-name> <new-category-name>",description ="Renames given category",example ="categoryrename Trading Xtreme Trading")]
        public async Task CatRename(params string[] args)
        {
            if (args.Length < 2)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Insufficient Parameters!",
                    Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}categoryrename <old-category-name> <new-category-name>`",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            var alf = GetCategory(args[0]);
            if (alf == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid category",
                    Description = $"`{args[0]}` could not be parsed as category!",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            await alf.ModifyAsync(x => x.Name = string.Join(' ',args.Skip(1)));
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Rename successful!",
                Description = $"Your category was renamed to `{string.Join(' ',args.Skip(1))}`",
                Color = Blurple
            }.WithCurrentTimestamp().Build());
            return;
        }
        [GuildPermissions(GuildPermission.ManageChannels)]
        [Alt("catdelete")]
        [DiscordCommand("categorydelete", commandHelp = "categoryrdelete <category-name>", description = "Deletes given category", example = "categorydelete Useless")]
        public async Task CatDel(string aa)
        {
            var alf = GetCategory(aa);
            if (alf == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid category",
                    Description = $"`{aa}` could not be parsed as category!",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            await alf.DeleteAsync();
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Delete successful!",
                Description = $"Your category was deleted along with all its channels",
                Color = Blurple
            }.WithCurrentTimestamp().Build());
            return;
        }
        [GuildPermissions(GuildPermission.ManageChannels)]
        [DiscordCommand("chrename", commandHelp ="chrename <#channel> <multi-word-string>")]
        [Alt("channelrename")]
        public async Task RenameChannel(params string[] args)
        {
            if (args.Length == 0 || args.Length == 1)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Insufficient Parameters!",
                    Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}channelrename <#channel> <new-channel-name>`",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            if (GetChannel(args[0]) == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid channel",
                    Description = $"`{args[0]}` could not be parsed as channel!",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            var bchname = string.Join('-', args.Skip(1));
            if (!System.Text.RegularExpressions.Regex.IsMatch(bchname,"[a-z0-9-_]{2,100}")) {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid channel re-name",
                    Description = $"`{bchname}` is an invalid channel name, as it either ~ \n1) Contains non-allowed characters\n 2) Is too long",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            var cha = GetChannel(args[0]);
            await cha.ModifyAsync(i => i.Name = bchname);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Channel Name Updated!!",
                Description = $"<#{cha.Id}> is now set!!!",
                Color = Blurple
            }.WithCurrentTimestamp().Build());
            return;
        }
        [GuildPermissions(GuildPermission.ManageChannels)]
        [DiscordCommand("chdesc", commandHelp = "chdesc <#channel> <multi-word-string>")]
        [Alt("chtopic")]
        [Alt("channeldescription")]
        public async Task ReDescChannel(params string[] args)
        {
            if (args.Length == 0 || args.Length == 1)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Insufficient Parameters!",
                    Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}channeldesc <#channel> <new-channel-name>`",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            if (GetChannel(args[0]) == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid channel",
                    Description = $"`{args[0]}` could not be parsed as channel!",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            var bchname = string.Join(' ', args.Skip(1));
            var cha = GetChannel(args[0]);
            await (cha as SocketTextChannel).ModifyAsync(d => d.Topic = bchname);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Channel Description Updated!!",
                Description = $"<#{cha.Id}> is now set with its new topic!!!",
                Color = Blurple
            }.WithCurrentTimestamp().Build());
            return;
        }
        [GuildPermissions(GuildPermission.ManageChannels)]
        [Alt("chperms")]
        
        [DiscordCommand("channelperms",commandHelp ="channelperms <#channel> <@role/@user> <Permission> <yes,no,inherit>",description ="Edits the Channel-wise perms of the given Role or Member",example ="channelperms @Moderator viewChannel no")]
        public async Task ChannelPermEdit(params string[] args)
        {
            bool roleOrNot;
            PermValue ovr;
            SocketRole srl;
            SocketUser sus;
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
            sus = GetUser(args[1]);
            srl = GetRole(args[1]);
            if (sus == null && srl == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid Role/User",
                    Description = $"We couldn't find any role or user from `{args[2]}`",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            } else if (sus == null)
            {
                roleOrNot = true;
            } else if (srl == null)
            {
                roleOrNot = false;
            } else
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Multiple Possibilities :open_mouth:",
                    Description = $"Given `{args[1]}`, we found both a Role and a User.\n**Role Found:**\n{srl.Mention}\n**User Found**\n{sus.Mention}\nPlease use a mention instead of a search query!",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
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
                }.WithCurrentTimestamp().Build());
                return;
            }*/
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
            var op = GetOP(prm, ovr);
            if (roleOrNot)
            {
                await channe.AddPermissionOverwriteAsync(srl, op);
            } else
            {
                await channe.AddPermissionOverwriteAsync(sus, op);
            }
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Overwrite added successfully!",
                Description = $"Channel Overwrite added for <#{channe.Id}>",
                Color = Color.Red
            }.AddField("Overwrite Details",$"For: {(roleOrNot ? srl.Mention : sus.Mention)}\nPermission: {prm}\nValue: {ovr}")
            .WithCurrentTimestamp().Build());
            return;
        }
        [GuildPermissions(GuildPermission.ManageChannels)]

        [DiscordCommand("overwrites", commandHelp = "overwrites <#channel> <@role/@user>", description = "Shows the Channel-wise overwrites of the given Role or Member", example = "overwrites @Moderator")]
        public async Task Os(params string[] args)
        {
            bool roleOrNot;
            SocketUser sus;
            SocketRole srl;
            string pos;
            switch (args.Length)
            {
                case 0 or 1:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters!",
                        Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}overwrites <#channel> <@role/@member>`",
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
            sus = GetUser(args[1]);
            srl = GetRole(args[1]);
            if (sus == null && srl == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid Role/User",
                    Description = $"We couldn't find any role or user from `{args[1]}`",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
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
                    Title = "Multiple Possibilities :O",
                    Description = $"Given `{args[1]}`, we found both a Role and a User.\n**Role Found:**\n{srl.Mention}\n**User Found**\n{sus.Mention}\nPlease use a mention instead of a search query!",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            if (roleOrNot)
            {
                var po = channe.GetPermissionOverwrite(srl);
                if (po == null)
                {
                    pos = $"There are no permission overwrites for {srl.Mention} in <#{channe.Id}>";
                } else
                {
                    pos = po.Value.ToAllowList().Count > 0 ? "✅ " : "" + string.Join("\n✅", po.Value.ToAllowList()) + "\n" + "❌ " + string.Join("\n❌ ", po.Value.ToDenyList());
                }
            } else
            {
                var po = channe.GetPermissionOverwrite(sus);
                if (po == null)
                {
                    pos = $"There are no permission overwrites for {sus.Mention} in <#{channe.Id}>";
                }
                else
                {
                    pos = po.Value.ToAllowList().Count > 0 ? "✅ ": "" + string.Join("\n✅", po.Value.ToAllowList()) + "\n" + (po.Value.ToDenyList().Count > 0 ? "❌ ": "") + string.Join("\n❌ ", po.Value.ToDenyList());
                }
            }
            Console.WriteLine(pos);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Permission Overwrites",
                Color = Blurple
            }.AddField("Channel",$"<#{channe.Id}>")
            .AddField(roleOrNot ? "Role": "User",roleOrNot ? srl.Mention : sus.Mention)
            .AddField("Overwrites",$"```{pos}```")
            .WithCurrentTimestamp().Build());
        }
    }
}
