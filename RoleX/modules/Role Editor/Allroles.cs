using Discord;
using Discord.WebSocket;
using Public_Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GuildPermissions = Public_Bot.GuildPermissions;
namespace RoleX.Modules
{
    [DiscordCommandClass("Role Editor", "Class for editing of Roles!")]
    public class Allroles : CommandModuleBase
    {
        [DiscordCommand("allroles", description = "Lists all roles", commandHelp = "allroles", example = "allroles")]
        public async Task AllRoles(params string[] _)
        {
            //string rlx = "```" + string.Join('\n', Context.Guild.Roles.OrderByDescending(x => x.Position).Select(x => $"{x.Name} ID: {x.Id}")) + "```";
            var mbed = new EmbedBuilder
            {
                Title = $"All roles in {Context.Guild.Name}",
                Color = Blurple
            }.WithCurrentTimestamp();
            for (int y = 0; y < Context.Guild.Roles.Count; y++)
            {
                var x = Context.Guild.Roles.OrderByDescending(x => x.Position).ElementAt(y);
                mbed.Fields.Add(new EmbedFieldBuilder()
                {
                    Name = x.Name,
                    Value = $"ID: {x.Id}\nPermValue: [{x.Permissions.RawValue}](http://discordapi.com/permissions.html#{x.Permissions.RawValue})\n",
                    IsInline = (y % 2 == 0)
                });
            };
            Console.WriteLine(mbed.Fields.Count);
            await ReplyAsync("", false, mbed);
        }
    }
}