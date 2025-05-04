using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalNet.Middlewares;
public class PacketEncodeMiddleware<T> : IMiddleware<T> where T : notnull
{
    public async Task InvokeAsync(IConnectionContext<T> context, IMiddleware<T>.NextMiddle next)
    {
        while (context.PacketToSend.Reader.TryRead(out var packet))
        {
            var parsed = context.Packetizer.Encode(packet.Id, packet.Obj);

            await context.PacketToWrite.Writer.WriteAsync(new(packet.Id, new(parsed))).ConfigureAwait(false);
        }

        await next(context).ConfigureAwait(false);
    }
}
