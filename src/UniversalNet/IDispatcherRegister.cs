using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalNet;

public interface IDispatcherRegister<T> where T : notnull
{
    public void Register(IDispatcher<T> dispatcher);

    public static void RegisterAll(IServiceProvider provider, IDispatcher<T> dispatcher)
    {
        var dispatchers = (IEnumerable<IDispatcherRegister<T>>?)provider.GetService(typeof(IEnumerable<IDispatcherRegister<T>>));

        if (dispatchers is null)
        {
            return;
        }

        foreach (var register in dispatchers)
        {
            register.Register(dispatcher);
        }
    }
}
