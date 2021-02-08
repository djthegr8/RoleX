using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

#pragma warning disable 1998

namespace RoleX.Utilities.Collector {
    public static class CollectorsUtils {
        private static Subject<(Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction)> ReactionAdded =
            new Subject<(Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction)>();

        private static Subject<SocketMessage> MessageReceived = new Subject<SocketMessage>();

        static CollectorsUtils() {
            Program.Client.ReactionAdded += (cacheable, channel, arg3) => {
                ReactionAdded.OnNext((cacheable, channel, arg3));
                return Task.CompletedTask;
            };
            Program.Client.MessageReceived += message => {
                MessageReceived.OnNext(message);
                return Task.CompletedTask;
            };
        }

        public static CollectorController CollectReaction(Predicate<SocketReaction> predicate,
                                                          Action<EmoteCollectorEventArgs> action, CollectorFilter filter = CollectorFilter.Off) {
            var collectorController = new CollectorController();

            predicate = ApplyFilters(predicate, filter);
            var disposable = ReactionAdded.Subscribe(tuple => {
                try {
                    if (predicate(tuple.Item3)) action(new EmoteCollectorEventArgs(collectorController, tuple.Item3));
                }
                catch (Exception) {
                    // ignored
                }
            });
            collectorController.Stop += (sender, args) => disposable.Dispose();

            return collectorController;
        }

        public static CollectorController CollectMessage(Predicate<IMessage> predicate,
                                                         Action<MessageCollectorEventArgs> action, CollectorFilter filter = CollectorFilter.Off) {
            var collectorController = new CollectorController();

            predicate = ApplyFilters(predicate, filter);
            var disposable = MessageReceived.Subscribe(message => {
                try {
                    if (predicate(message)) action(new MessageCollectorEventArgs(collectorController, message));
                }
                catch (Exception) {
                    // ignored
                }
            });
            collectorController.Stop += (sender, args) => disposable.Dispose();

            return collectorController;
        }

        public static CollectorController CollectReaction(IChannel channel, Predicate<SocketReaction> predicate,
                                                          Action<EmoteCollectorEventArgs> action, CollectorFilter filter = CollectorFilter.Off)
            => CollectReaction(reaction => channel.Id == reaction.Channel.Id && predicate(reaction), action, filter);

        public static CollectorController CollectReaction(IEmote emote, Predicate<SocketReaction> predicate,
                                                          Action<EmoteCollectorEventArgs> action, CollectorFilter filter = CollectorFilter.Off)
            => CollectReaction(reaction => emote.Equals(reaction.Emote) && predicate(reaction), action, filter);

        public static CollectorController CollectReaction(IMessage message, Predicate<SocketReaction> predicate,
                                                          Action<EmoteCollectorEventArgs> action, CollectorFilter filter = CollectorFilter.Off)
            => CollectReaction(reaction => message.Id == reaction.MessageId && predicate(reaction), action, filter);

        public static CollectorController CollectReaction(IUser user, Predicate<SocketReaction> predicate,
                                                          Action<EmoteCollectorEventArgs> action, CollectorFilter filter = CollectorFilter.Off)
            => CollectReaction(reaction => user.Id == reaction.UserId && predicate(reaction), action, filter);

        public static CollectorController CollectMessage(IUser user, Predicate<IMessage> predicate,
                                                         Action<MessageCollectorEventArgs> action, CollectorFilter filter = CollectorFilter.Off)
            => CollectMessage(message => user.Id == message.Author.Id && predicate(message), action, filter);

        public static CollectorController CollectMessage(IChannel channel, Predicate<IMessage> predicate,
                                                         Action<MessageCollectorEventArgs> action, CollectorFilter filter = CollectorFilter.Off)
            => CollectMessage(message => channel.Id == message.Channel.Id && predicate(message), action, filter);

        public static CollectorsGroup CollectReactions<T>(Predicate<SocketReaction> predicate, Action<EmoteMultiCollectorEventArgs, T> action,
                                                          params (IEmote, T)[] selectors) {
            return CollectReactions(predicate, action, selectors.Select(tuple => (tuple.Item1, new Func<T>(() => tuple.Item2))).ToArray());
        }

        public static CollectorsGroup CollectReactions<T>(Predicate<SocketReaction> predicate, Action<EmoteMultiCollectorEventArgs, T> action,
                                                          params (IEmote, Func<T>)[] selectors) {
            var collectorsGroup = new CollectorsGroup();
            foreach (var selector in selectors.ToList()) {
                var collectorController = new CollectorController();

                var localPredicate = new Predicate<SocketReaction>(reaction => reaction.Emote.Equals(selector.Item1) && predicate(reaction));
                var disposable = ReactionAdded.Subscribe(tuple => {
                    try {
                        if (localPredicate(tuple.Item3))
                            action(new EmoteMultiCollectorEventArgs(collectorController, collectorsGroup, tuple.Item3), selector.Item2());
                    }
                    catch (Exception e) {
                        Console.WriteLine(e);
                        throw;
                    }
                });
                collectorController.Stop += (sender, args) => disposable.Dispose();

                // ReSharper disable once PossiblyMistakenUseOfParamsMethod
                collectorsGroup.Add(collectorController);
            }

            return collectorsGroup;
        }

        private static Predicate<IMessage> ApplyFilters(Predicate<IMessage> initial, CollectorFilter filter) {
            return filter switch {
                CollectorFilter.Off        => initial,
                CollectorFilter.IgnoreSelf => (message => message.Author.Id != Program.Client.CurrentUser.Id && initial(message)),
                CollectorFilter.IgnoreBots => (message => !message.Author.IsBot && !message.Author.IsWebhook && initial(message)),
                _                          => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
            };
        }

        private static Predicate<SocketReaction> ApplyFilters(Predicate<SocketReaction> initial, CollectorFilter filter) {
            return filter switch {
                CollectorFilter.Off        => initial,
                CollectorFilter.IgnoreSelf => (reaction => reaction.UserId != Program.Client.CurrentUser.Id && initial(reaction)),
                CollectorFilter.IgnoreBots => reaction => {
                    try {
                        var user = reaction.User.GetValueOrDefault(Program.Client.GetUser(reaction.UserId));
                        return !user.IsBot && !user.IsWebhook && initial(reaction);
                    }
                    catch (Exception) {
                        return false;
                    }
                },
                _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
            };
        }
    }
}