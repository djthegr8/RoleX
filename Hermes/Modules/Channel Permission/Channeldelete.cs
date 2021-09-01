using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Hermes.Modules.Services;

namespace Hermes.Modules.Channel_Permission
{
    [DiscordCommandClass("Channel Editor", "Edit Channel-wise perms of a Role using these commands!")]
    public class Channeldelete : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageChannels)]
        [Alt("chdel")]
        [Alt("chdelete")]
        [DiscordCommand("channeldelete", description = "Deletes specified channel", example = "channeldelete #general",
            commandHelp = "channeldelete <#channel>")]
        public async Task Cdel(params string[] ags)
        {
            if (ags.Length == 0)
                ags = new[]
                {
                    Context.Channel.Id.ToString()
                };
            var aaa = GetChannel(ags[0]);
            if (aaa == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid channel",
                    Description = $"`{ags}` could not be parsed as a channel!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            var cros = Emote.Parse("<a:cros:859033035545378826>");
            var tickk = Emote.Parse("<a:tick:859032462410907649>");
            var gc = Guid.NewGuid();
            var cb = new ComponentBuilder().WithButton("", $"{gc}Tick", ButtonStyle.Secondary, tickk)
                .WithButton("", $"{gc}Cros", ButtonStyle.Secondary, cros);
            var ram = await Context.Channel.SendMessageAsync(
                $"Are you sure you want to delete <#{aaa.Id}>?\nThis is a potentially destructive action.",
                component: cb.Build());
            var cancelSource = new CancellationTokenSource();
            cancelSource.CancelAfter(15000);
            var Interaction = await InteractionHandler.NextButtonAsync(
                k => k.Data.CustomId.Contains(gc.ToString()) && k.User.Id == Context.User.Id, cancelSource.Token);
            if (Interaction == null)
            {
                await Context.Channel.SendMessageAsync("No response received!");
            }
            else
            {
                var isTick = Interaction.Data.CustomId.Contains("Tick");
                await Interaction.AcknowledgeAsync();
                if (!isTick)
                {
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder
                    {
                        Title = "Alright then...",
                        Color = Blurple,
                        ImageUrl = "https://i.imgur.com/RBC7KUt.png"
                    }.WithCurrentTimestamp().Build());
                    return;
                }

                await aaa.DeleteAsync();
                try
                {
                    await Context.Channel.SendMessageAsync(embed: new EmbedBuilder
                    {
                        Title = "Deleted Channel Successfully",
                        Description = $"Channel `#{aaa.Name}` was deleted!",
                        Color = Blurple
                    }.WithCurrentTimestamp().Build());
                }
                catch
                {
                    Console.WriteLine("Ch del'd");
                }
            }
        }
    }
}
