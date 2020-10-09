using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Public_Bot;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using GuildPermissions = Public_Bot.GuildPermissions;
namespace TradeMemer.modules
{
    [DiscordCommandClass("General","General Commands for all")]
    class General: CommandModuleBase
    {
        [DiscordCommand("altidentify", commandHelp = "altidentify <number-of-alts>", description = "Finds the x users newest to Discord and most probable alts")]
        public async Task Yu(params string[] argz)
        {
            var test = 10;
            if (int.TryParse(argz.FirstOrDefault(), out int retest))
            {
                test = retest;
            }
            if (test >= Context.Guild.MemberCount)
            {
                await Context.Channel.SendMessageAsync("This guild does not have the specified amount of users");
                return;
            }
            var yus = Context.Guild.Users;
            string cty = "```";
            var tenYoungestUsers = yus.ToList();
            tenYoungestUsers.RemoveAll(x => x.IsBot);
            try
            {
                tenYoungestUsers.Sort((prev, next) => DateTimeOffset.Compare(prev.CreatedAt, next.CreatedAt));
            }
            catch
            {
                tenYoungestUsers.Sort((prev, next) => 0);
            }
            tenYoungestUsers.Reverse();
            var current = tenYoungestUsers.GetRange(0, test);
            var pr = current.Max(rx => rx.Username.Length) + '\t';
            current.ForEach(x => cty += (x.Username.PadRight(pr) + $"{x.CreatedAt.Month}/{x.CreatedAt.Day}/{x.CreatedAt.Year}" + '\n'));
            cty += "```";
            var mmbed = new EmbedBuilder
            {
                Title = "Youngest Users in {Context.Guild.Name}",
                Description = cty,
                Color = Blurple
            }.WithCurrentTimestamp().Build();
            await Context.Channel.SendMessageAsync("", false, mmbed);
        }
    
    [DiscordCommand("invite",description ="Invite RoleX to your server!!", commandHelp ="invite")]
        public async Task Invite(params string[] args)
        {
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Invite RoleX to your server!",
                Description = "[**Invite using recommended permission link**](https://discord.com/oauth2/authorize?client_id=744766526225252435&scope=bot&permissions=805334064)\n[**Invite using Admin link for easy setup**](https://discord.com/oauth2/authorize?client_id=744766526225252435&scope=bot&permissions=8)",
                Footer = new EmbedFooterBuilder()
                {
                    Text="Thank you for choosing RoleX"
                },

                Color = Blurple
            }.WithCurrentTimestamp().Build());
        }
        [DiscordCommand("ping",commandHelp ="ping", description ="Finds the latency!")]
        public async Task Ping(params string[] argz)
        {
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync($"***RoleX enters the Discord Universe in {Context.Client.Latency} miliseconds***");
        }
        [GuildPermissions(GuildPermission.ManageGuild)]
        [DiscordCommand("prefix", commandHelp ="prefix <newprefix>", description ="Changes the prefix!", example ="prefix !")]
        public async Task Pre(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Existing Prefix",
                    Description = $"The current prefix is {await SqliteClass.PrefixGetter(Context.Guild.Id)}",
                    Color = Blurple,
                    Footer = new EmbedFooterBuilder()
                    {
                        Text= $"Do {await SqliteClass.PrefixGetter(Context.Guild.Id)}prefix <prefix> to change it!"
                    }
                }.WithCurrentTimestamp().Build());
                return;
            }
            await SqliteClass.PrefixAdder(Context.Guild.Id, args[0]);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Prefix Updated",
                Description = $"The updated prefix is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}`",
                Color = Blurple,
                Footer = new EmbedFooterBuilder
                {
                    Text = "Bot nickname updated to reflect prefix changes"
                }
            }.WithCurrentTimestamp().Build());
            await Context.Guild.CurrentUser.ModifyAsync(async dood => dood.Nickname = $"[{await SqliteClass.PrefixGetter(Context.Guild.Id)}] RoleX");
            return;
        }
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
                .WithFooter("Made by DJ001#0007 (ID: 701029647760097361)")
                .WithCurrentTimestamp();
                foreach( var aa in CustomCommandService.Modules)
                {
                    helpAuto.AddField(aa.Key, aa.Value);
                }
                helpAuto.AddField("Need more help?", $"For command-wise help, do `{await SqliteClass.PrefixGetter(Context.Guild.Id)}help <commandname/modulename>`");
                await ReplyAsync(embed: helpAuto.Build());
                return;
            }
            else
            {
                var cmd = args[0];
                var prefixure = await SqliteClass.PrefixGetter(Context.Guild.Id);
                var commandSelected = Commands.FirstOrDefault(x => (x.CommandName.ToLower() == cmd.ToLower() || x.Alts.Any(x => x.ToLower() == cmd.ToLower())) && x.CommandDescription != "");
                if (commandSelected == null)
                {
                    var modSelected = CustomCommandService.Modules.Keys.First(x => x.ToLower().Contains(cmd.ToLower()));
                    if (modSelected == null)
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "Theres no such command or module",
                            Description = $"`{args[0]}` isnt a command or a module!",
                            Color = Color.Red
                        }.WithCurrentTimestamp().Build());
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
                    }.WithCurrentTimestamp().Build());
                    return;
                }
                var aliasStr = prefixure + string.Join($", {prefixure}", commandSelected.Alts);
                var embeds = new EmbedBuilder();
                embeds.AddField("Command", "`" + commandSelected.CommandName + '`');
                embeds.AddField("Description", commandSelected.CommandDescription, true);
                if (!string.IsNullOrEmpty(commandSelected.CommandHelpMessage)) embeds.AddField("Usage", $"`{prefixure}{commandSelected.CommandHelpMessage}`");
                if (!string.IsNullOrEmpty(commandSelected.example)) embeds.AddField("Example", $"`{prefixure}{commandSelected.example}`");
                if (commandSelected.Alts.Count > 0) embeds.AddField("Aliases", aliasStr);
                //embeds.AddField("Links", "[Support Server](https://discord.gg/PbunDXN) | [Invite link](https://tiny.cc/TMAdmin) | [GitHub](https://tiny.cc/TMGitHub)");
                embeds.Footer = new EmbedFooterBuilder { Text = "Help Command by RoleX" };
                embeds.Color = Blurple;
                if (commandSelected.CommandName == "help")
                {
                    embeds.ThumbnailUrl = "https://tiny.cc/spidermanmeme";
                }
                await ReplyAsync("", false, embeds.Build());
            }
        }
    }
}
