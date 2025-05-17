using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalNet;
public sealed class PacketFormatterRegister<T, F>(F formatter) : IPacketFormatterRegister<T>
    where T : notnull where F : IPacketFormatter<T>, IWithPacketId<T>
{
    public F Formatter { get; init; } = formatter;

    public void Register(IPacketFormatters<T> packetizer)
    {
        packetizer.Formatters.Add(F.PacketId, Formatter);
    }
}
