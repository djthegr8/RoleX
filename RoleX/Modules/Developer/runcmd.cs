using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using RoleX.Modules.Services;

namespace RoleX.Modules.Developer
{
    [DiscordCommandClass("Developer", "Dev commands that you can't use 🤣!")]
    public class RunCmd : CommandModuleBase
    {
        [DiscordCommand("runcmd", commandHelp = "", description = "")]
        public async Task RunCmdCommand(params string[] args)
        {
            var bk = string.Join(' ', args);
            if (devids.Any(x => x == Context.User.Id))
            {
                ProcessStartInfo startInfo = new() { FileName = "/bin/bash", Arguments = bk };
                Process proc = new() { StartInfo = startInfo, };
                var re = proc.Start();
                await ReplyAsync(re.ToString());
            }
        }
    }
}