﻿using Discord;
using Public_Bot;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace RoleX.modules
{
    [DiscordCommandClass("Developer", "Dev Commands that you cant use")]
    class DeveloperCommands : CommandModuleBase
    {
        [DiscordCommand("guilds", commandHelp = "", description = "")]
        public async Task Guilds(params string[] args)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                string st = "```";
                foreach (var srver in Context.Client.Guilds)
                {

                    /*string inv;
                    try
                    {
                        inv = (await srver.GetInvitesAsync()).First().Url;
                    }
                    catch { inv = "No Perms LMAO!"; }*/
                    /*st += $"{srver.Name}\t{inv}\n";*/
                    st += $"{srver.Name}({srver.Id})\t{srver.MemberCount}\n";
                }
                st += "```";
                await ReplyAsync(
                    embed: new EmbedBuilder
                    {
                        Title = "All RoleX Guilds LMAO",
                        Description = st,
                        Color = Blurple
                    }.WithCurrentTimestamp().Build());
            }
        }
        [DiscordCommand("sqlite", commandHelp = "", description = "")]
        public async Task Gz(params string[] args)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                await Context.User.SendFileAsync($"..{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}rolex.db");
                await ReplyAsync("Check ur DM!");
            }
        }
        [DiscordCommand("getinvite", commandHelp = "", description = "")]
        public async Task getInv(params string[] args)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                try
                {
                    if (args.Length == 0 || !ulong.TryParse(args[0], out ulong _)) { await ReplyAsync("Why are you like this <:noob:756055614861344849>"); return; }
                    ulong x = ulong.Parse(args[0]);
                    var aaa = (await Context.Client.GetGuild(x).GetInvitesAsync()).First().Url;
                    await Context.User.SendMessageAsync(aaa);
                }
                catch
                {
                    await ReplyAsync("no");
                }
            }
        }
        /*[DiscordCommand("gencommandtotalus", commandHelp = "", description = "")]
        public async Task getInves(params string[] args)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                
                foreach (var x in CustomCommandService.Modules)
                {
                    string xyz = "# RoleX\n";
                    xyz += $"## {x.Key}\n*{x.Value}*\n";
                    foreach (var y in Commands.Where(y => y.ModuleName == x.Key))
                    {
                        xyz += $"### r{y.CommandName}         \nDescription: {(string.IsNullOrEmpty(y.CommandDescription) ? "None" : y.CommandDescription)}          \nAlts: `{(string.IsNullOrEmpty(string.Join(", ",y.Alts)) ? "None" : string.Join(", ", y.Alts))}`          \nExamples: `{(string.IsNullOrEmpty(y.example) ? "None" : y.example)}`          \n";
                    }
                    FileStream fst = new FileStream($"../Data/{x.Key}.md", FileMode.Create);
                    StreamWriter swr = new StreamWriter(fst);
                    await swr.WriteLineAsync(xyz);
                    await swr.FlushAsync();
                    swr.Close();
                }
                
            }
        }*/
        [DiscordCommand("stupidServer", commandHelp = "", description = "")]
        public async Task weird(params string[] args)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                if (args.Length == 0 || !ulong.TryParse(args[0], out ulong _)) { await ReplyAsync("Why are you like this <:noob:756055614861344849>"); return; }
                ulong x = ulong.Parse(args[0]);
                try
                {
                    await Context.Client.GetGuild(x).SystemChannel.SendMessageAsync("Leaving this server <:catthumbsup:780419880385380352>");
                }
                catch { }
                await Context.Client.GetGuild(x).LeaveAsync();
            }
        }
        [DiscordCommand("pushToAllServers", commandHelp = "", description = "")]
        public async Task Push(params string[] args)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                string x = string.Join(' ', args);
                string[] splitted = x.Split(',');
                string title = splitted[0];
                string description = string.Join(' ',splitted.Skip(1));
                foreach(var server in Context.Client.Guilds)
                {
                    var channel = server.DefaultChannel;
                    try
                    {
                        await channel.SendMessageAsync("", false, new EmbedBuilder
                        {
                            Title = title,
                            Description = description,
                            Color = Blurple,
                            ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
                        }.WithCurrentTimestamp().Build());
                    } catch
                    {
                        await ReplyAsync($"Couldn't write in {server.Name}");
                    } finally { }
                }
            }
        }
    }
}