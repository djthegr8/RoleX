using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using RoleX.Modules.Services;

namespace RoleX.Modules.Role_Editor
{
    [DiscordCommandClass("Role Editor", "Class for editing of Roles!")]
    public class Role : CommandModuleBase
    {
        [Alt("add")]
        [Alt("remove")]
        [RequiredUserPermissions(GuildPermission.ManageRoles)]
        [DiscordCommand("role", commandHelp = "role <@user> <@role>", description = "Adds/Removes the role to the given user", example = "role @DJ001 @Criminal")]
        public async Task Additive(params string[] args)
        {
            switch (args.Length)
            {
                case 0 or 1:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters",
                        Description = $"The way to use the command is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}role <@user> <@role>`",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
            }
            SocketUser uzi;
            SocketRole role;
            if (Context.Message.MentionedRoles.Any() && Context.Message.MentionedUsers.Any())
            {
                role = Context.Message.MentionedRoles.First();
                uzi = Context.Message.MentionedUsers.First();

            }
            else if (Context.Message.MentionedUsers.Any())
            {
                uzi = Context.Message.MentionedUsers.First();
                short aa;
                if (args[0].Contains(uzi.Id.ToString()))
                {
                    aa = 1;
                }
                else
                {
                    aa = 0;
                }
                role = GetRole(args[aa]);
                if (role == null)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "What role!?",
                        Description = $"We couldn't parse `{aa}` as role!?",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
            }
            else if (Context.Message.MentionedRoles.Any())
            {
                role = Context.Message.MentionedRoles.First();
                short aa;
                if (args[0].Contains(role.Id.ToString()))
                {
                    aa = 1;
                }
                else
                {
                    aa = 0;
                }
                uzi = await GetUser(args[aa]);
                if (uzi == null)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "What user!?",
                        Description = $"We couldn't parse `{aa}` as user!?",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
            }
            else
            {
                if (await GetUser(args[0]) != null && GetRole(args[0]) == null)
                {
                    uzi = await GetUser(args[0]);
                    role = GetRole(args[1]);
                    if (role == null)
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "What role!?",
                            Description = $"We couldn't parse `{args[1]}` as role!?",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                        return;
                    }
                }
                else if (GetRole(args[0]) != null && await GetUser(args[0]) == null)
                {
                    role = GetRole(args[0]);
                    uzi = await GetUser(args[1]);
                    if (uzi == null)
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "What user!?",
                            Description = $"We couldn't parse `{args[1]}` as user!?",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                        return;
                    }
                }
                else if ((GetRole(args[0]) != null && await GetUser(args[0]) != null) ||
                         (GetRole(args[1]) != null && await GetUser(args[1]) != null))
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Multiple Possibilities Detected",
                        Description =
                            $"Given {(await GetUser(args[0]) == null ? args[1] : args[0])}\n**Role Found:**\n{(GetRole(args[0]) == null ? GetRole(args[1]).Mention : GetRole(args[0]).Mention)}\n**User Found**\n{((await GetUser(args[0])) == null ? (await GetUser(args[1])).Mention : (await GetUser(args[0])).Mention)}\nPlease use a mention instead of a search query, or put # after the user's name so we can find them!!",
                        Color = Color.Red,
                    }.WithCurrentTimestamp());
                    return;
                }
                else
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "What user and role!?",
                        Description = "We couldn't parse either!?",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
            }

            if (role.Position >= (Context.User as SocketGuildUser).Roles.Max().Position && devids.All(k => k != Context.User.Id))
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Could not add role",
                        Description = $"Your highest role, **{(Context.User as SocketGuildUser).Roles.Max().Name}** is below the role you wish to give, **{role.Name}**",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
                if (Context.Guild.CurrentUser.Roles.All(idk => idk.CompareTo(role) < 0))
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Specified role is above me!",
                        Description = $"The bot's highest role => {Context.Guild.CurrentUser.Roles.Max().Name}\nThe role you wish to add => {role.Name}",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
                if ((uzi as SocketGuildUser).Roles.Any(a => a.Id == role.Id))
                {
                    await (uzi as SocketGuildUser).RemoveRoleAsync(role);
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = $"Removed {role} from {uzi}!",
                        Description = "Role removal successful!",
                        Color = Blurple
                    }.WithCurrentTimestamp());
                }
                else
                {
                    await (uzi as SocketGuildUser).AddRoleAsync(role);
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = $"Added {role} to {uzi}!",
                        Description = "Role addition successful!",
                        Color = Blurple
                    }.WithCurrentTimestamp());
                }
        }

        }
    }
