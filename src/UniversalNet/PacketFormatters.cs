using System.Buffers;

namespace UniversalNet;

public abstract class PacketFormatters<T> : IPacketFormatters<T> where T : notnull
{
	public IDictionary<T, IPacketFormatter<T>> Formatters { get; init; } = new Dictionary<T, IPacketFormatter<T>>();

	public object Decode(T packetTypeId, ReadOnlySequence<byte> data)
	{
		if (!Formatters.TryGetValue(packetTypeId, out var formatter))
			throw new InvalidOperationException($"unknown packet type id:{packetTypeId}");

		return formatter.GetValue(packetTypeId, data);
	}

	public abstract T DecodeId(ReadOnlySequence<byte> data);

	public Memory<byte> Encode(T packetTypeId, object obj)
	{
		if (!Formatters.TryGetValue(packetTypeId, out var formatter))
			throw new InvalidOperationException("unknown packet type id");

		return formatter.ToPacket(packetTypeId, obj);
	}

	public abstract Memory<byte> EncodeId(T id);
}
