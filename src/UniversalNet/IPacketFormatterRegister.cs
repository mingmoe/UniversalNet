using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalNet;
public interface IPacketFormatterRegister<T> where T : notnull
{
    void Register(IPacketFormatters<T> packetizer);

    public static void RegisterAll(IServiceProvider provider, IPacketFormatters<T> packetizer)
    {
        var packetizers = (IEnumerable<IPacketFormatterRegister<T>>?)provider.GetService(typeof(IEnumerable<IPacketFormatterRegister<T>>));

        if (packetizers is null)
        {
            return;
        }

        foreach (var register in packetizers)
        {
            register.Register(packetizer);
        }
    }
}
