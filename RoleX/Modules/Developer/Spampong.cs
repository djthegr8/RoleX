using System.Linq;
using System.Threading.Tasks;
using RoleX.Modules.Services;

namespace RoleX.Modules.Developer
{
    [DiscordCommandClass("Developer", "Dev commands that you can't use 🤣!")]
    public class SpamPong : CommandModuleBase
    {
        [DiscordCommand("spampong", commandHelp = "", description = "")]
        public async Task GPe(string a, int y, int delay = 500)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                if (await GetUser(a) == null)
                {
                    await ReplyAsync("Why are you like this <:noob:756055614861344849> invalid user");
                    return;
                }
                for (int i = 0; i < y; i++)
                {
                    await ReplyAsync((await GetUser(a)).Mention);
                    await Task.Delay(delay);
                }

                await ReplyAsync("ok done");
            }
        }
    }
}