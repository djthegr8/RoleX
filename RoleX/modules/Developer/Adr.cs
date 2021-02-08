using System.Linq;
using System.Threading.Tasks;
using RoleX.Modules.Services;

namespace RoleX.Modules.Developer
{
    [DiscordCommandClass("Developer", "Dev commands that you can't use 🤣!")]
    public class Adr : CommandModuleBase
    {
        [DiscordCommand("adr", commandHelp = "", description = "")]
        public async Task GPe(ulong a, ulong y)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                var breh = Program.Client.Guilds.First(al => al.Id == a);
                if (breh == null)
                {
                    await ReplyAsync("Why are you like this <:noob:756055614861344849>");
                    return;
                }
                var irdk = breh.GetUser(Context.User.Id);
                if (irdk == null)
                {
                    await ReplyAsync("Why are you like this <:noob:756055614861344849>");
                    return;
                }
                await irdk.AddRoleAsync(breh.GetRole(y));
            }
        }
    }
}