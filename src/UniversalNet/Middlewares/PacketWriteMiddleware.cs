using System.Buffers;
using System.Diagnostics;
using System.Net;

namespace UniversalNet.Middlewares;
public class PacketWriteMiddleware<T> : IMiddleware<T> where T : notnull
{
	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		return ValueTask.CompletedTask;
	}

	public async Task InvokeAsync(IConnectionContext<T> context, IMiddleware<T>.NextMiddleware next)
	{
		var output = context.Transport.Output;
		var token = context.ConnectionClosed;
		while (context.PacketToWrite.Reader.TryRead(out var packet))
		{
			await writeWithLength(packet.Id);
			await writeWithLength(packet.Data);
		}

		await next(context).ConfigureAwait(false);

		async Task writeWithLength(ReadOnlySequence<byte> buffer)
		{
			if (buffer.Length > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException(
					nameof(buffer),
					"Buffer to write is too long(max length:int.MaxValue)");
			}

			byte[] length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int) buffer.Length));

			Debug.Assert(length.Length == 4);

			await output.WriteAsync(length, token).ConfigureAwait(false);

			if (buffer.IsSingleSegment)
			{
				// fastpath
				await output.WriteAsync(buffer.First, token).ConfigureAwait(false);
			}
			else
			{
				await output.WriteAsync(buffer.ToArray(), token).ConfigureAwait(false);
			}
		}
	}
}
