using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using RoleX.Modules.Services;

namespace RoleX.Modules.Channel_Permission
{
    [DiscordCommandClass("Channel Editor", "Edit Channel-wise perms of a Role using these commands!")]
    public class Overwrites : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageChannels)]
        [Alt("ow")]
        [DiscordCommand("overwrites", commandHelp = "overwrites <#channel>", description = "Shows the Channel-wise overwrites", example = "overwrites #channel")]
        public async Task Os(params string[] args)
        {
            var channe = Context.Channel as SocketGuildChannel;
            SocketGuildChannel channez = null;
            if (args.Length > 0) channez = GetChannel(args[0]);
            if (channez == null) channez = channe;
            var eb = new EmbedBuilder
            {
                Title = "Permission Overwrites",
                Color = Blurple
            }.AddField("Channel", $"<#{channez.Id}>");
            var pos = channez.PermissionOverwrites;
            string rpos = "";
            var i = 0;
            foreach (var ov in pos.Where(x => x.TargetType == PermissionTarget.Role))
            {
                i++;
                var allowstr = string.Join('\n', ov.Permissions.ToAllowList().Select(x => $"{x}"));
                var deniedstr = string.Join('\n', ov.Permissions.ToDenyList().Select(x => $"{x}"));
                Console.WriteLine(i % 2);
                eb.AddField(GetRole(ov.TargetId.ToString()) == null ? "everyone" : GetRole(ov.TargetId.ToString()).Name,$"```\nAllowed Permissions\n{(allowstr == "" ? "None" : allowstr)}\nDenied Permissions\n{(deniedstr == "" ? "None" : deniedstr)}```\n", i == 3 ? false : true);
                if (i == 3) i = -1;
            }
            string upos = "";
            foreach (var ov in pos.Where(x => x.TargetType == PermissionTarget.User))
            {
                i++;
                var allowstr = string.Join('\n', ov.Permissions.ToAllowList().Select(x => $"{x}"));
                var deniedstr = string.Join('\n', ov.Permissions.ToDenyList().Select(x => $"{x}"));
                Console.WriteLine(i % 2);
                eb.AddField((await GetUser(ov.TargetId.ToString())).ToString(), $"```\nAllowed Permissions\n{(allowstr == "" ? "None" : allowstr)}\nDenied Permissions\n{(deniedstr == "" ? "None" : deniedstr)}```\n", i == 3 ? false : true);
                if (i == 3) i = -1;
            }
            if (rpos != "") eb.AddField("Role Overwrites", rpos);
            if (upos != "") eb.AddField("User Overwrites", upos);
            await ReplyAsync(embed:eb.WithCurrentTimestamp());

        }
    }
}
