
using System.Buffers;

namespace UniversalNet;

public struct RawPacket<T> where T : notnull
{
    public ReadOnlySequence<byte> Data { get; set; }

    public T Id { get; set; }

    public RawPacket(T Id)
    {
        Data = new();
        this.Id = Id;
    }

    public RawPacket(T Id, ReadOnlySequence<byte> data)
    {
        Data = data;
        this.Id = Id;
    }
}
