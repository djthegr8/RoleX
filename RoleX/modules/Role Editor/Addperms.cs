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
    public class Addperms : CommandModuleBase
    {
        [DiscordCommand("addperms", commandHelp = "addperms <@role/id> <Permission>", description = "Adds the given permission to the requested role")]
        public async Task AddPerms(params string[] args)
        {
            switch (args.Length)
            {
                case 0 or 1:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "U need to give the Role and Permission",
                        Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}addperms <@role/id> <Permission>`",
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
                    Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}addperms <@role/id> <Permission>`",
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
            if (!(Context.User as SocketGuildUser).Roles.Any(rl => rl.Position > roleA.Position) && Context.Guild.OwnerId != Context.User.Id)
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
            await roleA.ModifyAsync(rl => rl.Permissions = EditPerm(roleA, gp.Item1, true));
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = $"Permission added to Role!",
                Description = $"`{roleA.Name}` now has the permission `{args[1]}`",
                Color = Blurple
            }.WithCurrentTimestamp());
            return;
        }
    }
}