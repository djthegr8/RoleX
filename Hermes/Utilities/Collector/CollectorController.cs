using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hermes.Utilities.Collector
{
    public class CollectorController
    {
        private Timer? _timer;

        public TaskCompletionSource<CollectorEventArgsBase?>? TaskCompletionSource;
        public event EventHandler<CollectorEventArgsBase>? RemoveArgsFailed;

        public void SetTimeout(TimeSpan timeout)
        {
            _timer = new Timer(state => { Dispose(); }, null, timeout, TimeSpan.FromSeconds(0));
        }

        public event EventHandler? Stop;

        public void Dispose()
        {
            TaskCompletionSource?.SetResult(null);
            Stop?.Invoke(null, EventArgs.Empty);
            _timer?.Dispose();
        }

        public virtual void OnRemoveArgsFailed(CollectorEventArgsBase e)
        {
            RemoveArgsFailed?.Invoke(this, e);
        }

        public async Task<CollectorEventArgsBase?> WaitForEventOrDispose()
        {
            if (TaskCompletionSource != null) return await TaskCompletionSource.Task;
            TaskCompletionSource = new TaskCompletionSource<CollectorEventArgsBase?>();
            var result = await TaskCompletionSource.Task;
            TaskCompletionSource = null;
            return result;
        }
    }

    public enum CollectorFilter
    {
        Off,
        IgnoreSelf,
        IgnoreBots
    }
}