using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;
using RoleX.Utilities;

namespace RoleX.Modules.Role_Editor
{
    [DiscordCommandClass("Role Editor", "Class for editing of Roles!")]
    public class Allroles : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.Administrator)]
        [DiscordCommand("allroles", description = "Lists all roles", commandHelp = "allroles", example = "allroles")]
        public async Task AllRoles(params string[] _)
        {
            //string rlx = "```" + string.Join('\n', Context.Guild.Roles.OrderByDescending(x => x.Position).Select(x => $"{x.Name} ID: {x.Id}")) + "```";
            var paginatedMessage = new PaginatedMessage(PaginatedAppearanceOptions.Default, Context.Channel, new PaginatedMessage.MessagePage("Loading..."))
            {
                Title = $"All roles in {Context.Guild.Name}",
                Timestamp = DateTimeOffset.Now,
                Color = Blurple
            };
            var embb = new List<EmbedFieldBuilder>();
            for (int y = 0; y < Context.Guild.Roles.Count; y++)
            {
                var x = Context.Guild.Roles.OrderByDescending(x => x.Position).ElementAt(y);
                embb.Add(new EmbedFieldBuilder()
                {
                    Name = x.Name,
                    Value = $"ID: {x.Id}\nPermValue: [{x.Permissions.RawValue}](http://discordapi.com/permissions.html#{x.Permissions.RawValue})\n",
                    IsInline = (y % 2 == 0)
                });
            };
            paginatedMessage.SetPages("Here's a list of all roles in the Server", embb, 5);
            await paginatedMessage.Resend();
        }
    }
}