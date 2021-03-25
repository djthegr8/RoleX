using Discord;
using MoreLinq;
using RoleX.Modules.Services;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace RoleX.Modules.Developer
{
    [DiscordCommandClass("Developer", "Dev commands that you can't use 🤣!")]
    public class WhatGuild : CommandModuleBase
    {
        [Alt("whatguild")]
        [DiscordCommand("whatg", commandHelp = "", description = "")]
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

                await ReplyAsync("", false, new EmbedBuilder() {Title = guild.Name, Description = await guild.GetInfoString(), Color = Blurple, ThumbnailUrl = guild.IconUrl});
            }
        }
    }

    public static class GetInfoStr
        {
            public static async Task<string> GetInfoString(this SocketGuild guild)
            {
                await guild.DownloadUsersAsync();
                return $@"
Server Owner: <@{guild.OwnerId}> (ID: `{guild.OwnerId}`)
Server ID: `{guild.Id}`
Member Count: {guild.MemberCount}
Role Count: {guild.Roles.Count}
Channel Count: {guild.Channels.Count}
Date Created: {guild.CreatedAt.DateTime:D}
Icon: [Click here]({guild.IconUrl})
Banner: {guild.BannerUrl ?? "None"}
Vanity Url Code: {guild.VanityURLCode ?? "None"}
Boosts: {guild.PremiumTier}
Number Of Admins: {guild.Users.Count(Usr => Usr.GuildPermissions.Administrator && !Usr.IsBot)}
Number Of Mods: {guild.Users.Count(Usr => (Usr.GuildPermissions.ManageChannels || Usr.GuildPermissions.ManageGuild || Usr.GuildPermissions.ManageRoles) && !Usr.GuildPermissions.Administrator)}
Number Of Bots: {guild.Users.Count(Usr => Usr.IsBot)}
Raw Perms: `{(guild.CurrentUser == null ? "None" : guild.CurrentUser.GuildPermissions.RawValue)}`
";
            }
        }
    }
