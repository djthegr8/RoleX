using System.Linq;
using System.Threading.Tasks;
using Discord;
using Hermes.Modules.Services;

namespace Hermes.Modules.Developer
{
    [DiscordCommandClass("Developer", "Dev commands that you can't use 🤣!")]
    public class PushToAllServers : CommandModuleBase
    {
        [DiscordCommand("pushToAllServers", commandHelp = "", description = "")]
        public async Task Push(params string[] args)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                var x = string.Join(' ', args);
                var splitted = x.Split(',');
                var title = splitted[0];
                var description = string.Join(' ', splitted.Skip(1));
                foreach (var server in Program.Client.Guilds)
                {
                    if (server.Id == 629798125879558154) continue;
                    var channel = server.DefaultChannel;
                    try
                    {
                        await channel.SendMessageAsync("", false, new EmbedBuilder
                        {
                            Title = title,
                            Description = description,
                            Color = Blurple,
                            ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
                        }.WithCurrentTimestamp().Build());
                    }
                    catch
                    {
                        await ReplyAsync($"Couldn't write in {server.Name}");
                    }
                }
            }
        }
    }
}