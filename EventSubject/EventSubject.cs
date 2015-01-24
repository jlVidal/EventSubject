using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vidal.Event
{
    public static class EventSubject
    {
        public static EventSubject<TArgs> Create<TArgs>()
        {
            return new EventSubject<TArgs>();
        }

        public static IObservable<TArgs> CreateAndObserve<TArgs>(out EventSubject<TArgs> react)
        {
            react = new EventSubject<TArgs>();
            return react.Observe();
        }
    }

    public sealed class EventSubject<TArgs> : IEventSubject<TArgs>, IDisposable
    {
        #region [ Fields / Attributes ]
        private Action _disposeAction;
        private Action<TArgs> _delegates;

        private volatile bool _isDisposed;

        private readonly object _gateEvent = new object();
        #endregion

        #region [ Events / Properties ]
        public event Action<TArgs> Handler
        {
            add
            {
                RegisterEventDelegate(value);
            }
            remove
            {
                UnRegisterEventDelegate(value);
            }
        }
        #endregion

        private void AddDisposeAction(Action value)
        {
            var baseVal = _disposeAction;
            while (true)
            {
                var newVal = baseVal + value;
                var currentVal = Interlocked.CompareExchange(ref _disposeAction, newVal, baseVal);

                if (currentVal == baseVal)
                    return;

                baseVal = currentVal; // Try again
            }
        }
        private void RemoveDisposeAction(Action value)
        {
            var baseVal = _disposeAction;
            if (baseVal == null)
                return;

            while (true)
            {
                var newVal = baseVal - value;
                var currentVal = Interlocked.CompareExchange(ref _disposeAction, newVal, baseVal);

                if (currentVal == baseVal)
                    return;

                baseVal = currentVal; // Try again
            }
        }

        private void RegisterEventDelegate(Action<TArgs> invoker)
        {
            if (invoker == null)
                throw new NullReferenceException("invoker");

            lock (_gateEvent)
            {
                CheckDisposed(); // check inside of lock because of disposable synchronization

                if (IsAlreadySubscribed(invoker))
                    return;

                AddActionInternal(invoker);
            }
        }

        private bool IsAlreadySubscribed(Action<TArgs> invoker)
        {
            var current = _delegates;
            if (current == null)
                return false;

            var items = current.GetInvocationList();
            for (int i = items.Length; i-- > 0; )
            {
                if ((Action<TArgs>)items[i] == invoker)
                    return true;
            }
            return false;
        }

        private void UnRegisterEventDelegate(Action<TArgs> invoker)
        {
            if (invoker == null)
                return;

            lock (_gateEvent)
            {
                var baseVal = _delegates;
                if (baseVal == null)
                    return;

                RemoveActionInternal(invoker);
            }
        }

        private void AddActionInternal(Action<TArgs> invoker)
        {
            var baseVal = _delegates;
            while (true)
            {
                var newVal = baseVal + invoker;
                var currentVal = Interlocked.CompareExchange(ref _delegates, newVal, baseVal);

                if (currentVal == baseVal) // success
                    return;

                baseVal = currentVal;
            }
        }

        private void RemoveActionInternal(Action<TArgs> invoker)
        {
            var baseVal = _delegates;
            while (true)
            {
                var newVal = baseVal - invoker;
                var currentVal = Interlocked.CompareExchange(ref _delegates, newVal, baseVal);

                if (currentVal == baseVal)
                    return;

                baseVal = currentVal; // Try again
            }
        }
        public IObservable<TArgs> Observe()
        {
            return Observable.Defer(() =>
            {
                CheckDisposed();
                return Observable.FromEvent<TArgs>(AddActionInternal, RemoveActionInternal);
            })
            .TakeUntil(Observable.FromEvent(AddDisposeAction, RemoveDisposeAction));
        }

        public void OnNext(TArgs value)
        {
            CheckDisposed();

            var current = _delegates;
            if (current == null)
                return;

            current(value);
        }

        public void Dispose()
        {
            _isDisposed = true;
            _delegates = null;

            try
            {
                var disposeDelegates = _disposeAction;
                if (disposeDelegates == null)
                    return;

                _disposeAction = null;
                disposeDelegates();
            }
            finally
            {
                lock (_gateEvent)
                {
                    _delegates = null; // re-clean with sync
                }
            }
        }
        private void CheckDisposed()
        {
            if (_isDisposed)
            {
                ThrowDisposed();
            }
        }

        private void ThrowDisposed()
        {
            throw new ObjectDisposedException(this.GetType().Name);
        }
    }
}
