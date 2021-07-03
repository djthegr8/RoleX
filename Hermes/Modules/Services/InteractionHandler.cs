using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hermes.Modules.Services
{
    public static class InteractionHandler
    {
        /// <summary>
        /// Runs some code when an interaction from matching user with a matching GUID is received, within time limit
        /// </summary>
        /// <param name="func">The code to run when matching interaction is received</param>
        /// <param name="ctxt">Context to get user and chnl from</param>
        /// <param name="guid">GUID of the cmd</param>
        /// <param name="ms">Time limit to expire cmd</param>
        /// <returns>Whether the interaction was received in time or not</returns>
        [Obsolete("Use NextButtonAsync instead")]
        public static async Task<bool> RunWhenReceived(Func<SocketInteraction, Task> func, SocketCommandContext ctxt, Guid guid, int ms = 15000)
        {
            Func<SocketInteraction, Task> wrap = null;
            bool receivedInput = false;
            wrap = async (act) =>
            {
                if (
                act.Type == Discord.InteractionType.MessageComponent &&
                act.Channel.Id == ctxt.Channel.Id &&
                act.User.Id == ctxt.User.Id &&
                act.Data.ToString().Contains(guid.ToString())
                    )
                {
                    receivedInput = true;
                    await func(act);
                    Program.Client.InteractionCreated -= wrap;
                }
            };
            Program.Client.InteractionCreated += wrap;
            await Task.Delay(ms);
            Program.Client.InteractionCreated -= wrap;
            return receivedInput;
        }
        public static async Task<SocketMessageComponent> NextButtonAsync(Predicate<SocketMessageComponent> filter = null, CancellationToken cancellationToken = default)
        {
            filter ??= m => true;

            var cancelSource = new TaskCompletionSource<bool>();
            var componentSource = new TaskCompletionSource<SocketMessageComponent>();
            var cancellationRegistration = cancellationToken.Register(() => cancelSource.SetResult(true));

            var componentTask = componentSource.Task;
            var cancelTask = cancelSource.Task;

            Task CheckComponent(SocketMessageComponent comp)
            {
                if (filter.Invoke(comp))
                {
                    componentSource.SetResult(comp);
                }

                return Task.CompletedTask;
            }

            Task HandleInteraction(SocketInteraction arg)
            {
                if (arg is SocketMessageComponent comp)
                {
                    return CheckComponent(comp);
                }

                return Task.CompletedTask;
            }

            try
            {
                Program.Client.InteractionCreated += HandleInteraction;

                var result = await Task.WhenAny(componentTask, cancelTask).ConfigureAwait(false);

                return result == componentTask
                    ? await componentTask.ConfigureAwait(false)
                    : null;
            }
            finally
            {
                Program.Client.InteractionCreated -= HandleInteraction;
                cancellationRegistration.Dispose();
            }
        }
    }
}
