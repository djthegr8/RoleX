using Discord;
using Discord.WebSocket;
using Public_Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RoleX.Modules
{
    [DiscordCommandClass("Role Editor", "Class for editing of Roles!")]
    public class Delete : CommandModuleBase
    {
        [Alt("del")]
        [RequiredUserPermissions(new[] { GuildPermission.ManageGuild, GuildPermission.ManageRoles})]
        [DiscordCommand("delete", commandHelp = "delete <@role/id>", description = "Deletes the mentioned role", example = "delete @DumbRole")]
        public async Task DelRole(params string[] args)
        {
            SocketRole DeleteRole;
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What role?",
                    Description = "Mention the role you wish to delete",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            DeleteRole = GetRole(args[0]);
            if (DeleteRole == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What role?",
                    Description = "Mention the role you wish to delete",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (Context.Guild.CurrentUser.Roles.All(idk => idk.CompareTo(DeleteRole) < 0))
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Hey, thats above me",
                    Description = $"The bot's highest role => {Context.Guild.CurrentUser.Roles.Max().Name}\nThe role you wish to delete => {DeleteRole.Name}",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (!(Context.User as SocketGuildUser).Roles.Any(rl => rl.Position > DeleteRole.Position) && Context.Guild.OwnerId != Context.User.Id)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Oops!",
                    Description = "You're below the role you want to delete!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            else
            {
                var nm = DeleteRole.Name;
                await DeleteRole.DeleteAsync();
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = $"Role deleted successfully!",
                    Description = $"The role `{nm}` was successfully deleted",
                    Color = Blurple
                }.WithCurrentTimestamp());
            }
        }
    }
}