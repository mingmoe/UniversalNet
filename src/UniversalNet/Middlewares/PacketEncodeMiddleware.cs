using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalNet.Middlewares;
public class PacketEncodeMiddleware<T> : IMiddleware<T> where T : notnull
{

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    public async Task InvokeAsync(IConnectionContext<T> context, IMiddleware<T>.NextMiddleware next)
    {
        while (context.PacketToSend.Reader.TryRead(out var packet))
        {
            var encodedId = context.Packetizer.EncodeId(packet.Id);
            var encoded = context.Packetizer.Encode(packet.Id, packet.Obj);

            await context.PacketToWrite.Writer.WriteAsync(new(new(encodedId), new(encoded))).ConfigureAwait(false);
        }

        await next(context).ConfigureAwait(false);
    }
}
