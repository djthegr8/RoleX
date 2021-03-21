using System;
using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;
using static RoleX.Modules.Services.SqliteClass;

namespace RoleX.Modules.Moderation
{
    [DiscordCommandClass("Moderation", "Basic Moderation for yer server!")]
    public class Unban : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.BanMembers)]
        [DiscordCommand("unban", commandHelp = "unban <@user>", example = "unban ForgivenDude", description = "Unbans given user")]
        public async Task Unbn(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "Mention the user you wish to unban",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var bu = await GetBannedUser(args[0]);
            if (bu == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = $"`{args[0]}` isn't a previously banned user!?",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            await Context.Guild.RemoveBanAsync(bu);
            await AddToModlogs(Context.Guild.Id, bu.Id, Context.User.Id, Punishment.Unban, DateTime.Now);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = $"{bu.Username}#{bu.Discriminator} unbanned succesfully!",
                Description = $"Unban successful! Welcome them back!",
                Color = Blurple
            }.WithCurrentTimestamp());
        }
    }
}
