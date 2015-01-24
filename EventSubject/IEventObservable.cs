using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vidal.Event
{
    public interface IEventObservable<out TArgs>
    {
        IObservable<TArgs> Observe();
    }
}
