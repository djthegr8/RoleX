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