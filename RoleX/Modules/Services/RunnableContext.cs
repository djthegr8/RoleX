using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace RoleX.Modules.Services
{
    public class CustomCommandGlobals
    {
        public RunnableContext Context { get; set; }
        public async Task ReplyAsync(string message = "", bool isTTS = false, EmbedBuilder eb = null)
        {
            await Context.Channel.SendMessageAsync(message, isTTS, eb?.Build(), null, AllowedMentions.None);
        }
        public static EmbedBuilder CreateEmbed(string Title = "", string Description = "", Color? Color = null, string Url = null, string ImageUrl = null, string FooterText = null, string FooterIconUrl = null)
        {
            return new ()
            {
                Title = Title,
                Color = Color,
                Description = Description,
                Footer = FooterText == null
                    ? new EmbedFooterBuilder
                    {
                        Text = FooterText,
                        IconUrl = FooterIconUrl
                    }
                    : null,
                ImageUrl = ImageUrl,
                Url = Url
            };
        }
        public async Task<GuildEmote> GetEmote(string str, SocketGuild Guild = null)
        {
            Guild ??= Context.Guild;
            var replstr = str.Replace("a:", "").Replace("<", "").Replace(">", "").Replace(":", "");
            Console.WriteLine(replstr);
            if (Guild.Emotes.Any(x => String.Equals(replstr, x.Name, StringComparison.CurrentCultureIgnoreCase))) return Guild.Emotes.First(x => String.Equals(replstr, x.Name, StringComparison.CurrentCultureIgnoreCase));
            Console.WriteLine(replstr);
            try
            {
                var resultString = ulong.Parse(Regex.Match(replstr, @"\d+").Value);

                if (resultString == 0 || await Context.Guild.GetEmoteAsync(resultString) == null)
                {
                    return null;
                }
                return await Guild.GetEmoteAsync(resultString);
            }
            catch { return null; }

        }
        public SocketGuildChannel GetChannel(string name)
        {
            var regex = new Regex(@"(\d{18}|\d{17})");
            if (regex.IsMatch(name))
            {
                var u = Context.Guild.GetChannel(ulong.Parse(regex.Match(name).Groups[1].Value));
                return u is SocketCategoryChannel ? null : u;
            }
            if (ulong.TryParse(name, out var res))
            {
                var x = Context.Guild.Channels.FirstOrDefault(x => x.Id == res);
                return x is SocketCategoryChannel ? null : x;
            }
            else
            {
                var x = Context.Guild.Channels.FirstOrDefault(x => x.Name.ToLower().StartsWith(name.ToLower()));
                return x is SocketCategoryChannel ? null : x;
            }


        }
        public SocketCategoryChannel GetCategory(string name)
        {
            var regex = new Regex(@"(\d{18}|\d{17})");
            if (regex.IsMatch(name))
            {
                var u = Context.Guild.GetCategoryChannel(ulong.Parse(regex.Match(name).Groups[1].Value));
                return u;
            }
            if (ulong.TryParse(name, out var res))
                return Context.Guild.CategoryChannels.FirstOrDefault(x => x.Id == res);
            return Context.Guild.CategoryChannels.FirstOrDefault(x => x.Name.ToLower().StartsWith(name.ToLower()));


        }
        public async Task<SocketGuildUser> GetUser(string user)
        {
            await Context.Guild.DownloadUsersAsync();
            var regex = new Regex(@"(\d{18}|\d{17})");
            if (regex.IsMatch(user))
            {
                var u = Context.Guild.GetUser(ulong.Parse(regex.Match(user).Groups[1].Value));
                return u;
            }

            user = user.ToLower();
            if (Context.Message.MentionedUsers.Any())
            {
                return Context.Message.MentionedUsers.First() as SocketGuildUser;
            }

            if (Context.Guild.Users.Any(x => x.Username.ToLower().StartsWith(user)))
            {
                return Context.Guild.Users.First(x => x.Username.ToLower().StartsWith(user));
            }
            if (Context.Guild.Users.Any(x => x.ToString().ToLower().StartsWith(user)))
            {
                return Context.Guild.Users.First(x => x.ToString().ToLower().StartsWith(user));
            }
            if (Context.Guild.Users.Any(x => x.Nickname != null && x.Nickname.ToLower().StartsWith(user)))
            {
                return Context.Guild.Users.First(x => x.Nickname != null && x.Nickname.ToLower().StartsWith(user));
            }
            return null;
        }
        public async Task<IUser> GetBannedUser(string uname)
        {
            var alr = await Context.Guild.GetBansAsync();
            var regex = new Regex(@"(\d{18}|\d{17})");
            if (regex.IsMatch(uname))
            {
                return alr.FirstOrDefault(aa => aa.User.Id == ulong.Parse(uname))?.User;
            }
            return alr.FirstOrDefault(x => x.User.Username.ToLower().Contains(uname.ToLower()))?.User;
        }

        public CustomCommandGlobals(SocketCommandContext context)
        {
            Context = new(context);
        }

    }
    public class RunnableContext
    {
        public SocketGuild Guild { get; }

        public ISocketMessageChannel Channel { get; }

        public SocketUser User { get; }

        public SocketUserMessage Message { get; }

        public RunnableContext(SocketCommandContext context)
        {
            Guild = context.Guild;
            Channel = context.Channel;
            User = context.User;
            Message = context.Message;
        }
    }

}