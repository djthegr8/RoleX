using System.Linq;
using System.Threading.Tasks;
using Hermes.Modules.Services;

namespace Hermes.Modules.Developer
{
    [DiscordCommandClass("Developer", "Dev commands that you can't use 🤣!")]
    public class Sqlitenonquery : CommandModuleBase
    {
        [DiscordCommand("sqlitenonquery", commandHelp = "", description = "", example = "")]
        public async Task SQNQ(params string[] args)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                var f = string.Join(' ', args);
                await SqliteClass.NonQueryFunctionCreator(f);
            }
        }
    }
}