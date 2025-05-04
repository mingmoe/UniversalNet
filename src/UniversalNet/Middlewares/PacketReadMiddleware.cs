
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;

namespace UniversalNet.Kestrel;

/// <summary>
/// ʹ������м��,���Ǳ��뱣֤����<see cref="IConnectionContext{T}.PacketToParse"/>�����¸��м��������.
/// ����ᵼ���ڴ���ʴ���,��Ϊ����м�������¸��м�����غ������ڴ�.
/// </summary>
/// <typeparam name="T"></typeparam>
public class PacketReadMiddleware<T> : IMiddleware<T> where T : notnull
{
    public required ILogger<PacketReadMiddleware<T>> Logger { get; init; }

    public async Task InvokeAsync(IConnectionContext<T> context, IMiddleware<T>.NextMiddle next)
    {
        var input = context.Transport.Input;
        var token = context.ConnectionClosed;

        // always read
        while (!token.IsCancellationRequested)
        {
            // read id
            T id;
            {
                int idLength = await PipeUility.ReadInt(input, token).ConfigureAwait(false);
                using var idRead = await PipeUility.Read(input, idLength, token).ConfigureAwait(false);
                id = context.Packetizer.DecodeId(idRead.Buffer);
            }
            // read packet
            int packetLength = await PipeUility.ReadInt(input, token).ConfigureAwait(false);
            using var packetRead = await PipeUility.Read(input, packetLength, token).ConfigureAwait(false);
            RawPacket<T> packet = new(id, packetRead.Buffer);

            // write packet
            await context.PacketToParse.Writer.WriteAsync(packet, token).ConfigureAwait(false);

            // process the packet
            await next.Invoke(context).ConfigureAwait(false);
        }

        // process the packet
        await next.Invoke(context).ConfigureAwait(false);
    }
}