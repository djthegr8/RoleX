using System.Linq;
using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;
using RoleX.Utilities;

namespace RoleX.Modules.Moderation
{
    [DiscordCommandClass("Moderation", "Basic Moderation for yer server!")]
    public class Bans : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageGuild)]
        [DiscordCommand("bans", commandHelp = "bans", description = "Shows the bans in the server")]
        public async Task RBans(params string[] _)
        {
            var mbed = new EmbedBuilder()
            {
                Title = "List of bans in the server",
                Color = Blurple
            }.WithCurrentTimestamp();
            var listwherenotnull = (await Context.Guild.GetBansAsync()).ToList();
            listwherenotnull.RemoveAll(k => k == null);
            var efb = listwherenotnull.Select((r5, idx) => new EmbedFieldBuilder()
            {
                Name = r5.User.ToString() == "" ? "Probably deleted" : r5.User.ToString(),
                Value = string.Join("", r5.Reason == null ? "None given" : r5.Reason.Take(2000))
            });
            var pm = new PaginatedMessage(PaginatedAppearanceOptions.Default, Context.Channel);
            pm.SetPages("Here are your bans", efb, 5);
            await pm.Resend();
        }
    }
}