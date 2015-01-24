using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Vidal.Event.Experimental;

namespace ValidateEventSubject
{
    [TestClass]
    public class PatternEventHandlerExperiemental
    {
        int countEvent;
        [TestMethod]
        public void NETEventHandlerUsage()
        {
            using (var ev = new PatternEventSubject<Unit>())
            {
                countEvent = 0;
                ev.EventHandler += ev_EventHandler;
                ev.EventHandler += ev_EventHandler;
                ev.OnNext(new EventPattern<Unit>(this, Unit.Default));
                ev.EventHandler -= ev_EventHandler;
                Assert.IsTrue(countEvent == 1);
                countEvent = 0;
                EventHandler<Unit> handler = (s, e) => { countEvent++; };
                ev.EventHandler += handler;
                ev.EventHandler += handler;
                ev.EventHandler -= handler;
                Assert.IsTrue(countEvent == 0);
                ev.OnNext(new EventPattern<Unit>(this, Unit.Default));
            }
        }

        void ev_EventHandler(object sender, Unit e)
        {
            countEvent++;
        }

    }
}
