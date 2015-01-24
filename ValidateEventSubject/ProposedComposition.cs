using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vidal.Event;
using System.Reactive.Linq;
using System.Diagnostics;

namespace ValidateEventSubject
{
    [TestClass]
    public class ProposedComposition
    {
        [TestMethod]
        public void ObservableMap() // Using Select Operator 
        {
            var values = new List<string>();
            using (var ev = new EventSubject<Tuple<int, string>>())
            using (var sub = ev.Observe().Select(obj => obj.Item2)
                                        .Subscribe(values.Add))
            {
                ev.OnNext(new Tuple<int, string>(1, "First"));
                ev.OnNext(new Tuple<int, string>(2, "Second"));
                ev.OnNext(new Tuple<int, string>(3, "Third"));

                Assert.IsTrue(values.SequenceEqual(new[] { "First", "Second", "Third" }));
            }
        }

        [TestMethod]
        public void ObservableFilter() // Using Where Operator
        {
            var values = new List<int>();
            using (var ev = new EventSubject<Tuple<int, string>>())
            using (var sub = ev.Observe().Where(obj => (obj.Item1 % 2) == 0)
                                        .Subscribe(a => values.Add(a.Item1)))
            {
                ev.OnNext(new Tuple<int, string>(1, "First"));
                ev.OnNext(new Tuple<int, string>(2, "Second"));
                ev.OnNext(new Tuple<int, string>(3, "Third"));
                ev.OnNext(new Tuple<int, string>(4, "Fourth"));

                Assert.IsTrue(values.SequenceEqual(new[] { 2, 4 }));
            }
        }

        [TestMethod]
        public void ObservableReduce() // Using Sum Operator
        {
            var values = new List<int>();
            var ev = new EventSubject<Tuple<int, string>>();

            using (var sub = ev.Observe().Sum(a => a.Item1)
                                        .Subscribe(values.Add))
            {
                ev.OnNext(new Tuple<int, string>(1, "First"));
                ev.OnNext(new Tuple<int, string>(2, "Second"));
                ev.OnNext(new Tuple<int, string>(3, "Third"));
                ev.OnNext(new Tuple<int, string>(4, "Fourth"));

                ev.Dispose();

                Assert.IsTrue(values.SequenceEqual(new[] { 10 }));
            }
        }

        [TestMethod]
        public void ObservableMerge() // Using Merge Operator
        {
            var values = new List<Tuple<int,String>>();
            using (var left = new EventSubject<Tuple<int, string>>())
            using (var right = new EventSubject<Tuple<int,string>>())
            using (var sub = left.Observe().Merge(right.Observe())
                                        .Subscribe(values.Add))
            {
                left.OnNext(new Tuple<int, string>(1, "First"));
                right.OnNext(new Tuple<int, string>(2, "Two"));
                left.OnNext(new Tuple<int, string>(3, "Third"));
                right.OnNext(new Tuple<int, string>(4, "Four"));

                Assert.IsTrue(values.SequenceEqual(new[] { Tuple.Create(1,"First"), Tuple.Create(2, "Two"), Tuple.Create(3, "Third"), Tuple.Create(4, "Four") }));
            }
        }

        [TestMethod]
        public void ObservableZip() // Using Zip Operator
        {
            var values = new List<Tuple<int, String, String>>();
            using (var left = new EventSubject<Tuple<int, string>>())
            using (var right = new EventSubject<Tuple<int, string>>())
            using (var sub = left.Observe().Zip(right.Observe(), (a, b) => new { a, b })
                                        .Where(next => next.a.Item1 == next.b.Item1)
                                        .Select(next => Tuple.Create(next.a.Item1, next.a.Item2, next.b.Item2))
                                        .Subscribe(values.Add))
            {
                left.OnNext(new Tuple<int, string>(1, "First"));
                right.OnNext(new Tuple<int, string>(1, "One"));

                left.OnNext(new Tuple<int, string>(2, "Second"));
                left.OnNext(new Tuple<int, string>(3, "Third"));

                right.OnNext(new Tuple<int, string>(2, "Two"));
                right.OnNext(new Tuple<int, string>(3, "Three"));

                Assert.IsTrue(values.SequenceEqual(new[] { Tuple.Create(1, "First", "One"), Tuple.Create(2, "Second", "Two"), Tuple.Create(3, "Third", "Three") }));
            }
        }
    }
}
