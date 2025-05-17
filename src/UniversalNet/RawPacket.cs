
using System.Buffers;

namespace UniversalNet;

public struct RawPacket<T> where T : notnull
{
    public ReadOnlySequence<byte> Data { get; set; }

    public ReadOnlySequence<byte> Id { get; set; }

    public RawPacket(ReadOnlySequence<byte> Id)
    {
        Data = new();
        this.Id = Id;
    }

    public RawPacket(ReadOnlySequence<byte> Id, ReadOnlySequence<byte> data)
    {
        Data = data;
        this.Id = Id;
    }
}
