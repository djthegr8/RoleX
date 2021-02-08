using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;
using RoleX.Utilities;

namespace RoleX.Modules.General
{
    [DiscordCommandClass("General", "General commands for all")]
    internal class Alias : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.Administrator)]
        [DiscordCommand("alias", commandHelp = "alias <add/remove> <alias-name> <cmd-and-parameters>", description = "Adds an alias to a command or usage of a command. Note that the prefix must NOT be given.", example = "alias doggy nick dj dog")]
        public async Task AliasCommand(params string[] args)
        {
            if (args.Length == 0 || args.Length == 1)
            {
                /*
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Insufficient Parameters",
                    Description = $"The way to use the command is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}alias <alias-name> <cmd-and-parameters>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());*/
                var paginatedMessage = new PaginatedMessage(new PaginatedAppearanceOptions
                    {
                        FooterFormat = $"Do {await SqliteClass.PrefixGetter(Context.Guild.Id)}help alias to know more about adding aliases"
                    }, Context.Channel,
                    new PaginatedMessage.MessagePage("Error :/"));
                var loembb = (await SqliteClass.GuildAliasGetter(Context.Guild.Id)).Select(k => new EmbedFieldBuilder()
                {
                    Name = k.Item1,
                    Value = $"`{k.Item2}`",
                }).ToList();
                if (!loembb.Any())
                {
                    await ReplyAsync("", false, new EmbedBuilder()
                    {
                        Title = "No aliases yet",
                        Description = "Add aliases using `alias add <alias-name> <cmd-and-parameters>`",
                        Color = Blurple
                    });
                    return;
                }
                paginatedMessage.SetPages("Here's a list of aliases that your server has", fields:loembb, fieldsLimit:7);
                await paginatedMessage.Resend();
                return;
            }
            switch (args[0])
            {
                case "add" or "+":
                {
                    if (args.Length == 2)
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "Insufficient Parameters",
                            Description = $"The way to add aliases is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}alias + <alias-name> <cmd-and-parameters>`",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                        return;
                    }
                    var cmdAlias = args[1];
                    var cmd = string.Join(' ', args.Skip(2));
                    cmd = cmd.Replace("^", "\\^").Replace("|", "\\|");
                    await SqliteClass.AliasAdder(Context.Guild.Id, cmdAlias, cmd);
                    await ReplyAsync("", false, new EmbedBuilder()
                    {
                        Title = $"Successfully added alias {cmdAlias}",
                        Description = "Run `alias` to find the list of aliases in your guild!",
                        Color = Blurple
                    });
                    break;
                }
                case "remove" or "-":
                {
                    var didItExist = await SqliteClass.AliasRemover(Context.Guild.Id, args[1]) != 0;
                    if (!didItExist) await ReplyAsync("", false, new EmbedBuilder()
                    {
                        Title = $"No such alias",
                        Description = "Run `alias` to find the list of aliases in your guild!",
                        Color = Blurple
                    });
                    else await ReplyAsync("", false, new EmbedBuilder()
                    {
                        Title = $"Deleted alias",
                        Description = "Run `alias` to find the list of aliases in your guild!",
                        Color = Blurple
                    });
                    break;
                }
                default:
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Invalid Parameters",
                        Description = $"The way to add aliases is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}alias + <alias-name> <cmd-and-parameters>`\nand removing is\n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}alias - <alias-name>`",
                        Color = Color.Red
                    }.WithCurrentTimestamp()); 
                    break;
                }
            }
        }
    }
}
