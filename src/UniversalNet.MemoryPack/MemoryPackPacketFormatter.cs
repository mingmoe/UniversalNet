using MemoryPack;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalNet.MemoryPack;
public class MemoryPackPacketFormatter<T, P> : IPacketFormatter<T> where T : notnull where P : IWithPacketId<T>
{
    public object GetValue(T packetId, ReadOnlySequence<byte> packet)
    {
        Debug.Assert(packetId.Equals(P.PacketId));

        var obj = MemoryPackSerializer.Deserialize<P>(packet);

        return obj ?? throw new InvalidOperationException("MemoryPackSerizlizer.Deserialize<T>() returns null");
    }

    public Memory<byte> ToPacket(T packetId, object value)
    {
        Debug.Assert(packetId.Equals(P.PacketId));

        return MemoryPackSerializer.Serialize((P)value);
    }
}
