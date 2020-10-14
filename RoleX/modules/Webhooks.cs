﻿using Discord;
using System.Net;
using Public_Bot;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.IO;
using GuildPermissions = Public_Bot.GuildPermissions;
using RoleX.modules;

namespace RoleX.modules
{
    [DiscordCommandClass("Webhook Manager","Helps Manage all Webhooks!")]
    class Webhooks : CommandModuleBase
    {
        [GuildPermissions(GuildPermission.ManageWebhooks)]
        [DiscordCommand("showwh", commandHelp = "deletewh <#channel>", description = "Deletes the first webhook of given name or ID", example = "deletewh MyWebhook")]
        public async Task DeShowWh(params string[] args)
        {
            var allGWH = (await Context.Guild.GetWebhooksAsync()).ToList();
            if (args.Length == 0)
            {
                if (allGWH.Count == 0 ) {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "No webhook found in yer server!",
                        Description = $"Really, how do you guys manage?\nIf you wanna make a Webhook, run `{await SqliteClass.PrefixGetter(Context.Guild.Id)}addwh`",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
                }
                else 
                {
                    var emb = new EmbedBuilder
                    {
                        Title = "All Guild Webhooks",
                        Description = $"*Below is the complete list of webhooks in your server*",
                        Color = Blurple
                    }.WithCurrentTimestamp();

                    for (int i = 0; i < allGWH.Count; i++)
                    {
                        Discord.Rest.RestWebhook rw = allGWH[i];
                        emb.AddField($"{i + 1}) " + rw.Name, $"Channel: <#{rw.ChannelId}>\nCreated By: {rw.Creator.Mention}\nAvatar: [link]({(string.IsNullOrEmpty(rw.GetAvatarUrl()) ? "https://discord.com/assets/6debd47ed13483642cf09e832ed0bc1b.png" : rw.GetAvatarUrl())})");
                    }
                    await ReplyAsync("", false, emb.Build());
                    return;
                }
            }
            var chn = GetChannel(args[0]);
            if (chn == null)
            {
                var emb = new EmbedBuilder
                {
                    Title = $"We couldn't parse `{args[0]}` as a channel so...",
                    Description = $"*Below is the complete list of webhooks in your server*",
                    Color = Blurple
                }.WithCurrentTimestamp();
                for (int i = 0; i < allGWH.Count; i++)
                {
                    Discord.Rest.RestWebhook rw = allGWH[i];
                    emb.AddField($"{i + 1}) " + rw.Name, $"Channel: <#{rw.ChannelId}>\nCreated By: {rw.Creator.Mention}\nAvatar: [link]({(string.IsNullOrEmpty(rw.GetAvatarUrl()) ? "https://discord.com/assets/6debd47ed13483642cf09e832ed0bc1b.png" : rw.GetAvatarUrl())})");
                }
                await ReplyAsync("", false, emb.Build());
                return;
            } else
            {
                var emb = new EmbedBuilder
                {
                    Title = "All Channel Webhooks",
                    Description = $"*Below is the complete list of webhooks the channel <#{chn.Id}>*",
                    Color = Blurple
                }.WithCurrentTimestamp();
                var idc = (await (chn as SocketTextChannel).GetWebhooksAsync()).ToList();
                if (idc.Count == 0)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "No webhook found in dat channel",
                        Description = $"Really, how do you guys manage?\nIf you wanna make a Webhook, run `{await SqliteClass.PrefixGetter(Context.Guild.Id)}addwh`",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
                }
                for(int i = 0; i < idc.Count; i++)
                {
                    Discord.Rest.RestWebhook rw = idc[i];
                    emb.AddField($"{i + 1}) " + rw.Name, $"Channel: <#{rw.ChannelId}>\nCreated By: {rw.Creator.Mention}\nAvatar: [link]({(string.IsNullOrEmpty(rw.GetAvatarUrl()) ? "https://discord.com/assets/6debd47ed13483642cf09e832ed0bc1b.png" : rw.GetAvatarUrl())})");
                }
                await ReplyAsync("", false, emb.Build());
                return;
            }
        }
        [GuildPermissions(GuildPermission.ManageWebhooks)]
        [DiscordCommand("deletewh", commandHelp ="deletewh <webhook-name/id>", description ="Deletes the first webhook of given name or ID", example ="deletewh MyWebhook")]
        public async Task DeleteWh(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "No webhook name/id given :(",
                    Description = $"There were no arguments given :sob:",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            var allGWH = await Context.Guild.GetWebhooksAsync();
            var iGTSW = allGWH.ToList().FindAll(x => x.Name.ToLower().Contains(args[0].ToLower()));
            if (iGTSW == null || iGTSW?.Count == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "No webhook found with that name!",
                    Description = $"There were no webhooks found with the word `{args[0]}`",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }
            else if (iGTSW.Count == 1)
            {
                var reqwh = iGTSW[0];
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Webhook deleted successfully!",
                    Description = $"The webhook `{reqwh.Name}` of channel <#{reqwh.ChannelId}> was deleted successfully!!",
                    Color = Blurple
                }.WithCurrentTimestamp().Build());
                await reqwh.DeleteAsync();
                return;
            } else
            {
                var emb = new EmbedBuilder
                {
                    Title = "Multiple Webhooks found!",
                    Description = $"We found multiple webhooks by your query `{args[0]}`",
                    Color = Color.Red
                }.WithCurrentTimestamp();
                for (int i = 0; i < iGTSW.Count; i++)
                {
                    Discord.Rest.RestWebhook rw = iGTSW[i];
                    emb.AddField($"{i + 1}) " + rw.Name, $"Channel: <#{rw.ChannelId}>\nCreated By: {rw.Creator.Username}#{rw.Creator.Discriminator}\nAvatar: [link]({(string.IsNullOrEmpty(rw.GetAvatarUrl()) ? "https://discord.com/assets/6debd47ed13483642cf09e832ed0bc1b.png" : rw.GetAvatarUrl())})");
                }
                await ReplyAsync("", false, emb.Build());
                return;
            }
        }
        [GuildPermissions(GuildPermission.ManageWebhooks)]
        [Alt("createwh")]
        [DiscordCommand("addwh",commandHelp ="addwh <#channel> <Webhook-Name> <WebhookAvatarUrl>",description ="Creates a new webhook in given channel of given name, avatar & DMs the Webhook URL", example ="addwh #memes MemeWebhook https://tiny.cc/joketoyou")]
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
                    }.WithCurrentTimestamp().Build());
                    return;
                } 
            }
            var weh = await achan.CreateWebhookAsync(args.Length <= 1 ? $"RoleX Created Webhook (Requester ID:{Context.User.Id})" : args[1]);
            if (args.Length > 2)
            {
                try
                {
                    pfp = new MemoryStream(new WebClient().DownloadData(args[2]));
                    await weh.ModifyAsync(x => x.Image = new Image(pfp));
                } catch { }
            }
            await (await Context.User.GetOrCreateDMChannelAsync()).SendMessageAsync("", false, new EmbedBuilder
            {
               Title=$"New Webhook Creation Successful",
               Description = $"**Name:** {weh.Name}\n**Channel:** <#{weh.ChannelId}>\n**Link:** [Click here or on the title](https://discordapp.com/api/webhooks/{weh.Id}/{weh.Token})",
               Url=$"https://discordapp.com/api/webhooks/{weh.Id}/{weh.Token}",
               ImageUrl = weh.GetAvatarUrl(),
               Color = Blurple
            }.WithCurrentTimestamp()
            .Build()
            );
            await ReplyAsync(Context.User.Mention, false, new EmbedBuilder
            {
                Title = "Created Webhook Successfully!",
                Description = $"I have DMed you with the Url!",
                Color = Blurple
            }.WithCurrentTimestamp().Build());
            return;
        }
        
    }
}
