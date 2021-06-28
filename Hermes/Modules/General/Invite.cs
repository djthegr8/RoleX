using System.Threading.Tasks;
using Discord;
using Hermes.Modules.Services;

namespace Hermes.Modules.General
{
    [DiscordCommandClass("General", "General commands for all!")]
    public class R_Invite : CommandModuleBase
    {
        [DiscordCommand("invite",description ="Invite Hermes to your server!!", commandHelp ="invite")]
        public async Task Invite(params string[] _)
        {
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Invite Hermes to your server!",
                Description = "[**Invite using recommended permission link**](https://discord.com/oauth2/authorize?client_id=744766526225252435&scope=bot&permissions=805334064)\n[**Invite using Admin link for easy setup**](https://discord.com/oauth2/authorize?client_id=744766526225252435&scope=bot&permissions=8)",
                Footer = new EmbedFooterBuilder
                {
                    Text="Thank you for choosing Hermes"
                },

                Color = Blurple
            }.WithCurrentTimestamp());
        }
    }
}