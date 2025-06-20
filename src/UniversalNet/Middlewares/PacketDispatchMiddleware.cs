﻿using Microsoft.Extensions.Logging;

namespace UniversalNet.Middlewares;

public class PacketDispatchMiddleware<T> : IMiddleware<T> where T : notnull
{
	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		return ValueTask.CompletedTask;
	}

	public required ILogger<PacketDispatchMiddleware<T>> Logger { get; init; }

	public async Task InvokeAsync(IConnectionContext<T> context, IMiddleware<T>.NextMiddleware next)
	{
		while (context.PacketToDispatch.Reader.TryRead(out var packet))
		{
			var result = await context.Dispatcher.Dispatch(context, packet.Id, packet.Obj).ConfigureAwait(false);

			if (!result)
			{
				Logger.LogError("Packet with Id {Id} has no handler", packet.Id);
			}
		}

		await next(context).ConfigureAwait(false);
	}
}
