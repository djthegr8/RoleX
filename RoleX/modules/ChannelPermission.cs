using Discord;
using Discord.WebSocket;
using Public_Bot;
using System;
using System.Linq;
using System.Threading.Tasks;
using GuildPermissions = Public_Bot.GuildPermissions;
namespace RoleX.modules
{
    [DiscordCommandClass("Channel Editor", "Edit Channel-wise perms of a Role using these commands!")]
    class ChannelPermission : CommandModuleBase
    {
    [RequiredBotPermission(GuildPermission.ManageChannels)]
        [GuildPermissions(GuildPermission.ManageChannels)]
        [Alt("chdelete")]
        [Alt("chdel")]
        [DiscordCommand("channeldelete", description = "Deletes given channel", example = "channeldelete #WeirdChan", commandHelp = "channeldelete <#channel>")]
        public async Task Cdel(string ags)
        {
            var aaa = GetChannel(ags);
            if (aaa == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid channel",
                    Description = $"`{aaa}` could not be parsed as channel!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            else
            {
                await aaa.DeleteAsync();
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Deleted Channel Successfully",
                    Description = $"Channel `#{aaa.Name}` was deleted!",
                    Color = Blurple
                }.WithCurrentTimestamp()
                );
            }
        }
        [RequiredBotPermission(GuildPermission.ManageChannels)]
        [GuildPermissions(GuildPermission.ManageChannels)]
        [Alt("catrename")]
        [DiscordCommand("categoryrename", commandHelp = "categoryrename <old-category-name> <new-category-name>", description = "Renames given category", example = "categoryrename Trading Xtreme Trading")]
        public async Task CatRename(params string[] args)
        {
            if (args.Length < 2)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Insufficient Parameters!",
                    Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}categoryrename <old-category-name> <new-category-name>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
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
                }.WithCurrentTimestamp());
                return;
            }
            await alf.ModifyAsync(x => x.Name = string.Join(' ', args.Skip(1)));
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Rename successful!",
                Description = $"Your category was renamed to `{string.Join(' ', args.Skip(1))}`",
                Color = Blurple
            }.WithCurrentTimestamp());
            return;
        }
        [RequiredBotPermission(GuildPermission.ManageChannels)]
        [GuildPermissions(GuildPermission.ManageChannels)]
        [Alt("catdelete")]
        [Alt("catdel")]
        [DiscordCommand("categorydelete", commandHelp = "categorydelete <category-name>", description = "Deletes given category and all its channels", example = "categorydelete Useless")]
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
                }.WithCurrentTimestamp());
                return;
            }
            foreach (var ch in alf.Channels)
            {
                await ch.DeleteAsync();
            }
            await alf.DeleteAsync();
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Delete successful!",
                Description = $"Your category was deleted along with all its channels",
                Color = Blurple
            }.WithCurrentTimestamp());
            return;
        }
        [RequiredBotPermission(GuildPermission.ManageChannels)]
        [GuildPermissions(GuildPermission.ManageChannels)]
        [DiscordCommand("chrename", commandHelp = "chrename <#channel> <multi-word-string>")]
        [Alt("channelrename")]
        [Alt("chr")]
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
        [RequiredBotPermission(GuildPermission.ManageChannels)]
        [GuildPermissions(GuildPermission.ManageChannels)]
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
                argsL.Insert(0,"");
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
        [RequiredBotPermission(GuildPermission.ManageChannels)]
        [GuildPermissions(GuildPermission.ManageChannels)]
        [Alt("chperms")]
        [Alt("chp")]

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
        [GuildPermissions(GuildPermission.ManageChannels)]

        [DiscordCommand("overwrites", commandHelp = "overwrites <#channel>", description = "Shows the Channel-wise overwrites", example = "overwrites #channel")]
        public async Task Os(params string[] args)
        {
            /*switch (args.Length)
            {
                case 0 or 1:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters!",
                        Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}overwrites <#channel> <@role/@member>`",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
            }*/
            if (args.Length == 0)
            {
                var Embed = new EmbedBuilder
                {
                    Title = $"Overwrites for <#{Context.Channel.Id}>",
                    Color = Blurple,

                };
            }
            else
            {
                var channe = Context.Channel as SocketGuildChannel;
                var channez = GetChannel(args[0]);
                if (channez != null) channe = channez;
                var pos = channe.PermissionOverwrites;
                string rpos = "```";
                foreach (var ov in pos.Where(x => x.TargetType == PermissionTarget.Role))
                {
                    rpos += $"<@&{ov.TargetId}>\n";
                    rpos += ov.Permissions.ToAllowList().Count > 0 ? "✅ " : "" + string.Join("\n✅", ov.Permissions.ToAllowList()) + "\n" + (ov.Permissions.ToDenyList().Count > 0 ? "❌ " : "") + string.Join("\n❌ ", ov.Permissions.ToDenyList());
                }
                rpos += "```";
                string upos = "```";
                foreach(var ov in pos.Where(x => x.TargetType == PermissionTarget.User))
                {
                    upos += $"<@{ov.TargetId}>\n";
                    upos += ov.Permissions.ToAllowList().Count > 0 ? "✅ " : "" + string.Join("\n✅", ov.Permissions.ToAllowList()) + "\n" + (ov.Permissions.ToDenyList().Count > 0 ? "❌ " : "") + string.Join("\n❌ ", ov.Permissions.ToDenyList());
                }
                upos += "```";
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Permission Overwrites",
                    Color = Blurple
                }.AddField("Channel", $"<#{channe.Id}>")
                .AddField("Role Overwrites", rpos)
                .AddField("User Overwrites", upos)
                .WithCurrentTimestamp());
            }
        }
    }
}
