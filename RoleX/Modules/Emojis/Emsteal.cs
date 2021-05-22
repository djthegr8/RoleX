using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;

namespace RoleX.Modules.Emojis
{
    [DiscordCommandClass("Emote Editor", "For complete management of server emotes!")]
    public class EmSteal : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageEmojis)]
        [DiscordCommand("emsteal", description ="Steals given emoji from given server", example ="emsteal 8325280985332 285098320958583", commandHelp ="emrename <server_id> <emoji/emoji_id>, <emoji2/emoji2_id>, <emoji3/emoji3_id>...")]
        public async Task EMDEL(params string[] args)
        {
            if (args.Length < 2)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What emoji to steal?",
                    Description = $"Command Syntax: `{await SqliteClass.PrefixGetter(Context.Guild.Id)}emsteal server_id <emoji_id>, <emoji2_id>, <emoji3_id>...`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            var fax = ulong.TryParse(args[0], out var gid);
            
            if (!fax)
            {
                var gg = Program.Client.GetGuild(gid);
                if (gg != null) await gg.DownloadUsersAsync();
                if (gg?.GetUser(Context.User.Id) == null)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "What server do I steal from?",
                        Description = "Enter the server ID, and ensure RoleX is in the server",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
            }

            var joined = string.Join(' ', args.Skip(1)).Replace(" ", "");
            var emojis = joined.Split(',');
            var ems = await ReplyAsync("Starting the process of stealing emojis...");
            if (emojis.Length > 14 && !await SqliteClass.PremiumOrNot(Context.Guild.Id))
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Too many emotes!",
                    Description =
                        "Because your server isn't [premium](https://rolex.gitbook.io/rolex/rolex-premium), you're limited to 15 emotes.",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            if (emojis.Length > 29)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Too many emotes!",
                    Description =
                        "Thank you for supporting RoleX by using Premium, but you're limited to 30 emotes (compared to 15 for non-Premium).\n Thanks again!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            WebClient wc = new();
            foreach (var emoji in emojis)
            {
                var emote = await GetEmote(emoji, Program.Client.GetGuild(gid));
                if (emote == null)
                {
                    await ems.ModifyAsync(k => k.Content = ems.Content + $"\nSkipped {emoji}");
                    continue;
                }
                var dd = await wc.DownloadDataTaskAsync(emote.Url);
                var ms = new MemoryStream(dd);
                var ae = await Context.Guild.CreateEmoteAsync(emote.Name, new Image(ms));
                await ems.ModifyAsync(k => k.Content= ems.Content + $"\nSuccessfully added {ae}");
                // Never be in a hurry
                await Task.Delay(200);
                await ms.DisposeAsync();
            }
            
            await ems.ModifyAsync(k => k.Content= ems.Content + "\nFinished successfully!");

        }
    }
}