using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vidal.Event;
using System.Reactive;
using System.Reactive.Subjects;
using System.Diagnostics;

namespace ValidateEventSubject
{
    [TestClass]
    public class MainCommonUsage
    {

        [TestMethod]
        public void NETBasicUsage()
        {
            var ev = new EventSubject<Unit>();
            int count = 0;
            ev.Handler += v => count++;
            ev.OnNext(Unit.Default);
            ev.Dispose();
            Assert.IsTrue(count == 1);
        }

        
        [TestMethod]
        public void RxBasicUsage()
        {
            var ev = new EventSubject<Unit>();
            int count = 0;
            ev.Observe().Subscribe(v => count++);
            ev.OnNext(Unit.Default);
            ev.Dispose();
            Assert.IsTrue(count == 1);
        }

        [TestMethod]
        public void RxOnCompletedUsage()
        {
            var ev = new EventSubject<Unit>();
            bool completed = false;
            ev.Observe().Subscribe(v => { }, () => completed = true);
            ev.Dispose();
            Assert.IsTrue(completed);
        }


        [TestMethod]
        public void EventSubjectPublisherDisposeException()
        {
            var ev = new EventSubject<Unit>();
            ev.Dispose();
            try
            {
                ev.OnNext(Unit.Default);
                Assert.Fail();
            }
            catch (ObjectDisposedException ex)
            {
                Assert.IsTrue(ex.Message.Contains(typeof(EventSubject<Unit>).Name));
            }
        }

        [TestMethod]
        public void AddNETEventDisposeException()
        {
            var ev = new EventSubject<Unit>();
            ev.Dispose();
            try
            {
                ev.Handler += (v) => { };
                Assert.Fail();
            }
            catch (ObjectDisposedException ex)
            {
                Assert.IsTrue(ex.Message.Contains(typeof(EventSubject<Unit>).Name));
            }
        }

        [TestMethod]
        public void EventRxSubscriptionDisposeExceptionObservingAfter()
        {
            var ev = new EventSubject<Unit>();
            ev.Dispose();
            try
            {
                ev.Observe().Subscribe();
                Assert.Fail();
            }
            catch (ObjectDisposedException ex)
            {
                Assert.IsTrue(ex.Message.Contains(typeof(EventSubject<Unit>).Name));
            }
        }


        [TestMethod]
        public void EventRxSubscriptionDisposeExceptionObservingBefore()
        {
            var ev = new EventSubject<Unit>();
            var sx = ev.Observe();
            ev.Dispose();
            try
            {
                sx.Subscribe();
                Assert.Fail();
            }
            catch (ObjectDisposedException ex)
            {
                Assert.IsTrue(ex.Message.Contains(typeof(EventSubject<Unit>).Name));
            }
        }


        int eventCalled = 0;
        [TestMethod]
        public void LimitDuplicateNETEvent()
        {
            eventCalled = 0;

            using (var ev = new EventSubject<Unit>())
            {
                ev.Handler += ev_Handler;
                ev.Handler += ev_Handler;
                ev.OnNext(Unit.Default);
            }

            Assert.IsTrue(eventCalled == 1);
        }

        void ev_Handler(Unit obj)
        {
            eventCalled++;
        }
    }
}
