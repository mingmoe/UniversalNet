using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalNet.Middlewares;

public class PacketDecodeMiddleware<T> : IMiddleware<T> where T : notnull
{

    public async Task InvokeAsync(IConnectionContext<T> context, IMiddleware<T>.NextMiddle next)
    {
        while (context.PacketToParse.Reader.TryRead(out var packet))
        {
            var parsed = context.Packetizer.Decode(packet.Id, packet.Data);

            await context.PacketToDispatch.Writer.WriteAsync(new(packet.Id, parsed)).ConfigureAwait(false);
        }

        await next(context).ConfigureAwait(false);
    }
}
