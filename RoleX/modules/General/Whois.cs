using System;
using static RoleX.Modules.SqliteClass;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Public_Bot;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using GuildPermissions = Public_Bot.GuildPermissions;
namespace RoleX.Modules
{
    [DiscordCommandClass("General", "General commands for all!")]
    public class Whois : CommandModuleBase
    {
        [DiscordCommand("whois", description = "Shows information about the mentioned user", commandHelp = "whois <@user>", example ="whois DJ001")]
        public async Task WhoIs(params string[] user)
        {
            SocketGuildUser userAccount;
            if (user.Length == 0)
                userAccount = Context.User as SocketGuildUser;
            else userAccount = await GetUser(user[0]);

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
            string perms = "```\n";
            string permsRight = "";
            var props = typeof(Discord.GuildPermissions).GetProperties();
            var boolProps = props.Where(x => x.PropertyType == typeof(bool));
            var pTypes = boolProps.Where(x => (bool)x.GetValue(userAccount.GuildPermissions) == true).ToList();
            var nTypes = boolProps.Where(x => (bool)x.GetValue(userAccount.GuildPermissions) == false).ToList();
            var pd = boolProps.Max(x => x.Name.Length) + 1;
            if (nTypes.Count == 0)
                perms += "Administrator: ✅```";
            else
            {
                foreach (var perm in pTypes)
                    perms += $"{perm.Name}:".PadRight(pd) + " ✅\n";
                perms += "```";
                permsRight = "```\n";
                foreach (var nperm in nTypes)
                    permsRight += $"{nperm.Name}:".PadRight(pd) + " ❌\n";
                permsRight += "```";
            }
            var orderedroles = userAccount.Roles.OrderBy(x => x.Position * -1).ToArray();
            string roles = "";
            for (int i = 0; i < orderedroles.Count(); i++)
            {
                var role = orderedroles[i];
                if (roles.Length + role.Mention.Length < 256)
                    roles += role.Mention + "\n";
                else
                {
                    roles += $"+ {orderedroles.Length - i + 1} more";
                    break;
                }
            }
            string stats = $"Nickname: {(userAccount.Nickname == null ? "None" : userAccount.Nickname)}\n" +
                              $"Id: {userAccount.Id}\n" +
                              $"Creation Date: {userAccount.CreatedAt.UtcDateTime.ToString("r")}\n" +
                              $"Joined At: {userAccount.JoinedAt.Value.UtcDateTime.ToString("r")}\n";

            EmbedBuilder whois = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = userAccount.ToString(),
                    IconUrl = userAccount.GetAvatarUrl()
                },
                Color = Blurple,
                Description = permsRight == "" ? "**Stats**\n" + stats : "",
                Fields = permsRight == "" ? new List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder()
                    {
                        Name = "Roles",
                        Value = roles,
                    }
                } : new List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder()
                    {
                        Name = "Stats",
                        Value = stats,
                        IsInline = true,

                    },
                    new EmbedFieldBuilder()
                    {
                        Name = "Roles",
                        Value = roles,
                        IsInline = false,

                    }
                }
            }.WithCurrentTimestamp();
            await Context.Channel.SendMessageAsync("", false, whois.Build());
        }
    }
}