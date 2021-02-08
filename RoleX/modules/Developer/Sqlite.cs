using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;

namespace RoleX.Modules.Developer
{
    [DiscordCommandClass("Developer", "Dev commands that you can't use 🤣!")]
    public class Sqlite : CommandModuleBase
    {
        [DiscordCommand("sqlite", commandHelp = "", description = "")]
        public async Task Gz(params string[] _)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                await Context.User.SendFileAsync($"..{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}rolex.db");
                await ReplyAsync("Check ur DM!");
            }
        }
    }
}