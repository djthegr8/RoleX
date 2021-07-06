using System.Linq;
using System.Threading.Tasks;
using Hermes.Modules.Services;

namespace Hermes.Modules.Developer
{
    [DiscordCommandClass("Developer", "Dev commands that you can't use 🤣!")]
    public class StupidServer : CommandModuleBase
    {
        [DiscordCommand("stupidServer", commandHelp = "", description = "")]
        public async Task weird(params string[] args)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                if (args.Length == 0 || !ulong.TryParse(args[0], out var _))
                {
                    await ReplyAsync("Why are you like this <:noob:756055614861344849>");
                    return;
                }

                var x = ulong.Parse(args[0]);
                try
                {
                    await Program.Client.GetGuild(x).SystemChannel
                        .SendMessageAsync("Leaving this server <:catthumbsup:780419880385380352>");
                }
                catch
                {
                }

                await Program.Client.GetGuild(x).LeaveAsync();
            }
        }
    }
}