using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;

namespace RoleX.Modules.General
{
    [DiscordCommandClass("General", "General commands for all!")]
    public class Help : CommandModuleBase
    {
        [DiscordCommand("help",commandHelp ="help <command>", description ="Shows the Help Message")]
        public async Task HelpC(params string[] args)
        {
            if (args.Length == 0)
            {

                EmbedBuilder helpAuto = new EmbedBuilder
                {
                    Title = "RoleX Command Help",
                    Color = Blurple
                }
                .WithAuthor("RoleX", Context.Client.CurrentUser.GetAvatarUrl())
                .WithFooter("Made by DJ001 (ID: 701029647760097361) and SLENDER (ID: 615873008959225856)")
                .WithCurrentTimestamp();
                foreach( var aa in CustomCommandService.Modules)
                {
                    if (aa.Key == "Developer") continue;
                    helpAuto.AddField(aa.Key, aa.Value);
                }
                helpAuto.AddField("Need more help?", $"Read our Documentation [here](https://rolex.gitbook.io/rolex/ \"Weird Easter Egg\")\n or join [our support server](https://discord.com/invite/3Uq4WF2RFZ \"Probably weirder one\")!\nFor command-wise help, do `{await SqliteClass.PrefixGetter(Context.Guild.Id)}help <commandname/modulename>`");
                await ReplyAsync(embed: helpAuto);
                return;
            }

            var cmd = args[0];
            var prefixure = await SqliteClass.PrefixGetter(Context.Guild.Id);
            var commandSelected = Commands.FirstOrDefault(x => (x.CommandName.ToLower() == cmd.ToLower() || x.Alts.Any(x => x.ToLower() == cmd.ToLower())) && x.CommandDescription != "");
            if (commandSelected == null)
            {
                var modSelected = CustomCommandService.Modules.Keys.FirstOrDefault(x => x.ToLower().Contains(cmd.ToLower()));
                if (modSelected == null)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "There's no such command or module",
                        Description = $"`{args[0]}` isnt a command or a module!",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
                List<string> LS = new List<string>();
                Commands.FindAll(c => c.ModuleName == modSelected).ForEach(async x => LS.Add($"`{await SqliteClass.PrefixGetter(Context.Guild.Id)}{x.CommandName}`"));
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = $"Module Help for {modSelected}",
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder
                        {
                            Name="Description",
                            Value=CustomCommandService.Modules[modSelected]
                        },
                        new EmbedFieldBuilder{
                            Name="Commands",
                            Value=$"{string.Join("\n", LS)}" 
                        }
                    },
                    Color = Blurple
                }.WithCurrentTimestamp());
                return;
            }
            var aliasStr = prefixure + string.Join($", {prefixure}", commandSelected.Alts);
            var embeds = new EmbedBuilder();
            embeds.AddField("Command", "`" + commandSelected.CommandName + '`');
            embeds.AddField("Description", commandSelected.CommandDescription, true);
            if (!string.IsNullOrEmpty(commandSelected.CommandHelpMessage)) embeds.AddField("Usage", $"`{prefixure}{commandSelected.CommandHelpMessage}`");
            if (!string.IsNullOrEmpty(commandSelected.example)) embeds.AddField("Example", $"`{prefixure}{commandSelected.example}`");
            embeds.AddField("Premium Command?", commandSelected.isPremium ? "[Yes](https://patreon.com/rolexbot)" : "No");
            if (commandSelected.Alts.Count > 0) embeds.AddField("Aliases", aliasStr);
            embeds.AddField("Links", "[Support Server](https://discord.com/invite/3Uq4WF2RFZ) | [Invite link](https://tiny.cc/RoleXAdmin)");
            embeds.Footer = new EmbedFooterBuilder { Text = "Help Command by RoleX" };
            embeds.Color = Blurple;
            if (commandSelected.CommandName == "help")
            {
                embeds.ThumbnailUrl = "https://tiny.cc/spidermanmeme";
            }
            await ReplyAsync("", false, embeds);
        }
    }
}