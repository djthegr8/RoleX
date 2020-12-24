using Discord;
using Public_Bot;
using System.IO;
using System;
using System.Linq;
using MoreLinq;
using System.Threading.Tasks;
namespace RoleX.Modules
{
    [DiscordCommandClass("Developer", "Dev commands that you can't use 🤣!")]
    public class StupidServer : CommandModuleBase
    {
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
    }
}