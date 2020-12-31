using Discord;
using Discord.WebSocket;
using Public_Bot;
using System;
using System.Threading.Tasks;
namespace RoleX.Modules
{
    [DiscordCommandClass("Channel Editor", "Edit Channel-wise perms of a Role using these commands!")]
    public class Categorydelete : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.Administrator)]
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
            var ram = await Context.Channel.SendMessageAsync("Are you sure you want to delete?\nThis is a potentially destructive action.");
            await ram.AddReactionsAsync(
                new IEmote[] {
                        Emote.Parse("<a:tick:792389924312973333>"),
                        Emote.Parse("<a:cros:792389968890429461>")
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
                        Reaction.Emote.ToString() == "<a:tick:792389924312973333>" ||
                        Reaction.Emote.ToString() == "<a:cros:792389968890429461>"
                        ))
                    {
                        var tick = Emote.Parse("<a:tick:792389924312973333>");
                        isTick = Reaction.Emote.ToString() == tick.ToString();
                        if (!isTick)
                        {
                            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder
                            {
                                Title = "Alright then...",
                                Color = Blurple,
                                ImageUrl = "https://media.discordapp.net/attachments/758922634749542420/792611702885449748/unknown.png"
                            }.WithCurrentTimestamp().Build());
                            Program.Client.ReactionAdded -= weird;
                            return;
                        }
                        else
                        {
                            isTick = false;
                            foreach (var ch in alf.Channels)
                            {
                                await ch.DeleteAsync();
                            }
                            await alf.DeleteAsync();
                            await ReplyAsync("", false, new EmbedBuilder
                            {
                                Title = "Delete successful!",
                                Description = $"Your category was deleted along with all its channels",
                                Color = Blurple
                            }.WithCurrentTimestamp());
                            Program.Client.ReactionAdded -= weird;
                            return;
                        }
                    }
                };
            Program.Client.ReactionAdded += weird;
            await Task.Delay(15000);
            if (isTick) await Context.Channel.SendMessageAsync("Well, you didn't reply :(");
            Program.Client.ReactionAdded -= weird;
            return;
        }
    }
}