using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hermes.Modules.Services;

namespace Hermes.Modules.Role_Editor
{
    [DiscordCommandClass("Role Editor", "Class for editing of Roles!")]
    public class RColor : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageRoles)]
        [DiscordCommand("color", description = "Changes the color of specified role. We accept color strings as hexadecimals, or from the extensive list [here](https://docs.microsoft.com/en-us/dotnet/api/system.drawing.color?view=net-5.0#properties)", commandHelp = "color <@role> <hex/None>", example = "color @LightPurple #bb86fc")]
        public async Task ChangeRole(params string[] args)
        {
            if (args.Length < 2)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Insufficient Parameters",
                    Description = $"The way to use the command is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}color <@role> <color>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var role = GetRole(args[0]);
            if (role == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "That role is invalid",
                    Description = $"I couldn't parse `{args[0]}` as a role!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (!(Context.User as SocketGuildUser).Roles.Any(rl => rl.Position > role.Position) && Context.Guild.OwnerId != Context.User.Id && devids.All(k => k != Context.User.Id))
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Oops!",
                    Description = "You're below the role you want to edit!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (args[1].ToLower() == "none" || args[1].ToLower() == "invisible")
            {
                await role.ModifyAsync(x => x.Color = new Color());
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Done!!",
                    Description = $"The role {role.Name}'s color is removed!",
                    Color = Blurple
                }.WithCurrentTimestamp());
                return;
            }
            System.Drawing.ColorConverter c = new System.Drawing.ColorConverter();
            System.Drawing.Color col = new System.Drawing.Color();
            bool hasC = false;
            var hArgs1 = args[1][0] != '#' ? $"#{args[1]}" : args[1];
            if (Regex.IsMatch(hArgs1, "^(#[0-9A-Fa-f]{3})$|^(#[0-9A-Fa-f]{6})$"))
            {

                col = (System.Drawing.Color)c.ConvertFromString(hArgs1);
                hasC = true;
            }
            else
            {
                System.ComponentModel.TypeConverter.StandardValuesCollection svc = (System.ComponentModel.TypeConverter.StandardValuesCollection)c.GetStandardValues();
                foreach (System.Drawing.Color o in svc)
                {
                    if (o.Name.Equals(args[1], StringComparison.OrdinalIgnoreCase))
                    {
                        col = (System.Drawing.Color)c.ConvertFromString(args[1]);
                        hasC = true;
                    }
                }
            }
            if (hasC == false)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "What color??",
                    Description = $"Couldn't parse `{args[1]}` as a color!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            await role.ModifyAsync(x => x.Color = new Color(col.R, col.G, col.B));
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Done!!",
                Description = $"The role {role.Name} is now set to the color of this embed!",
                Color = new Color(col.R, col.G, col.B) == new Color(255, 255, 255) ? new Color(254, 254, 254) : new Color(col.R, col.G, col.B)
            }.WithCurrentTimestamp());
            return;
        }
    }
}