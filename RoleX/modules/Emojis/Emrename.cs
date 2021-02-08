using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;

namespace RoleX.Modules.Emojis
{
    [DiscordCommandClass("Emote Editor", "For complete management of server emotes!")]
    public class Emrename : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageEmojis)]
        [DiscordCommand("emrename", commandHelp = "emrename :old_emote: new_emote_name", description = "Renames given emoji :)", example ="emrename kek kekw")]
        public async Task EmRename(params string[] args)
        {
            if (args.Length < 2 || await GetEmote(args[0]) == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What emoji and what name?",
                    Description = $"The way to run this cmd is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}emrename old_emote new_emote_name`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var em = await GetEmote(args[0]);
            var strj = string.Join('_', args.Skip(1));
            var regex = new Regex("[^a-zA-Z0-9_]");
            if (strj.Length >= 32 || strj.Length < 2 || regex.IsMatch(strj))
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid emote name!",
                    Description = $"The emote name must contain only underscores and numbers, and has to be atleast 2 and at max 32 characters in length.",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            await Context.Guild.ModifyEmoteAsync(em, k => k.Name = strj);
            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = "Emoji Renamed Successfully!",
                Description = $"The emoji was renamed to `{strj}`",
                Color = Blurple
            }.WithCurrentTimestamp());
        }
    }
}