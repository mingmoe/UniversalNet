using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalNet;

public sealed class StringPacketFormatter : PacketFormatters<string>
{
    public Encoding Encoding { get; } = new UTF8Encoding(false, true);

    public override string DecodeId(ReadOnlySequence<byte> data)
    {
        return Encoding.UTF8.GetString(data);
    }

    public override Memory<byte> EncodeId(string id)
    {
        return Encoding.UTF8.GetBytes(id);
    }
}
