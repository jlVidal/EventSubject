using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vidal.Event;
using System.Reactive.Linq;

namespace ValidateEventSubject
{
    [TestClass]
    public class ProposedResourceMaintenance
    {
        private readonly EventSubject<int> _eventSubject = new EventSubject<int>();
        private bool _delegateCalled;

        [TestMethod]
        public void EventSubjectHandleTest()
        {
            _delegateCalled = false;
            _eventSubject.Handler += eventSubject_Handle;
            _eventSubject.OnNext(1);
            _eventSubject.Handler -= eventSubject_Handle;
            Assert.IsTrue(_delegateCalled);

            _delegateCalled = false;
            _eventSubject.OnNext(2);
            Assert.IsFalse(_delegateCalled);
        }

        private void eventSubject_Handle(int value)
        {
            _delegateCalled = true;
            Trace.WriteLine(string.Format("Doing something with '{0}' value.", value));
        }

        [TestMethod]
        public void EventSubjectObsevableTest()
        {
            _delegateCalled = false;

            var disp = _eventSubject.Observe().Subscribe(_ => _delegateCalled = true);
            _eventSubject.OnNext(1);
            disp.Dispose();
            Assert.IsTrue(_delegateCalled);

            _delegateCalled = false;
            _eventSubject.OnNext(2);
            Assert.IsFalse(_delegateCalled);
        }

        [TestMethod]
        public void EventSubjectObservableUnsubscribeTestV1()
        {
            _delegateCalled = false;

            _eventSubject.Observe().Take(1)
                                   .Subscribe(_ => _delegateCalled = true);
            _eventSubject.OnNext(1);
            Assert.IsTrue(_delegateCalled);

            _delegateCalled = false;
            _eventSubject.OnNext(2);
            Assert.IsFalse(_delegateCalled);
        }

        [TestMethod]
        public void EventSubjectObservableUnsubscribeTestV2()
        {
            _delegateCalled = false;

            _eventSubject.Observe()
                        .TakeWhile(next => next < 2)
                        .Subscribe(_ => _delegateCalled = true);
            _eventSubject.OnNext(1);
            Assert.IsTrue(_delegateCalled);

            _delegateCalled = false;
            _eventSubject.OnNext(2);
            Assert.IsFalse(_delegateCalled);
        }

    }
}
