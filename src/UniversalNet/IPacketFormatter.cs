
using System.Buffers;

namespace UniversalNet;

/// <summary>
///     包格式化器
/// </summary>
public interface IPacketFormatter<T> where T : notnull
{
    object GetValue(T packetId, ReadOnlySequence<byte> packet);

    Memory<byte> ToPacket(T packetId, object value);
}
