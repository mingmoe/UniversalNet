namespace UniversalNet;
public sealed class Dispatcher<T> : IDispatcher<T> where T : notnull
{
	public IDictionary<T, IPacketHandler<T>> Handlers { get; init; } = new Dictionary<T, IPacketHandler<T>>();

	public async Task<bool> Dispatch(IConnectionContext<T> context, T packetId, object packet)
	{
		if (Handlers.TryGetValue(packetId, out var handler))
		{
			await handler.Handle(context, packetId, packet).ConfigureAwait(false);
			return true;
		}

		return false;
	}
}
