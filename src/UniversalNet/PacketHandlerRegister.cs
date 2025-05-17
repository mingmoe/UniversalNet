using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalNet;
public sealed class PacketHandlerRegister<T, F>(F handler) : IDispatcherRegister<T>
    where T : notnull where F : IPacketHandler<T>, IWithPacketId<T>
{
    public F Handler { get; init; } = handler;

    public void Register(IDispatcher<T> packetizer)
    {
        packetizer.Handlers.Add(F.PacketId, Handler);
    }
}
