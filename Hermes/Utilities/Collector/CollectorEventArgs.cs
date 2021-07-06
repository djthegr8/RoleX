using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Hermes.Utilities.Collector
{
    public abstract class CollectorEventArgsBase : EventArgs
    {
        protected CollectorEventArgsBase(CollectorController controller)
        {
            Controller = controller;
            Controller.TaskCompletionSource?.SetResult(this);
        }

        public CollectorController Controller { get; set; }

        public void StopCollect()
        {
            Controller.Dispose();
        }

        public abstract Task RemoveReason();
    }

    public class EmoteCollectorEventArgs : CollectorEventArgsBase
    {
        public EmoteCollectorEventArgs(CollectorController controller, SocketReaction reaction) : base(controller)
        {
            Reaction = reaction;
        }

        public SocketReaction Reaction { get; set; }

        public override async Task RemoveReason()
        {
            try
            {
                var message = (IUserMessage) (Reaction.Message.IsSpecified
                    ? Reaction.Message.Value
                    : await Reaction.Channel.GetMessageAsync(Reaction.MessageId));
                await message.RemoveReactionAsync(Reaction.Emote, Reaction.User.Value);
            }
            catch (Exception)
            {
                Controller.OnRemoveArgsFailed(this);
            }
        }
    }

    public class EmoteMultiCollectorEventArgs : EmoteCollectorEventArgs
    {
        public EmoteMultiCollectorEventArgs(CollectorController controller, CollectorsGroup group,
            SocketReaction reaction) : base(controller, reaction)
        {
            CollectorsGroup = group;
        }

        public CollectorsGroup CollectorsGroup { get; set; }
    }

    public class MessageCollectorEventArgs : CollectorEventArgsBase
    {
        public MessageCollectorEventArgs(CollectorController controller, IMessage message) : base(controller)
        {
            Message = message;
        }

        public IMessage Message { get; set; }

        public override async Task RemoveReason()
        {
            try
            {
                await Message.DeleteAsync();
            }
            catch
            {
                Controller.OnRemoveArgsFailed(this);
            }
        }
    }

    public class CommandCollectorEventArgs : CollectorEventArgsBase
    {
        public CommandCollectorEventArgs(CollectorController controller, IMessage message,
            KeyValuePair<CommandMatch, ParseResult> info,
            ICommandContext context) : base(controller)
        {
            Message = message;
            CommandInfo = info.Key;
            Context = context;
            ParseResult = info.Value;
        }

        public bool Handled { get; set; }

        public IMessage Message { get; }
        public CommandMatch CommandInfo { get; }
        public ParseResult ParseResult { get; }
        public ICommandContext Context { get; }

        public override Task RemoveReason()
        {
            Message.SafeDelete();
            return Task.CompletedTask;
        }

        // public async Task<IResult> ExecuteCommand(ICommandContext? overrideContext = null) {
        //     return await CommandInfo.ExecuteAsync(overrideContext ?? Context, ParseResult, EmptyServiceProvider.Instance).ConfigureAwait(false);
        // }
    }
}