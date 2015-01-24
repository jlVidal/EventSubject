using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vidal.Event.Experimental
{
    public interface IPatternDelegate<TArgs>
    {
        event EventHandler<TArgs> EventHandler;
    }
}
