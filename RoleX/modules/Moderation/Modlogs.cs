using Discord;
using Discord.Rest;
using RoleX.Modules.Services;
using System.Collections.Generic;
using System.Linq;
using RoleX.Utilities;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static RoleX.Modules.Services.SqliteClass;

namespace RoleX.Modules.Moderation
{
    [DiscordCommandClass("Moderation", "Basic Moderation for yer server!")]
    public class Modlogs : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.KickMembers)]

        [DiscordCommand("modlogs", commandHelp = "modlogs <@user>", description = "Shows all modlogs of a user", example = "modlogs @WeirdMan")]
        public async Task MLogs(params string[] aa)
        {
            if (aa.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What user?",
                    Description = "Mention the user you wish to get modlogs of",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            IUser user;
            if (await GetUser(aa[0]) == null)
            {
                var regex = new Regex(@"(\d{18}|\d{17})");
                if (regex.IsMatch(aa[0]))
                {
                    var aadrc = new DiscordRestClient();
                    await aadrc.LoginAsync(TokenType.Bot, Program.token);
                    var aala = await aadrc.GetUserAsync(ulong.Parse(regex.Match(aa[0]).Groups[1].Value));
                    if (aala == null)
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "What user?",
                            Description = "That user isn't valid :(",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                        return;
                    }
                    user = aala;
                }
                else if (Context.Message.MentionedUsers.Any())
                {
                    user = Context.Message.MentionedUsers.First();
                }
                else
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "What user?",
                        Description = "That user isn't valid :(",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
            }
            else
            {
                user = await GetUser(aa[0]);
            }
            EmbedBuilder emb = new EmbedBuilder
            {
                Title = $"Modlogs for user {user.Username}#{user.Discriminator}",
                Color = Blurple,
                ThumbnailUrl = user.GetAvatarUrl(size: 64)
            };
            var eb = new List<EmbedFieldBuilder>();
            foreach (Infraction i in await GetUserModlogs(Context.Guild.Id, user.Id))
            {
                eb.Add(new EmbedFieldBuilder() {
                    Name=Infraction.GetPunishment(i.Punishment),
                    Value=$"**Mod:** <@{i.ModeratorId}>\n**Date:** {i.Time.ToUniversalTime().ToShortDateString()}\n**Time: **{(i.Time.ToUniversalTime().TimeOfDay.Hours <= 12 ? i.Time.ToUniversalTime().TimeOfDay.Hours : i.Time.ToUniversalTime().TimeOfDay.Hours - 12)}:{i.Time.ToUniversalTime().TimeOfDay.Minutes} {(i.Time.ToUniversalTime().TimeOfDay.Hours < 12 ? "AM" : "PM")}\n**Reason:** {i.Reason}",
                    IsInline=true });
            }
            if (eb.Count == 0)
            {
                emb.Description = "They've been a good user! No modlogs :)";
            }
            var pm = new PaginatedMessage(PaginatedAppearanceOptions.Default, Context.Message.Channel, new PaginatedMessage.MessagePage { Description = "Error!" });
            pm.SetPages($"Here's a list of the user's modlogs", eb, 7);
            await pm.Resend();
        }
    }
}