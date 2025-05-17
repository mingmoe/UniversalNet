

namespace UniversalNet;

public interface IPacketHandler<T> : IAsyncDisposable where T : notnull
{
    public Task Handle(IConnectionContext<T> context, T packetId, object packet);
}