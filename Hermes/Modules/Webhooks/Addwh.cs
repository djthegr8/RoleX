using System.IO;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hermes.Modules.Services;

namespace Hermes.Modules.Webhooks
{
    [DiscordCommandClass("Webhook Manager", "Helps manage all webhooks!")]
    public class Addwh : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageWebhooks)]
        [DiscordCommand("addwh", commandHelp = "addwh <#channel> <Webhook-Name> <WebhookAvatarUrl>", description = "Creates a new webhook in given channel of given name, avatar & DMs the Webhook URL", example = "addwh #memes MemeWebhook https://tiny.cc/joketoyou")]
        public async Task AddWH(params string[] args)
        {
            SocketTextChannel achan;
            MemoryStream pfp;
            if (args.Length == 0) achan = Context.Channel as SocketTextChannel;
            else
            {
                achan = GetChannel(args[0]) as SocketTextChannel;
                if (achan == null)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Invalid channel name",
                        Description = $"`{args[0]}` could not be parsed as channel!",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
            }
            var weh = await achan.CreateWebhookAsync(args.Length <= 1 ? $"Hermes Created Webhook (Requester ID:{Context.User.Id})" : args[1]);
            if (args.Length > 2)
            {
                try
                {
                    pfp = new MemoryStream(new WebClient().DownloadData(args[2]));
                    await weh.ModifyAsync(x => x.Image = new Image(pfp));
                }
                catch { }
            }
            await (await Context.User.CreateDMChannelAsync()).SendMessageAsync("", false, new EmbedBuilder
            {
                Title = "New Webhook Creation Successful",
                Description = $"**Name:** {weh.Name}\n**Channel:** <#{weh.ChannelId}>\n**Link:** [Click here or on the title](https://discordapp.com/api/webhooks/{weh.Id}/{weh.Token})",
                Url = $"https://discordapp.com/api/webhooks/{weh.Id}/{weh.Token}",
                ImageUrl = weh.GetAvatarUrl(),
                Color = Blurple
            }.WithCurrentTimestamp().Build()
            );
            await ReplyAsync(Context.User.Mention, false, new EmbedBuilder
            {
                Title = "Created Webhook Successfully!",
                Description = "I have DMed you with the URL!",
                Color = Blurple
            }.WithCurrentTimestamp());
        }
    }
}