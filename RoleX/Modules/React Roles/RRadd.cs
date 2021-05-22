using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;

namespace RoleX.Modules.React_Roles
{
    [DiscordCommandClass("Reactions", "A class with Intuitive Reaction Roles")]
    internal class RRAdd : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageRoles)]
        [DiscordCommand("readd",
            commandHelp = "readd <message-link> <emoji> <role>", 
            description = "Adds a reaction role", 
            example = "readd https://discord.com/channels/591660163229024287/790477735352336384/798021230774321162 :weirdemoji: Weirds",
            IsPremium = true
            )]
        public async Task RRaddCommand(params string[] args)
        {
            switch (args.Length)
            {
                case 0 or 1 or 2:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters",
                        Description = $"The way to use the command is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}readd <link-to-message> <emoji> <role>`",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
            }
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
            var role = GetRole(args[2]);
            if (role == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Which role?",
                    Description = $"Couldn't parse `{args[2]}` as a role :(",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var reros = (await SqliteClass.GetReactRoleAsync($"SELECT * FROM reactroles WHERE ChannelID = {chnlid} AND MessageID = {msgid}"));
            SqliteClass.ReactRole rero;
            if (reros.Count == 0)
            {
                rero = new SqliteClass.ReactRole
                {
                    ChannelId = chnlid,
                    MessageId = msgid,
                    GuildId = Context.Guild.Id,
                    Emojis = new() { em.ToString() },
                    Roles = new() { role.Id },
                    BlackListedRoles = new ulong[] { },
                    WhiteListedRoles = new ulong[] { },
                    Unique = false,
                    SelfDestructTime = DateTime.MinValue
                };
            }
            else
            {
                rero = reros[0];
                rero.Roles.Add(role.Id);
                rero.Emojis.Add(em.ToString());
            }
            await SqliteClass.AddOrUpdateReactRole(rero);
            await message.AddReactionAsync(em);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Reaction Role added successfully!",
                Description = $"[Jump]({args[0]})",
                Color = Blurple
            }.WithCurrentTimestamp());
        }
    }
}
