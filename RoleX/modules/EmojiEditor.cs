using Discord;
using Public_Bot;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;

namespace RoleX.modules
{
    /*[DiscordCommandClass("Emote Editor", "For complete management of server emotes!")]
    class EmojiEditor : CommandModuleBase
    {
        [DiscordCommand("steal", commandHelp = "steal new_emote_name <:emote:>`\n`steal new_emote_name {attached image}", description = "")]
        public async Task steal(params string[] args)
        {
            //Lets go boiz
            if (args.Length == 0)
            {
                if (Context.Message.Attachments.Any())
                {
                    var at1 = Context.Message.Attachments.First();
                    if (at1.Url.EndsWith("png") || at1.Url.EndsWith("jpg") || at1.Url.EndsWith("jpeg") || at1.Url.EndsWith("gif"))
                    {
                        WebClient wec = new();
                        byte[] bytes = wec.DownloadData(at1.Url);
                        MemoryStream ms = new MemoryStream(bytes);
                        System.Drawing.Image drawer = System.Drawing.Image.FromStream(ms);
                        drawer.s
                    }
                    else
                    {

                    }
                } else
                {

                }
            }
        }
    */
}

