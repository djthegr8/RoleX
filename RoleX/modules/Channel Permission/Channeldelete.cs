using Discord;
using Discord.WebSocket;
using Public_Bot;
using System;
using System.Threading.Tasks;
namespace RoleX.Modules
{
    [DiscordCommandClass("Channel Editor", "Edit Channel-wise perms of a Role using these commands!")]
    public class Channeldelete : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageChannels)]
        [Alt("chdel")]
        [DiscordCommand("channeldelete", description = "Deletes given channel", example = "channeldelete #WeirdChan", commandHelp = "channeldelete <#channel>")]
        public async Task Cdel(string ags)
        {
            var aaa = GetChannel(ags);
            if (aaa == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid channel",
                    Description = $"`{ags}` could not be parsed as channel!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            else
            {
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
                          } else
                          {
                              isTick = false;
                              await aaa.DeleteAsync();
                              await Context.Channel.SendMessageAsync(embed: new EmbedBuilder
                              {
                                  Title = "Deleted Channel Successfully",
                                  Description = $"Channel `#{aaa.Name}` was deleted!",
                                  Color = Blurple
                              }.WithCurrentTimestamp().Build());
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
}