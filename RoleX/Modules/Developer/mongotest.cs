using System.Linq;
using System.Threading.Tasks;
using RoleX.Modules.Services;

namespace RoleX.Modules.Developer
{
    [DiscordCommandClass("Developer", "Dev commands that you can't use 🤣!")]
    public class MongoTest : CommandModuleBase
    {
        [Alt("mgtest")]
        [DiscordCommand("mongotes", commandHelp = "", description = "")]
        public async Task GPe(params string[] _)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                await Services.MongoDB.RemindersToMongo();
                await ReplyAsync("Mk");
            }
        }
    }
}