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
    public class PushToAllServers : CommandModuleBase
    {
        [DiscordCommand("pushToAllServers", commandHelp = "", description = "")]
        public async Task Push(params string[] args)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                string x = string.Join(' ', args);
                string[] splitted = x.Split(',');
                string title = splitted[0];
                string description = string.Join(' ', splitted.Skip(1));
                foreach (var server in Context.Client.Guilds)
                {
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
                    finally { }
                }
            }
        }
    }
}