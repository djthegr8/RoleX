using System.Linq;
using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;

namespace RoleX.Modules.General
{
    [DiscordCommandClass("General", "General commands for all!")]
    public class Setup : CommandModuleBase
    {
        [DiscordCommand("setup", commandHelp ="setup", description ="Helps set the bot up!")]
        public async Task RSetup(params string[] _)
        {
            var x = "";
            x += $"Admin:        {(Context.Guild.CurrentUser.GuildPermissions.Administrator ? "✅" : "❌")}\n";
            x += $"Kick:         {(Context.Guild.CurrentUser.GuildPermissions.KickMembers ? "✅" : "❌")}\n";
            x += $"Ban:          {(Context.Guild.CurrentUser.GuildPermissions.BanMembers ? "✅" : "❌")}\n";
            x += $"Mention:      {(Context.Guild.CurrentUser.GuildPermissions.MentionEveryone ? "✅" : "❌")}\n";
            x += $"Manage Guild: {(Context.Guild.CurrentUser.GuildPermissions.ManageGuild ? "✅" : "❌")}\n";
            x += $"Messages:     {(Context.Guild.CurrentUser.GuildPermissions.ManageMessages ? "✅" : "❌")}\n";
            x += $"Channels:     {(Context.Guild.CurrentUser.GuildPermissions.ManageChannels ? "✅" : "❌")}\n";
            x += $"Roles:        {(Context.Guild.CurrentUser.GuildPermissions.ManageRoles ? "✅" : "❌")}\n";
            x += $"Webhooks:     {(Context.Guild.CurrentUser.GuildPermissions.ManageWebhooks ? "✅" : "❌")}\n";
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Setting Up RoleX",
                ThumbnailUrl = Context.Client == null ? "" : Context.Client.CurrentUser.GetAvatarUrl(),
                Description = (await SqliteClass.PremiumOrNot(Context.Guild.Id)) ? "Whoa you're premium 🤩" : "RoleX is a bot that requires various permissions to do various tasks.",
                Fields = {new EmbedFieldBuilder
                {
                    Name = "Permissions",
                    Value = $"```{x}```"
                } },
                Color = Context.Guild.CurrentUser.GuildPermissions.Administrator ? Color.Green : (x.Count(k => k == '✅') == 7 ? Color.Green : Color.Red),
                Footer = new EmbedFooterBuilder
                {
                    Text = "Command Inspired from LuminousBot (ID: 722435272532426783)"
                }
            }.WithCurrentTimestamp()
            );
        }
    }
}