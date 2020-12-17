using Discord;
using Public_Bot;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoleX.modules
{
    [DiscordCommandClass("Developer", "Dev Commands that you cant use")]
    class DeveloperCommands : CommandModuleBase
    {
        [DiscordCommand("sqlitenonquery", commandHelp = "", description = "", example = "")]
        public async Task SQNQ(params string[] args)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                var f = string.Join(' ', args);
                await SqliteClass.NonQueryFunctionCreator(f);
            }
        }
        [DiscordCommand("guilds", commandHelp = "", description = "")]
        public async Task Guilds(params string[] _)
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
                        Title = $"All RoleX Guilds LMAO (total: {Context.Client.Guilds.Count})",
                        Description = st,
                        Color = Blurple
                    }.WithCurrentTimestamp());
            }
        }
        [DiscordCommand("sqlite", commandHelp = "", description = "")]
        public async Task Gz(params string[] _)
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
                string description = string.Join(' ', splitted.Skip(1));
                foreach (var server in Context.Client.Guilds)
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
                    }
                    catch
                    {
                        await ReplyAsync($"Couldn't write in {server.Name}");
                    }
                    finally { }
                }
            }
        }
        [DiscordCommand("adr", commandHelp = "", description = "")]
        public async Task GPe(ulong a, ulong y)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                var breh = Context.Client.Guilds.First(al => al.Id == a);
                if (breh == null)
                {
                    await ReplyAsync("Why are you like this <:noob:756055614861344849>");
                    return;
                }
                var irdk = breh.GetUser(Context.User.Id);
                if (irdk == null)
                {
                    await ReplyAsync("Why are you like this <:noob:756055614861344849>");
                    return;
                }
                await irdk.AddRoleAsync(breh.GetRole(y));
            }
        }
    }
}
