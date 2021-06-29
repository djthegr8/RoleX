using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hermes.Modules.Services;

namespace Hermes.Modules.Channel_Permission
{
    [DiscordCommandClass("Channel Editor", "Edit Channel-wise perms of a Role using these commands!")]
    public class Channeldelete : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageChannels)]
        [Alt("chdel")]
        [Alt("chdelete")]
        
        [DiscordCommand("channeldelete", description = "Deletes specified channel", example = "channeldelete #general", commandHelp = "channeldelete <#channel>")]
        public async Task Cdel(string ags)
        {
            var aaa = GetChannel(ags);
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

            var ram = await Context.Channel.SendMessageAsync($"Are you sure you want to delete <#{aaa.Id}>?\nThis is a potentially destructive action.");
            await ram.AddReactionsAsync(
                new IEmote[] {
                    Emote.Parse("<a:tick:859032462410907649>"),
                    Emote.Parse("<a:cros:859033035545378826>")
                });
            bool isTick = true;
            Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> weird = null;
            weird =
                async (UserMsg, MsgChannel, Reaction) =>
                {
                    if (MsgChannel == ram.Channel &&
                        UserMsg.Id == ram.Id &&
                        Reaction.UserId == Context.User.Id
                        && (
                            Reaction.Emote.ToString() == "<a:tick:859032462410907649>" ||
                            Reaction.Emote.ToString() == "<a:cros:859033035545378826>"
                        ))
                    {
                        var tick = Emote.Parse("<a:tick:859032462410907649>");
                        isTick = Reaction.Emote.ToString() == tick.ToString();
                        if (!isTick)
                        {
                            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder
                            {
                                Title = "Alright then...",
                                Color = Blurple,
                                ImageUrl = "https://imgur.com/RBC7KUt"
                            }.WithCurrentTimestamp().Build());
                            Program.Client.ReactionAdded -= weird;
                            return;
                        }

                        isTick = false;
                        await aaa.DeleteAsync();
                        await Context.Channel.SendMessageAsync(embed: new EmbedBuilder
                        {
                            Title = "Deleted Channel Successfully",
                            Description = $"Channel `#{aaa.Name}` was deleted!",
                            Color = Blurple
                        }.WithCurrentTimestamp().Build());
                        Program.Client.ReactionAdded -= weird;
                    }
                };
            Program.Client.ReactionAdded += weird;
            await Task.Delay(15000);
            if (isTick) await Context.Channel.SendMessageAsync("No response received!");
            Program.Client.ReactionAdded -= weird;
        }
    }
}
