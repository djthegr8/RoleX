using System.Threading.Tasks;
using System.Linq;
using Hermes.Modules.Services;

namespace Hermes.Modules.General
{
    [DiscordCommandClass("General", "General commands for all!")]
    public class CInvites : CommandModuleBase
    {
		[RequiredUserPermissions(GuildPermission.ManageGuild, GuildPermission.ManageRoles)]
		[Alt("clearinvites")]
        [DiscordCommand("cin", commandHelp = "cin", description = "Clears all invites!")]
        public async Task RPing(params string[] _)
        {
            await Context.Channel.TriggerTypingAsync();
			foreach (var invite in Context.Guild.Invites) {
				await invite.DeleteAsync();
			}
            await ReplyAsync(
                $"Deleted all invites successfully!");
        }
    }
}
