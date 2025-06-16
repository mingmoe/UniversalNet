namespace UniversalNet.Middlewares;

public class PacketDecodeMiddleware<T> : IMiddleware<T> where T : notnull
{
	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		return ValueTask.CompletedTask;
	}

	public async Task InvokeAsync(IConnectionContext<T> context, IMiddleware<T>.NextMiddleware next)
	{
		while (context.PacketToParse.Reader.TryRead(out var packet))
		{
			var id = context.Packetizer.DecodeId(packet.Id);
			var decoded = context.Packetizer.Decode(id, packet.Data);

			await context.PacketToDispatch.Writer.WriteAsync(new(id, decoded)).ConfigureAwait(false);
		}

		await next(context).ConfigureAwait(false);
	}
}
