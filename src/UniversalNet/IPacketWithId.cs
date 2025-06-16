
namespace UniversalNet;

public interface IWithPacketId<T> where T : notnull
{
	static abstract T PacketId { get; }
}
