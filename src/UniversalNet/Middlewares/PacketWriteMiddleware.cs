using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UniversalNet.Middlewares;
public class PacketWriteMiddleware<T> : IMiddleware<T> where T : notnull
{
    public async Task InvokeAsync(IConnectionContext<T> context, IMiddleware<T>.NextMiddle next)
    {
        while (context.PacketToWrite.Reader.TryRead(out var packet))
        {
            var id = context.Packetizer.EncodeId(packet.Id);
            // note: covert native endian to network
            var idLength = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(id.Length));

            var data = packet.Data;
            var dataLength = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));

            await context.Transport.Output.WriteAsync(idLength).ConfigureAwait(false);
            await context.Transport.Output.WriteAsync(id).ConfigureAwait(false);
            await context.Transport.Output.WriteAsync(dataLength).ConfigureAwait(false);

            // fastpath
            if (data.IsSingleSegment)
            {
                await context.Transport.Output.WriteAsync(data.First).ConfigureAwait(false);
            }
            else
            {
                await context.Transport.Output.WriteAsync(data.ToArray()).ConfigureAwait(false);
            }
        }

        await next(context).ConfigureAwait(false);
    }
}
