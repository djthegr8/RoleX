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
                var escapedArgs = bk.Replace("\"", "\\\"");

                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{escapedArgs}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                var result = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                await ReplyAsync(result);
            }
        }
    }
}