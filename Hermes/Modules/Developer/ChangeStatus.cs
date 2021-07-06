using System.Linq;
using System.Threading.Tasks;
using Discord;
using Hermes.Modules.Services;

namespace Hermes.Modules.Developer
{
    [DiscordCommandClass("Developer", "Dev commands that you can't use 🤣!")]
    public class SetStatus : CommandModuleBase
    {
        [Alt("setstatus")]
        [DiscordCommand("status", commandHelp = "", description = "")]
        public async Task GPe(params string[] args)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                var join = string.Join(' ', args);
                await Program.Client.SetGameAsync(join);
                await Context.Message.AddReactionAsync(new Emoji("✔"));
            }
        }
    }
}