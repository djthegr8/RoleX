using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using System.Threading.Tasks;
using Discord.Rest;
using System.Security.Cryptography.X509Certificates;

namespace Public_Bot
{
    public class ReactionService
    {
        private static DiscordSocketClient _client;
        private static Dictionary<ulong, (Func<RestMessage, SocketTextChannel, SocketReaction, Task>, Func<RestMessage, SocketTextChannel, SocketReaction, Task>)> Handlers = new Dictionary<ulong, (Func<RestMessage, SocketTextChannel, SocketReaction, Task>, Func<RestMessage, SocketTextChannel, SocketReaction, Task>)>();
        /// <summary>
        /// Creates the Reaction service
        /// </summary>
        /// <param name="c"></param>
        internal static void Create(DiscordSocketClient c)
        {
            _client = c;

            _client.ReactionAdded += HandleReactionAdded;

            _client.ReactionRemoved += _client_ReactionRemoved;
        }

        private static async Task _client_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg2.GetType() != typeof(SocketTextChannel))
                return;

            var chan = (SocketTextChannel)arg2;
            /*
            if (!GuildCache.CacheContains(chan.Guild.Id))
                _ = GuildCache.GetGuild(chan.Guild.Id);*/

            if (Handlers.ContainsKey(arg1.Id))
            {
                var msg = await chan.GetMessageAsync(arg1.Id);

                if (msg == null)
                    return;

                var task = Handlers[arg1.Id];
                await task.Item2.Invoke((RestMessage)msg, chan, arg3).ConfigureAwait(false);
            }
        }

        private static async Task HandleReactionAdded(Discord.Cacheable<Discord.IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg2.GetType() != typeof(SocketTextChannel))
                return;

            var chan = (SocketTextChannel)arg2;

            /*if (!GuildCache.CacheContains(chan.Guild.Id))
                _ = GuildCache.GetGuild(chan.Guild.Id);*/

            if (Handlers.ContainsKey(arg1.Id))
            {
                var msg = await chan.GetMessageAsync(arg1.Id);

                if (msg == null)
                    return;

                var task = Handlers[arg1.Id];
                await task.Item1.Invoke((RestMessage)msg, chan, arg3).ConfigureAwait(false);
            }
        }

        public static void AddReactionHandler(ulong MessageId, 
            Func<RestMessage, SocketTextChannel, SocketReaction, Task> AddedCallback,
            Func<RestMessage, SocketTextChannel, SocketReaction, Task> RemovedCallback)
        {
            if (Handlers.ContainsKey(MessageId))
                throw new Exception($"{MessageId} already contains a handler!");
            Handlers.Add(MessageId, (AddedCallback, RemovedCallback));
        }
        public static void RemoveReactionHandler(ulong MessageId)
        {
            if(Handlers.ContainsKey(MessageId))
                Handlers.Remove(MessageId);
        }
    }
}
