using MemoryPack;
using System.Buffers;
using System.Diagnostics;

namespace UniversalNet.MemoryPack;

public class MemorypackPacketFormatter<T, P> : IPacketFormatter<T>, IWithPacketId<T> where T : notnull where P : IWithPacketId<T>
{
	public static T PacketId => P.PacketId;

	private static readonly Lazy<MemorypackPacketFormatter<T, P>> _instance = new(true);

	public object GetValue(T packetId, ReadOnlySequence<byte> packet)
	{
		Debug.Assert(packetId.Equals(P.PacketId));

		var obj = MemoryPackSerializer.Deserialize<P>(packet);

		return obj ?? throw new InvalidOperationException("MemoryPackSerizlizer.Deserialize<T>() returns null");
	}

	public Memory<byte> ToPacket(T packetId, object value)
	{
		Debug.Assert(packetId.Equals(P.PacketId));

		return MemoryPackSerializer.Serialize((P) value);
	}
}
