
using System.Buffers;
using System.Collections.Concurrent;

namespace UniversalNet;

/// <summary>
///     包序列化/逆序列化器.
/// </summary>
public interface IPacketFormatters<T> where T : notnull
{
    IDictionary<T, IPacketFormatter<T>> Formatters { get; }

    T DecodeId(ReadOnlySequence<byte> data);

    Memory<byte> EncodeId(T id);

    /// <summary>
    ///     把字节序列转换为包.
    /// </summary>
    object Decode(T packetTypeId, ReadOnlySequence<byte> data);

    /// <summary>
    ///     把包转化为字节序列
    /// </summary>
    Memory<byte> Encode(T packetTypeId, object obj);
}