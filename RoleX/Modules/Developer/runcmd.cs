using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using RoleX.Modules.Services;

namespace RoleX.Modules.Developer
{
    [DiscordCommandClass("Developer", "Dev commands that you can't use 🤣!")]
    public class RunCmd : CommandModuleBase
    {
        [Alt("restartrolex")]
        [DiscordCommand("runcmd", commandHelp = "", description = "")]
        public async Task RunCmdCommand(params string[] args)
        {
            var bk = string.Join(' ', args);
            if (devids.Any(x => x == Context.User.Id))
            {
                await ReplyAsync("ok doing tysm");
                Process.Start(@"..\..\..\..\restartrolex.bat");
            }
        }
    }
}