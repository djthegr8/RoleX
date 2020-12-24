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
    public class Removeperms : CommandModuleBase
    {
        [DiscordCommand("removeperms", commandHelp = "removeperms <@role/id> <Permission>", description = "Remove the given permission from the requested role")]
        [Alt("rperms")]
        public async Task RemovePerms(params string[] args)
        {
            switch (args.Length)
            {
                case 0 or 1:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters",
                        Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}removeperms <@role/id> <Permission>`",
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
                    Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}removeperms <@role/id> <Permission>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (Context.Guild.CurrentUser.Roles.All(idk => idk.CompareTo(roleA) < 0))
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Hey, thats above me",
                    Description = $"The bot's highest role => {Context.Guild.CurrentUser.Roles.Max().Name}\nThe role you wish to delete => {roleA.Name}",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (!(Context.User as SocketGuildUser).Roles.Any(rl => rl.Position > roleA.Position) && Context.Guild.OwnerId != Context.User.Id && Context.User.Id != 701029647760097361 && Context.User.Id != 615873008959225856)
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
                    Description = $"The list of permissions is ~ ```{string.Join('\n', Enum.GetNames(typeof(GuildPermission)))}```",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            await roleA.ModifyAsync(rl => rl.Permissions = EditPerm(roleA, gp.Item1, false));
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = $"Permission removed From Role!",
                Description = $"Permission `{args[1]}` revoked from `{roleA.Name.ToUpper()}`",
                Color = Blurple
            }.WithCurrentTimestamp());
            return;
        }
    }
}