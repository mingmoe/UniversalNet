using System.IO.Pipelines;
using System.Threading.Channels;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;

namespace UniversalNet.Kestrel;

public class KestrelConnectionContext<T>(ConnectionContext connectionContext) : ConnectionContext, IConnectionContext<T> where T : notnull
{
    public override string ConnectionId
    {
        get
        {
            return connectionContext.ConnectionId;
        }
        set
        {
            connectionContext.ConnectionId = value;
        }
    }

    public required IDispatcher<T> Dispatcher { get; init; }
    public required IPacketFormatters<T> Packetizer { get; init; }
    public Channel<RawPacket<T>> PacketToParse { get; init; } = Channel.CreateUnbounded<RawPacket<T>>();
    public Channel<ParsedPacket<T>> PacketToDispatch { get; init; } = Channel.CreateUnbounded<ParsedPacket<T>>();
    public Channel<ParsedPacket<T>> PacketToSend { get; init; } = Channel.CreateUnbounded<ParsedPacket<T>>();
    public Channel<RawPacket<T>> PacketToWrite { get; init; } = Channel.CreateUnbounded<RawPacket<T>>();
    public override IDuplexPipe Transport
    {
        get
        {
            return connectionContext.Transport;
        }
        set
        {
            connectionContext.Transport = value;
        }
    }

    public override IDictionary<object, object?> Items
    {
        get
        {
            return connectionContext.Items;
        }
        set
        {
            connectionContext.Items = value;
        }
    }

    public override CancellationToken ConnectionClosed
    {
        get
        {
            return connectionContext.ConnectionClosed;
        }
        set
        {
            connectionContext.ConnectionClosed = value;
        }
    }

    public override IFeatureCollection Features
    {
        get
        {
            return connectionContext.Features;
        }
    }

    public override void Abort()
    {
        connectionContext.Abort();
    }

    public override ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return connectionContext.DisposeAsync();
    }
}
