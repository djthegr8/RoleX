using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using RoleX.Modules.Services;

namespace RoleX.Modules.Webhooks
{
    [DiscordCommandClass("Webhook Manager", "Helps manage all webhooks!")]
    public class Showwh : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageWebhooks)]
        [DiscordCommand("showwh", commandHelp = "deletewh <#channel>", description = "Deletes the first webhook of given name or ID", example = "deletewh MyWebhook")]
        public async Task DeShowWh(params string[] args)
        {
            var allGWH = (await Context.Guild.GetWebhooksAsync()).ToList();
            if (args.Length == 0)
            {
                if (allGWH.Count == 0)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "No webhook found in your server!",
                        Description = $"If you want to make a Webhook, run `{await SqliteClass.PrefixGetter(Context.Guild.Id)}addwh`",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }

                var emb = new EmbedBuilder
                {
                    Title = "All Guild Webhooks",
                    Description = "*Below is the complete list of webhooks in your server*",
                    Color = Blurple
                }.WithCurrentTimestamp();

                for (int i = 0; i < allGWH.Count; i++)
                {
                    RestWebhook rw = allGWH[i];
                    emb.Fields.Add(new EmbedFieldBuilder {
                        Name = $"{i + 1}) " + rw.Name,
                        Value = $"Channel: <#{rw.ChannelId}>\nCreated By: {rw.Creator.Mention}\nAvatar: [link]({(string.IsNullOrEmpty(rw.GetAvatarUrl()) ? "https://discord.com/assets/6debd47ed13483642cf09e832ed0bc1b.png" : rw.GetAvatarUrl())})" });
                }
                await ReplyAsync("", false, emb);
                return;
            }
            var chn = GetChannel(args[0]);
            if (chn == null)
            {
                var emb = new EmbedBuilder
                {
                    Title = "We couldn't parse the channel!",
                    Description = $"Does `{args[0]}` even exist??",
                    Color = Blurple
                }.WithCurrentTimestamp();
                /*for (int i = 0; i < allGWH.Count; i++)
                {
                    Discord.Rest.RestWebhook rw = allGWH[i];
                    emb.AddField($"{i + 1}) " + rw.Name, $"Channel: <#{rw.ChannelId}>\nCreated By: {rw.Creator.Mention}\nAvatar: [link]({(string.IsNullOrEmpty(rw.GetAvatarUrl()) ? "https://discord.com/assets/6debd47ed13483642cf09e832ed0bc1b.png" : rw.GetAvatarUrl())})");
                }
                await ReplyAsync("", false, emb);
                return;*/
            }
            else
            {
                var idc = (await (chn as SocketTextChannel).GetWebhooksAsync()).ToList();
                if (idc.Count == 0)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "No webhook found in dat channel",
                        Description = $"Really, how do you guys manage?\nIf you wanna make a Webhook, run `{await SqliteClass.PrefixGetter(Context.Guild.Id)}addwh`",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
                /*var paginatedMessage = new PaginatedMessage(PaginatedAppearanceOptions.Default, Context.Channel, new PaginatedMessage.MessagePage("Loading...")) {
                    Title = "All Channel Webhooks",
                    Color = Blurple,
                    Timestamp = DateTimeOffset.Now
                };*/
                var embedFieldBuilders = idc.Select((webhook, i) =>
                    new EmbedFieldBuilder
                    {
                        Name = $"{i + 1}) " + webhook.Name,
                        Value = $"Channel: <#{webhook.ChannelId}>\nCreated By: {webhook.Creator.Mention}\nAvatar: [link]({(string.IsNullOrEmpty(webhook.GetAvatarUrl()) ? "https://discord.com/assets/6debd47ed13483642cf09e832ed0bc1b.png" : webhook.GetAvatarUrl())})"
                    });/*
                paginatedMessage.SetPages($"*Below is the complete list of webhooks the channel <#{chn.Id}>*", embedFieldBuilders, null);
                await paginatedMessage.Resend();*/
                await ReplyAsync("", false, new EmbedBuilder
                    {
                    Title = "All Webhooks in your channel!",
                    Fields = embedFieldBuilders.Take(15).ToList(),
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Only 15 are supported as of now"
                    },
                    Color = Blurple
                }.WithCurrentTimestamp()
                );
            }
        }
    }
}