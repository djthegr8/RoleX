using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;
using static RoleX.Modules.Services.SqliteClass;

namespace RoleX.Modules.Moderation
{
    [DiscordCommandClass("Moderation", "Basic Moderation for yer server!")]
    public class Mutedrole : CommandModuleBase
    {
        [RequiredUserPermissions(new[] { GuildPermission.ManageRoles, GuildPermission.ManageGuild})]
        [DiscordCommand("mutedrole", commandHelp = "mutedrole <create/role>", description = "Sets the roles for mutes", example = "mutedrole create`\n`mutedrole @Muted")]
        public async Task SMutedRole(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The current muted role",
                    Description = $"{(await MutedRoleIdGetter(Context.Guild.Id) == 0 ? "No muted role set" : $"<@&{await MutedRoleIdGetter(Context.Guild.Id)}")}>\n",
                    Color = Blurple,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"To change it, do `{await PrefixGetter(Context.Guild.Id)}muted <@MutedRole>`, and do `{await PrefixGetter(Context.Guild.Id)}muted create` to create a novel one"
                    }
                }.WithCurrentTimestamp());
                return;
            }

            if (args[0].ToLower() == "create")
            {
                var msg = await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Creating muted role.......",
                    Color = Blurple
                }.WithCurrentTimestamp());
                var rl = await Context.Guild.CreateRoleAsync("Muted by RoleX", new Discord.GuildPermissions(), new Color(0, 0, 0), false, null);
                foreach (var chnl in Context.Guild.Channels)
                {
                    await chnl.AddPermissionOverwriteAsync(rl, new OverwritePermissions(sendMessages: PermValue.Deny, speak: PermValue.Deny));
                }
                args[0] = rl.Id.ToString();
                await msg.DeleteAsync();
            }
            if (GetRole(args[0]) == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What role?",
                    Description = $"Couldn't parse `{args[0]}` as role :(",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            await MutedRoleIdAdder(Context.Guild.Id, GetRole(args[0]).Id);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "The updated Muted Role!",
                Description = $"The muted role is now <@&{await MutedRoleIdGetter(Context.Guild.Id)}>",
                Color = Blurple,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"To change it yet again, do `{await PrefixGetter(Context.Guild.Id)}mutedrole <@Role>`"
                }
            }.WithCurrentTimestamp());
        }
    }
}