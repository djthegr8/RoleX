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
    public class Dump : CommandModuleBase
    {
        [DiscordCommand("dump", commandHelp = "dump @role", description = "Literally dumps the people with that role", example = "dump @Dumdums")]
        public async Task D(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid Parameters",
                    Description = "You need to provide a role, I can't read your mind (yet)",
                    Color = Color.Red
                }.WithCurrentTimestamp()
                );
                return;
            }
            var x = GetRole(args[0]);
            if (x == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid Role",
                    Description = $"Couldn't parse `{args[0]}` as role",
                    Color = Color.Red
                }.WithCurrentTimestamp()
                );
                return;
            }
            await ReplyAsync($"```All Users with the role {x.Name} (ID: {x.Id})\n" + string.Join('\n', x.Members.Select(x => x.Username + "#" + x.Discriminator + " (ID: " + x.Id + ")")) + "```");
        }
    }
}