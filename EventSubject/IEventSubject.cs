using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vidal.Event
{
    public interface IEventSubject<TArgs> : IEventObservable<TArgs>, IEventPublisher<TArgs>, IEventDelegate<TArgs>
    {
    }

}
