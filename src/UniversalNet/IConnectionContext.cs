
using System.IO.Pipelines;
using System.Threading.Channels;

namespace UniversalNet;

public interface IConnectionContext<T> : IAsyncDisposable where T : notnull
{
    string ConnectionId { get; }

    IDispatcher<T> Dispatcher { get; }

    IPacketFormatters<T> Packetizer { get; }

    Channel<RawPacket<T>> PacketToParse { get; }

    Channel<ParsedPacket<T>> PacketToDispatch { get; }

    Channel<ParsedPacket<T>> PacketToSend { get; }

    Channel<RawPacket<T>> PacketToWrite { get; }

    IDuplexPipe Transport { get; }

    IDictionary<object, object?> Items { get; }

    CancellationToken ConnectionClosed { get; }

    void Abort();
}
