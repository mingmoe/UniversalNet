using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UniversalNet.Middlewares;
public class PacketWriteMiddleware<T> : IMiddleware<T> where T : notnull
{
    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    public async Task InvokeAsync(IConnectionContext<T> context, IMiddleware<T>.NextMiddle next)
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

            byte[] length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)buffer.Length));

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
