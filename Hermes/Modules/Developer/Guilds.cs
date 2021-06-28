using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Hermes.Modules.Services;

namespace Hermes.Modules.Developer
{
    [DiscordCommandClass("Developer", "Dev commands that you can't use 🤣!")]
    public class Guilds : CommandModuleBase
    {
        [DiscordCommand("guilds", commandHelp = "", description = "")]
        public async Task RGuilds(params string[] _)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                string st = "```";
                foreach (var srver in Program.Client.Guilds)
                {
                    try
                    {
                        /*string inv;
                        try
                        {
                            inv = (await srver.GetInvitesAsync()).First().Url;
                        }
                        catch { inv = "No Perms LMAO!"; }*/
                        /*st += $"{srver.Name}\t{inv}\n";*/
                        st += $"{srver.Name} (ID: {srver.Id})\n{srver.MemberCount} members (Perms: {(srver?.CurrentUser?.GuildPermissions == null ? "idk" : srver?.CurrentUser?.GuildPermissions)})\n";
                    } catch { }
                }
                st += "```";
                string filePath = "nice.txt";
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    sw.WriteLine(st);
                }
                await Context.Channel.SendFileAsync(filePath,
                    embed: new EmbedBuilder
                    {
                        Title = $"All Hermes Guilds LMAO (total: {Program.Client.Guilds.Count})",
                        Description = st.Length < 2000 ? st : "Ig i sent it as a file",
                        Color = Blurple
                    }.WithCurrentTimestamp().Build());
            }
        }
    }
}