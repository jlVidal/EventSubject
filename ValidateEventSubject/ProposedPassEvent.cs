using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vidal.Event;

namespace ValidateEventSubject
{
    [TestClass]
    public class ProposedPassEvent
    {
        public class Foo
        {
            private IObservable<System.Reactive.Unit> _observable;

            public Foo(IObservable<System.Reactive.Unit> observable)
            {
                this._observable = observable;
            }

            public void StartListen()
            {
                this._observable.Subscribe(_ => NextEvent());
            }

            private void NextEvent()
            {
                EventCount++;
            }

            public int EventCount { get; set; }

        }
        [TestMethod]
        public void EventSubjectPassJustAnEvent()
        {
            var ev = new EventSubject<System.Reactive.Unit>();
            var foo = new Foo(ev.Observe());
            foo.StartListen();
            ev.OnNext(System.Reactive.Unit.Default);
            Assert.IsTrue(foo.EventCount == 1);
            ev.Dispose();

            try
            {
                foo.StartListen();
            }
            catch (ObjectDisposedException ex)
            {
                Assert.IsTrue(ex.Message.Contains(typeof(EventSubject<System.Reactive.Unit>).Name));
            }
        }

        public class Fire
        {
            private Action<System.Reactive.Unit> _raiseValue;
            public Fire(Action<System.Reactive.Unit> raiseValueDelegate)
            {
                this._raiseValue = raiseValueDelegate;
            }

            public void Shoot()
            {
                this._raiseValue(System.Reactive.Unit.Default);
            }
        }
        [TestMethod]
        public void EventSubjectPassToPublish()
        {
            Fire f;
            using (var ev = new EventSubject<System.Reactive.Unit>())
            {
                bool eventCalled = false;
                var disp = ev.Observe().Subscribe(_ => eventCalled = true);
                f = new Fire(ev.OnNext);
                f.Shoot();
                Assert.IsTrue(eventCalled);
                eventCalled = false;
                disp.Dispose();
                f.Shoot();
                Assert.IsFalse(eventCalled);
            }
            try
            {
                f.Shoot();
            }
            catch (ObjectDisposedException ex)
            {
                Assert.IsTrue(ex.Message.Contains(typeof(EventSubject<System.Reactive.Unit>).Name));
            }
        }
    }


}
