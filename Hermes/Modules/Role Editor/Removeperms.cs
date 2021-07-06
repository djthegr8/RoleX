using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hermes.Modules.Services;

namespace Hermes.Modules.Role_Editor
{
    [DiscordCommandClass("Role Editor", "Class for editing of Roles!")]
    public class Removeperms : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageRoles)]
        [DiscordCommand("removeperms", commandHelp = "removeperms <@role/id> <Permission>",
            description = "Remove the given permission from the requested role")]
        [Alt("rperms")]
        public async Task RemovePerms(params string[] args)
        {
            switch (args.Length)
            {
                case 0 or 1:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters",
                        Description =
                            $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}removeperms <@role/id> <Permission>`",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
            }

            var roleA = GetRole(args[0]);
            if (roleA == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "That role is invalid",
                    Description =
                        $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}removeperms <@role/id> <Permission>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            if (Context.Guild.CurrentUser.Roles.All(idk => idk.CompareTo(roleA) < 0))
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Hey, thats above me",
                    Description =
                        $"The bot's highest role => {Context.Guild.CurrentUser.Roles.Max().Name}\nThe role you wish to delete => {roleA.Name}",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            if (!(Context.User as SocketGuildUser).Roles.Any(rl => rl.Position > roleA.Position) &&
                Context.Guild.OwnerId != Context.User.Id && devids.All(k => k != Context.User.Id))
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Oops!",
                    Description = "You're below the role you want to edit!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            var gp = GetPermission(args[1]);
            if (gp.Item2 == false)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "That permission is invalid",
                    Description =
                        $"The list of permissions is ~ ```{string.Join('\n', Enum.GetNames(typeof(GuildPermission)))}```",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            await roleA.ModifyAsync(rl => rl.Permissions = EditPerm(roleA, gp.Item1, false));
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Permission removed From Role!",
                Description = $"Permission `{args[1]}` revoked from `{roleA.Name.ToUpper()}`",
                Color = Blurple
            }.WithCurrentTimestamp());
        }
    }
}