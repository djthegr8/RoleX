using System.Linq;
using System.Threading.Tasks;
using Discord;
using MoreLinq;
using RoleX.Modules.Services;

namespace RoleX.Modules.Developer
{
    [DiscordCommandClass("Developer", "Dev commands that you can't use 🤣!")]
    public class AdPrem : CommandModuleBase
    {
        [Alt("addpremium")]
        [DiscordCommand("addprem", commandHelp = "", description = "")]
        public async Task GPe(ulong a, params string[] _)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                var guild = Program.Client.Guilds.First(al => al.Id == a);
                if (guild == null)
                {
                    await ReplyAsync("Why are you like this <:noob:756055614861344849>");
                    return;
                }
                if (await SqliteClass.PremiumOrNot(guild.Id))
                {
                    await ReplyAsync("Making a server premium 2 times gets u nothing so shut up and die");
                    return;
                }
                await SqliteClass.NonQueryFunctionCreator(
                    $"UPDATE prefixes SET Premium = 1 WHERE guildid = {Context.Guild.Id};");
                await ReplyAsync($"Made the server {guild.Name} premium, will DM owner with the good news!");
                var embed = new EmbedBuilder
                {
                    Title = "This server is now Premium!",
                    Description =
                        "Thank you for supporting RoleX.\nWe are able to develop our bot due to supportive servers like yours!",
                    Color = Blurple
                }.WithCurrentTimestamp().Build();
                try
                {
                    await guild.Owner.SendMessageAsync(embed: embed);
                }
                catch
                {
                    // let it be, let it beeee
                }

                try
                {
                    var sc =  guild.SystemChannel;
                    if (sc != null)
                    {
                        await sc.SendMessageAsync(embed: embed);
                    }
                    else
                    {
                        await guild.DefaultChannel.SendMessageAsync(embed:embed);
                    }

                }
                catch
                {
                    // let it be, let it beee
                }
                devids.ForEach(id => Program.Client.GetUser(id).SendMessageAsync($"The server {guild.Name} just went premium <a:vibing:782998739865305089>"));
            }
        }
    }
}