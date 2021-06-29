using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hermes.Modules.Services;

namespace Hermes.Modules.Channel_Permission
{
    [DiscordCommandClass("Channel Editor", "Edit Channel-wise perms of a Role using these commands!")]
    public class Categorydelete : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageChannels)]
        [Alt("catdel")]
        [DiscordCommand("categorydelete", commandHelp = "categorydelete <category-name>", description = "Deletes given category and all its channels", example = "categorydelete Useless")]
        public async Task CatDel(string aa)
        {
            var alf = GetCategory(aa);
            if (alf == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid category",
                    Description = $"`{aa}` could not be parsed as category!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var ram = await Context.Channel.SendMessageAsync($"Are you sure you want to delete <#{alf.Id}>?\nThis is a potentially destructive action.");
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
                                ImageUrl = "https://i.imgur.com/RBC7KUt.png"
                            }.WithCurrentTimestamp().Build());
                            Program.Client.ReactionAdded -= weird;
                            return;
                        }

                        isTick = false;
                        foreach (var ch in alf.Channels)
                        {
                            await ch.DeleteAsync();
                        }
                        await alf.DeleteAsync();
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Title = "Action successful!",
                            Description = $"<#{alf.Id}> was deleted along with all its channels",
                            Color = Blurple
                        }.WithCurrentTimestamp());
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