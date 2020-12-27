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
    public class Getinvite : CommandModuleBase
    {
        [DiscordCommand("getinvite", commandHelp = "", description = "")]
        public async Task getInv(params string[] args)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                if (args.Length == 0 || !ulong.TryParse(args[0], out ulong _)) { await ReplyAsync("Why are you like this <:noob:756055614861344849>"); return; }
                ulong x = ulong.Parse(args[0]);
                try
                {
                    var aaa = (await Context.Client.GetGuild(x).GetInvitesAsync()).First().Url;
                    await Context.User.SendMessageAsync(aaa);
                }
                catch
                {
                    await ReplyAsync($"I don't fricking have perms, but hey u can DM the owner. He is <@{Context.Client.GetGuild(x).OwnerId}>");
                }
            }
        }
    }
}