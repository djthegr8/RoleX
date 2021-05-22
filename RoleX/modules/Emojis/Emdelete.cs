using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;

namespace RoleX.Modules.Emojis
{
    [DiscordCommandClass("Emote Editor", "For complete management of server emotes!")]
    public class Emdelete : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageEmojis)]
        [DiscordCommand("emdelete", description ="Deletes given emoji.", example ="emdelete kekw", commandHelp ="emrename emoji_name")]
        public async Task EMDEL(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Emoji not provided",
                    Description = $"Command Syntax: `{await SqliteClass.PrefixGetter(Context.Guild.Id)}emdelete emote_name`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (await GetEmote(args[0]) == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Emoji not provided?",
                    Description = $"Couldn't parse `{args[0]}` as an emote",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var i = await GetEmote(args[0]);
            await Context.Guild.DeleteEmoteAsync(i);
            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = "Emoji Deleted Successfully!",
                Description = "The emoji was deleted",
                Color = Blurple
            }.WithCurrentTimestamp());
        }
    }
}
