using System;
using System.Reactive.Subjects;

namespace Hermes.Utilities
{
    public class DisposableBase : IDisposable
    {
        public ISubject<DisposableBase> Disposed { get; } = new Subject<DisposableBase>();
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (!IsDisposed) DisposeInternal();
        }

        protected virtual void DisposeInternal()
        {
            IsDisposed = true;
            Disposed.OnNext(this);
            (Disposed as IDisposable)?.Dispose();
        }
    }
}