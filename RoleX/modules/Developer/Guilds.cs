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
    public class Guilds : CommandModuleBase
    {
        [DiscordCommand("guilds", commandHelp = "", description = "")]
        public async Task RGuilds(params string[] _)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                string st = "```";
                foreach (var srver in Context.Client.Guilds)
                {

                    /*string inv;
                    try
                    {
                        inv = (await srver.GetInvitesAsync()).First().Url;
                    }
                    catch { inv = "No Perms LMAO!"; }*/
                    /*st += $"{srver.Name}\t{inv}\n";*/
                    st += $"{srver.Name} (ID: {srver.Id})\n{srver.MemberCount} members (Perms: {srver.CurrentUser.GuildPermissions.RawValue})\n";
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
                        Title = $"All RoleX Guilds LMAO (total: {Context.Client.Guilds.Count})",
                        Description = st.Length < 2000 ? st : "Ig i sent it as a file",
                        Color = Blurple
                    }.WithCurrentTimestamp().Build());
            }
        }
    }
}