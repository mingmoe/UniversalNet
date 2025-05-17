
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Net;
using System.Reflection.Metadata;
using static UniversalNet.PipeUility;
using ReadResult = UniversalNet.PipeUility.ReadResult;

namespace UniversalNet.Middlewares;

/// <summary>
/// 使用这个中间件,我们必须保证所有<see cref="IConnectionContext{T}.PacketToParse"/>都在下个中间件被处理.
/// 否则会导致内存访问错误,因为这个中间件会在下个中间件返回后会清空内存.
/// </summary>
/// <typeparam name="T"></typeparam>
public class PacketReadMiddleware<T> : IMiddleware<T> where T : notnull
{
    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    public required ILogger<PacketReadMiddleware<T>> Logger { get; init; }

    private sealed class PacketRead
    {
        public required PipeReader Reader { get; init; }
        public int? IdLength { get; set; } = null;
        public ReadOnlySequence<byte>? Id { get; set; } = null;
        public int? PacketLength { get; set; } = null;

        private void Reset()
        {
            IdLength = null;
            Id = null;
            PacketLength = null;
        }

        public bool TryRead(
            [NotNullWhen(true)] out ReadResult? result,
            [NotNullWhen(true)] out RawPacket<T>? packet)
        {
            result = null;
            packet = null;

            if (IdLength is null)
            {
                var read = TryReadInternetInt(Reader);

                if (read is null)
                {
                    return false;
                }

                IdLength = read.Value.Item1;
                read.Value.Item2.Consume();
            }

            if (Id is null)
            {
                var read = PipeUility.TryRead(Reader, IdLength.Value);

                if (read is null)
                {
                    return false;
                }

                Id = read.Value.SlicedBuffer;
                read.Value.Examine();
            }

            if (PacketLength is null)
            {
                var read = TryReadInternetInt(Reader, IdLength.Value);

                if (read is null)
                {
                    return false;
                }

                PacketLength = read.Value.Item1;
                read.Value.Item2.Examine();
            }

            result = PipeUility.TryRead(Reader, PacketLength.Value, IdLength.Value + sizeof(int));

            if (result is null)
            {
                return false;
            }

            // get it
            packet = new(Id.Value, result.Value.SlicedBuffer);

            // ready for next
            Reset();

            return true;
        }
    }

    public async Task InvokeAsync(IConnectionContext<T> context, IMiddleware<T>.NextMiddle next)
    {
        var input = context.Transport.Input;
        var token = context.ConnectionClosed;
        var read = new PacketRead() { Reader = input };

        while (!token.IsCancellationRequested)
        {
            await next.Invoke(context).ConfigureAwait(false);

            if (read.TryRead(out var readResult, out var packet))
            {
                try
                {
                    await context.PacketToParse.Writer.WriteAsync(packet.Value, token).ConfigureAwait(false);
                    await next.Invoke(context).ConfigureAwait(false);
                }
                finally
                {
                    readResult.Value.Consume();
                }
            }
        }

        // process the packet
        await next.Invoke(context).ConfigureAwait(false);
    }
}