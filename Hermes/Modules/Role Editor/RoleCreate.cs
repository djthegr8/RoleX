using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Hermes.Modules.Services;
using Color = Discord.Color;

namespace Hermes.Modules.Role_Editor
{
    [DiscordCommandClass("Role Editor", "Class for editing of Roles")]
    internal class RoleCreate : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageRoles)]
        [DiscordCommand("rcreate", commandHelp = "rcreate <name> <color>",
            description = "Creates role with given name and color", example = "rcreate WeirdRole #bb86fc")]
        public async Task RCreate(params string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters",
                        Description =
                            $"The way to use the command is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}rcreate <name> <color?>`",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                case 1:
                    var role = await Context.Guild.CreateRoleAsync(args[0]);
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Role creation successful!",
                        Description = $"Successfully created role <@&{role.Id}> (ID: {role.Id})",
                        Color = Blurple
                    }.WithCurrentTimestamp());
                    return;
                case 2:
                    var rle = await Context.Guild.CreateRoleAsync(args[0]);
                    var c = new ColorConverter();
                    var col = new System.Drawing.Color();
                    var hasC = false;
                    var hArgs1 = args[1][0] != '#' ? $"#{args[1]}" : args[1];
                    if (Regex.IsMatch(hArgs1, "^(#[0-9A-Fa-f]{3})$|^(#[0-9A-Fa-f]{6})$"))
                    {
                        col = (System.Drawing.Color) c.ConvertFromString(hArgs1);
                        hasC = true;
                    }
                    else
                    {
                        var svc = (TypeConverter.StandardValuesCollection) c.GetStandardValues();
                        foreach (System.Drawing.Color o in svc)
                            if (o.Name.Equals(args[1], StringComparison.OrdinalIgnoreCase))
                            {
                                col = (System.Drawing.Color) c.ConvertFromString(args[1]);
                                hasC = true;
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

                    await rle.ModifyAsync(x => x.Color = new Color(col.R, col.G, col.B));
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Role creation successful!",
                        Description = $"Successfully created role <@&{rle.Id}> (ID: {rle.Id})",
                        Color = Blurple
                    }.WithCurrentTimestamp());
                    return;
            }
        }
    }
}