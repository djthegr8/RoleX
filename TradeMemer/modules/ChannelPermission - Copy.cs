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
        [DiscordCommand("ping",commandHelp ="ping", description ="Finds the latency!")]
        public async Task Ping(params string[] argz)
        {
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync($"RoleX enters the Discord Universe in {Context.Client.Latency} miliseconds");
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
                Title = "Prefix Changed!",
                Description = $"The updated prefix is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}`",
                Color = Blurple
            }.WithCurrentTimestamp().Build());
            return;
        }
        [DiscordCommand("help",commandHelp ="help <command>", description ="Shows the Help Message")]
        public async Task HelpC(params string[] args)
        {
            if (args.Length == 0)
            {
                var GCS = new List<string>();
                var GCS1 = new List<string>();
                var GCS2 = new List<string>();
                Commands.FindAll(x => x.ModuleName == "General").ForEach(async x => GCS.Add(await SqliteClass.PrefixGetter(Context.Guild.Id) + x.CommandName));
                Commands.FindAll(x => x.ModuleName == "Role Editor").ForEach(async x => GCS1.Add(await SqliteClass.PrefixGetter(Context.Guild.Id) + x.CommandName));
                Commands.FindAll(x => x.ModuleName == "Channel Permission").ForEach(async x => GCS2.Add(await SqliteClass.PrefixGetter(Context.Guild.Id) + x.CommandName));


                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "RoleX Command Help",
                    Color = Blurple
                }
                .AddField("General Commands","`" + string.Join("`\n`",GCS) + "`",true)
                .AddField("Role Editor Commands", "`" + string.Join("`\n`",GCS1) + "`",true)
                .AddField("Channel Overwrite Commands", "`" + string.Join("\n",GCS2) + "`")
                .AddField("Need more help?", $"For command-wise help, do `{await SqliteClass.PrefixGetter(Context.Guild.Id)}help <commandname>`")
                .WithCurrentTimestamp().Build());
                return;
            }
            else
            {
                var cmd = args[0];
                var prefixure = await SqliteClass.PrefixGetter(Context.Guild.Id);
                var commandSelected = Commands.First(x => (x.CommandName.ToLower() == cmd.ToLower() || x.Alts.Any(x => x.ToLower() == cmd.ToLower())) && x.CommandDescription != "");
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
