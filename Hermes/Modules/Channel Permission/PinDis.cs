using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hermes.Modules.Services;

namespace Hermes.Modules.Channel_Permission
{
    [DiscordCommandClass("Channel Editor", "Edit Channel-wise perms of a Role using these commands!")]
    public class PinDis : CommandModuleBase
    {
        [Alt("pindis")]
        [Alt("pd")]
        [Alt("unpindis")]
        [Alt("upd")]
        [DiscordCommand("pinthis", description = "Pins / Unpins the given message that has been replied to",
            commandHelp = "pinthis")]
        public async Task PC(params string[] args)
        {
            if (Context.Message.Reference == null || Context.Message.Reference.ChannelId != Context.Channel.Id)
            {
                await ReplyAsync("", false, new EmbedBuilder()
                {
                    Title = "No message referenced!",
                    Description = "You gotta reply to a message to pin/unpin it."
                });
                return;
            }

            if (Context.Message.ReferencedMessage.IsPinned) await Context.Message.ReferencedMessage.UnpinAsync();
            else await Context.Message.ReferencedMessage.PinAsync();
            await Context.Message.AddReactionAsync(new Emoji("👌"));
        }
    }
}