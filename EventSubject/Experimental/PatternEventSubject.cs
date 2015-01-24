using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Threading;

namespace Vidal.Event.Experimental
{
    public sealed class PatternEventSubject<TArgs> : IEventSubject<EventPattern<TArgs>>, IPatternDelegate<TArgs>, IDisposable
    {
        private struct ProxyEventPatternHandler
        {
            private EventHandler<TArgs> _handler;
            public ProxyEventPatternHandler(EventHandler<TArgs> arg)
            {
                this._handler = arg;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode() ^ _handler.GetHashCode();
            }
            public override bool Equals(object obj)
            {
                return ((ProxyEventPatternHandler)obj)._handler == this._handler;
            }

            public void Invoke(EventPattern<TArgs> args)
            {
                _handler(args.Sender, args.EventArgs);
            }
        }

        #region [ Fields / Attributes ]
        private Action _disposeAction;
        private Action<EventPattern<TArgs>> _delegates;

        private volatile bool _isDisposed;

        private readonly object _gateDelegate = new object();
        private readonly HashSet<ProxyEventPatternHandler> _proxies = new HashSet<ProxyEventPatternHandler>();
        #endregion

        #region [ Events / Properties ]
        public event Action<EventPattern<TArgs>> Handler
        {
            add
            {
                RegisterDelegate(value);
            }
            remove
            {
                RemoveDelegate(value);
            }
        }

     
        public event EventHandler<TArgs> EventHandler
        {
            add
            {
                RegisterEventHandler(value);
            }
            remove
            {
                RemoveEventHandler(value);
            }
        }

        private void RegisterEventHandler(EventHandler<TArgs> value)
        {
            if (value == null)
                return;

            lock (_gateDelegate)
            {
                CheckDisposed(); // check inside of lock because of disposable synchronization

                var px = new ProxyEventPatternHandler(value); // check if it is already subscribed
                if (_proxies.Add(px))
                {
                    AddActionInternal(px.Invoke);
                }
            }
        }

        private void RemoveEventHandler(EventHandler<TArgs> value)
        {
            lock (_gateDelegate)
            {
                var pr = new ProxyEventPatternHandler(value);
                if (_proxies.Remove(pr))
                {
                    Handler -= pr.Invoke;
                }
            }
        }
        #endregion

        private void AddDisposeAction(Action value)
        {
            if (_isDisposed)
                return;

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

        private void RegisterDelegate(Action<EventPattern<TArgs>> invoker)
        {
            if (invoker == null)
                throw new NullReferenceException("invoker");

            lock (_gateDelegate)
            {
                CheckDisposed(); // check inside of lock because of disposable synchronization

                if (IsAlreadySubscribed(invoker))
                    return;

                AddActionInternal(invoker);
            }
        }

        private bool IsAlreadySubscribed(Action<EventPattern<TArgs>> invoker)
        {
            var current = _delegates;
            if (current == null)
                return false;

            var items = current.GetInvocationList();
            for (int i = items.Length; i-- > 0; )
            {
                if ((Action<EventPattern<TArgs>>)items[i] == invoker)
                    return true;
            }
            return false;
        }

        private void RemoveDelegate(Action<EventPattern<TArgs>> invoker)
        {
            if (invoker == null)
                return;

            lock (_gateDelegate)
            {
                var baseVal = _delegates;
                if (baseVal == null)
                    return;

                RemoveActionInternal(invoker);
            }
        }

        private void AddActionInternal(Action<EventPattern<TArgs>> invoker)
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

        private void RemoveActionInternal(Action<EventPattern<TArgs>> invoker)
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
        public IObservable<EventPattern<TArgs>> Observe()
        {
            return Observable.Defer(() =>
            {
                CheckDisposed();
                return Observable.FromEvent<EventPattern<TArgs>>(AddActionInternal, RemoveActionInternal);
            })
            .TakeUntil(Observable.FromEvent(AddDisposeAction, RemoveDisposeAction));
        }

        public void OnNext(EventPattern<TArgs> value)
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
                lock (_gateDelegate)
                {
                    _delegates = null; // re-clean with sync
                    _proxies.Clear();
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
