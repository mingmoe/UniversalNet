
using System.Collections.Concurrent;

namespace UniversalNet;

public interface IDispatcher<T> where T : notnull{
    ConcurrentDictionary<T, IPacketHandler<T>> Handlers { get; }
    Task<bool> Dispatch(IConnectionContext<T> context,T packetId,object packet);
}
