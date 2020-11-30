using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Public_Bot;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using GuildPermissions = Public_Bot.GuildPermissions;
namespace RoleX.modules
{
    [DiscordCommandClass("General","General Commands for all")]
    class General: CommandModuleBase
    {
        [Alt("hoomans")]
        [DiscordCommand("humans",description ="Shows number of users in server",commandHelp ="humans")]
        public async Task hmans()
        {
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = $"There are {Context.Guild.MemberCount} users in {Context.Guild.Name}!",
                Description = $"Wow nice server guys!",
                Color = Blurple,
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Hehe!"
                }
            }.WithCurrentTimestamp().Build());
            return;
        }
        //[Alt("alt")]
        //[DiscordCommand("altidentify", commandHelp = "altidentify <number-of-alts>", description = "Finds the x users newest to Discord and most probable alts")]
        //public async Task Yu(params string[] argz)
        //{
        //    var test = 10;
        //    if (int.TryParse(argz.FirstOrDefault(), out int retest))
        //    {
        //        test = retest;
        //    }
        //    if (test > Context.Guild.MemberCount)
        //    {
        //        await Context.Channel.SendMessageAsync(embed:new EmbedBuilder
        //        {
        //            Title="This guild does not have the specified amount of users",
        //            Description=$"You asked for {test} youngest users, while your server has only {Context.Guild.MemberCount}",
        //            Color = Blurple
        //        }.WithCurrentTimestamp().Build()
        //        );
        //        return;
        //    }
        //    await ReplyAsync("wait a sec, im getting all the users..");
        //    await Context.Guild.DownloadUsersAsync();
        //    await ReplyAsync("Done i guess");
        //    var yus = Context.Guild.Users;
        //    string cty = "```";
        //    var tenYoungestUsers = yus.ToList();
        //    tenYoungestUsers.RemoveAll(x => x.IsBot);
        //    try
        //    {
        //        tenYoungestUsers.Sort((prev, next) => DateTimeOffset.Compare(prev.CreatedAt, next.CreatedAt));
        //    }
        //    catch
        //    {
        //        tenYoungestUsers.Sort((prev, next) => 0);
        //    }
        //    tenYoungestUsers.Reverse();
        //    var current = tenYoungestUsers.GetRange(0, test);
        //    var pr = current.Max(rx => (rx.Username + "#" + rx.Discriminator + $"({rx.Id})").Length) + '\t';
        //    current.ForEach(x => cty += ((x.Username + "#" + x.Discriminator + $"({x.Id})").PadRight(pr) + $"{x.CreatedAt.Month}/{x.CreatedAt.Day}/{x.CreatedAt.Year}" + '\n'));
        //    cty += "```";
        //    var mmbed = new EmbedBuilder
        //    {
        //        Title = $"Youngest Users in {Context.Guild.Name}",
        //        Description = cty,
        //        Color = Blurple
        //    }.WithCurrentTimestamp().Build();
        //    await Context.Channel.SendMessageAsync("", false, mmbed);
        //}
    
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
        [GuildPermissions(GuildPermission.ManageGuild)]
        [DiscordCommand("setup", commandHelp ="setup", description ="Helps set the bot up!")]
        public async Task Setup(params string[] args)
        {
            string x = "";
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
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Description = "RoleX is a bot that requires various permissions to do various tasks.",
                Fields = {new EmbedFieldBuilder()
                {
                    Name = "Permissions",
                    Value = $"```{x}```"
                } },
                Color = Context.Guild.CurrentUser.GuildPermissions.Administrator ? Color.Green : (x.Count(k => k == '✅') == 7 ? Color.Green : Color.Red),
                Footer = new EmbedFooterBuilder
                {
                    Text = "Command Inspired from LuminousBot (ID: 722435272532426783)"
                }
            }.WithCurrentTimestamp().Build()
            );
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
                .WithFooter("Made by DJ001 (ID: 701029647760097361) and SLENDER (ID: 615873008959225856)")
                .WithCurrentTimestamp();
                foreach( var aa in CustomCommandService.Modules)
                {
                    if (aa.Key == "Developer") continue;
                    helpAuto.AddField(aa.Key, aa.Value);
                }
                helpAuto.AddField("Need more help?", $"Read our Documentation [here](https://rolex.gitbook.io/rolex/ \"Weird Easter Egg\")\n or join [our support server](https://discord.com/invite/3Uq4WF2RFZ \"Probably weirder one\")!\nFor command-wise help, do `{await SqliteClass.PrefixGetter(Context.Guild.Id)}help <commandname/modulename>`");
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
                    var modSelected = CustomCommandService.Modules.Keys.FirstOrDefault(x => x.ToLower().Contains(cmd.ToLower()));
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
                embeds.AddField("Links", "[Support Server](https://discord.com/invite/3Uq4WF2RFZ) | [Invite link](https://tiny.cc/RoleXAdmin)");
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
