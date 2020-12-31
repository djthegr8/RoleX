using Discord;
using Public_Bot;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;

namespace RoleX.Modules
{
    [DiscordCommandClass("Emote Editor", "For complete management of server emotes!")]
    public class Emdelete : CommandModuleBase
    {
        [RequiredUserPermissions(new[] { GuildPermission.ManageGuild, GuildPermission.ManageEmojis})]
        [DiscordCommand("emdelete", description ="Deletes given emoji.", example ="emdelete kekw", commandHelp ="emrename emoji_name")]
        public async Task EMDEL(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What emoji to delete?",
                    Description = $"The way to run this cmd is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}emdelete emote_name`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (await GetEmote(args[0]) == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What emoji to delete?",
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
                Description = $"The emoji was deleted :/",
                Color = Blurple
            }.WithCurrentTimestamp());
        }
    }
}