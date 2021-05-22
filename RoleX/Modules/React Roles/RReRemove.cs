using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;

namespace RoleX.Modules.React_Roles
{
    [DiscordCommandClass("Reactions", "A class with intuitive reaction roles")]
    internal class RReRemove : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageRoles)]
        [DiscordCommand("redel", commandHelp = "redel <message-link> <emoji>", description = "Deletes the reaction role associated with the emoji", example = "redel https://discord.com/channels/591660163229024287/790477735352336384/798021230774321162 :weirdemoji:", IsPremium = true)]
        public async Task RoleDel(params string[] args)
        {
            if (args.Length < 2) { await ReplyAsync("You gotta tell me what to remove and where "); return; }
            var reg = new Regex(@"^https:\/\/discord.com\/channels\/[0-9]{17,18}\/[0-9]{17,18}\/[0-9]{17,18}$");
            if (!reg.IsMatch(args[0]))
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Which message?",
                    Description = $"Couldn't parse `{args[0]}` as a Discord Message link",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var dry = args[0].Replace(@"https://discord.com/channels/", "").Split('/');
            var chnlid = ulong.Parse(dry[1]);
            var msgid = ulong.Parse(dry[2]);
            var channel = Context.Guild.GetTextChannel(chnlid);
            if (channel == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Which message?",
                    Description = $"Couldn't parse `{args[0]}` as a Discord Message link\nHint: We cannot find the channel. This might be due to Permissions, or that Channel (and Message) is from another server.",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var message = await channel.GetMessageAsync(msgid);
            if (message == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Which message?",
                    Description = $"Couldn't parse `{args[0]}` as a Discord Message link\nHint: We cannot find the message. This might be due to deletion or permissions.",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            var isEmote = Emote.TryParse(args[1], out Emote emz);
            IEmote em = emz;
            if (isEmote == false)
            {

                var el = new Emoji(args[1]);
                if (el == null)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Which emote?",
                        Description = $"Couldn't parse `{args[1]}` as an emote :(",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
                em = el;
            }
            var reros = (await SqliteClass.GetReactRoleAsync($"SELECT * FROM reactroles WHERE ChannelID = {chnlid} AND MessageID = {msgid}"));
            reros.RemoveAll(x => x.GuildId == Context.Guild.Id && x.Emojis.Contains(em.ToString()));
            await SqliteClass.AddOrUpdateReactRole(reros[0]);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Done",
                Description = "Removed the reaction role",
                Color = Blurple
            }.WithCurrentTimestamp());
        }
    }
}
