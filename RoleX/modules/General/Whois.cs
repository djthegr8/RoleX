using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Public_Bot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace RoleX.Modules
{
    [DiscordCommandClass("General", "General commands for all!")]
    public class Whois : CommandModuleBase
    {
        [DiscordCommand("whois", description = "Shows information about the mentioned user", commandHelp = "whois <@user>", example = "whois DJ001")]
        public async Task WhoIs(params string[] user)
        {
            RestUser userAccount;
            SocketGuildUser userGuildAccount = null;
            if (user.Length == 0)
            {
                userGuildAccount = Context.User as SocketGuildUser;
                userAccount = await Program.CL2.GetUserAsync(userGuildAccount.Id);
            }
            else
            {
                userGuildAccount = await GetUser(user[0]);
                userAccount = userGuildAccount == null ? null : await Program.CL2.GetUserAsync(userGuildAccount.Id);
            }

            if (userAccount == null)
            {
                if (ulong.TryParse(user[0], out ulong ide))
                {
                    userAccount = await Program.CL2.GetUserAsync(ide);
                }
                if (userAccount == null)
                {
                    EmbedBuilder error = new EmbedBuilder()
                    {
                        Title = "That user is invalid ¯\\_(ツ)_/¯",
                        Description = "Please provide a valid user",
                        Color = Color.Red
                    };
                    await Context.Channel.SendMessageAsync("", false, error.Build());
                    return;
                }
            }
            string mutualServers = "";
            var gwUser = Program.Client.GetUser(userAccount.Id);
            if (gwUser != null &&
                gwUser.MutualGuilds.Count >= 1 &&
                gwUser.MutualGuilds.Any(ree => Context.User.MutualGuilds.Any(r2 => r2.Id == ree.Id))
                )
            {
                var dry = gwUser.MutualGuilds.Where(ree => Context.User.MutualGuilds.Any(r2 => r2.Id == ree.Id));
                foreach (var gld in dry.Take(5)) {
                    await gld.DownloadUsersAsync();
                    var gUser = gld.GetUser(userAccount.Id);
                    mutualServers += $"{(string.IsNullOrEmpty(gUser.Nickname) ? "" : $"`{gUser.Nickname}` in ")}" + $"**{gld.Name}** ({gld.Id})\n";
                }
                mutualServers += dry.Count() <= 5 ? "" : $"and {dry.Count() - 5} other(s)";
            }
            string perms = "```\n";
            string permsRight = "";
            var orderedroles = userGuildAccount == null ? null : userGuildAccount.Roles.OrderBy(x => x.Position * -1).ToArray();
            string roles = "";
            if (orderedroles != null)
            {
                for (int i = 0; i < orderedroles.Count(); i++)
                {
                    var role = orderedroles[i];
                    if (roles.Length + role.Mention.Length < 120)
                        roles += role.Mention + "\n";
                    else
                    {
                        roles += $"+ {orderedroles.Length - i + 1} more";
                        break;
                    }
                }
            }
            string stats = $"{(userGuildAccount == null ? "" : ($"Nickname: {(userGuildAccount.Nickname == null ? "None" : userGuildAccount.Nickname)}\n"))}" +
                              $"Id: {userAccount.Id}\n" +
                              $"Creation Date: {userAccount.CreatedAt.UtcDateTime:r}\n";
            EmbedBuilder whois = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = userAccount.ToString() + (Program.Client.GetUser(userAccount.Id) != null ? (" (" + Program.Client.GetUser(userAccount.Id).Status.ToString() + ")") : ""),
                    IconUrl = userAccount.GetAvatarUrl()
                },
                Color = Blurple,
                Description = "**Stats**\n" + stats,
                Fields = userGuildAccount == null ? new List<EmbedFieldBuilder>() : new List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder()
                    {
                        Name="Roles",
                        Value = roles
                    }
                }
            }.WithCurrentTimestamp();
            if (mutualServers != "") whois.AddField("Shared Servers", mutualServers, true);
            await Context.Channel.SendMessageAsync("", false, whois.Build());
        }
    }
}