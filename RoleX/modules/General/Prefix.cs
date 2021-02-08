using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;

namespace RoleX.Modules.General
{
    [DiscordCommandClass("General", "General commands for all!")]
    public class Prefix : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageGuild)]
        [DiscordCommand("prefix", commandHelp ="prefix <newprefix>", description ="Changes the prefix!", example ="prefix !")]
        public async Task Pre(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Existing Prefix",
                    Description = $"The current prefix is {await SqliteClass.PrefixGetter(Context.Guild.Id)}",
                    Color = Blurple,
                    Footer = new EmbedFooterBuilder()
                    {
                        Text= $"Do {await SqliteClass.PrefixGetter(Context.Guild.Id)}prefix <prefix> to change it!"
                    }
                }.WithCurrentTimestamp());
                return;
            }
            await SqliteClass.PrefixAdder(Context.Guild.Id, args[0]);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Prefix Updated",
                Description = $"The updated prefix is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}`",
                Color = Blurple,
                Footer = new EmbedFooterBuilder
                {
                    Text = "Bot nickname updated to reflect prefix changes"
                }
            }.WithCurrentTimestamp());
            await Context.Guild.CurrentUser.ModifyAsync(async dood => dood.Nickname = $"[{await SqliteClass.PrefixGetter(Context.Guild.Id)}] RoleX");
            return;
        }
    }
}