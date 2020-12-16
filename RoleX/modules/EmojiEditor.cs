using Discord;
using Public_Bot;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using GuildPermissions = Public_Bot.GuildPermissions;
namespace RoleX.modules
{
    [DiscordCommandClass("Emote Editor", "For complete management of server emotes!")]
    class EmojiEditor : CommandModuleBase
    {
        /*
        [DiscordCommand("steal", commandHelp = "steal new_emote_name <:emote:>`\n`steal new_emote_name {attached image}", description = "")]
        public async Task steal(params string[] args)
        {
            //Lets go boiz
            if (args.Length == 0)
            {
                if (Context.Message.Attachments.Any())
                {
                    var at1 = Context.Message.Attachments.First();
                    if (at1.Url.EndsWith("png") || at1.Url.EndsWith("jpg") || at1.Url.EndsWith("jpeg") || at1.Url.EndsWith("gif"))
                    {
                        WebClient wec = new();
                        byte[] bytes = wec.DownloadData(at1.Url);
                        MemoryStream ms = new MemoryStream(bytes);
                        System.Drawing.Image drawer = System.Drawing.Image.FromStream(ms);
                        drawer.s
                    }
                    else
                    {

                    }
                } else
                {

                }
            }*/
        [GuildPermissions(new GuildPermission[] { GuildPermission.ManageGuild, GuildPermission.ManageEmojis })]
        [Alt("emdel")]
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
        [GuildPermissions(new GuildPermission[] { GuildPermission.ManageGuild, GuildPermission.ManageEmojis })]
        [Alt("emre")]
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

