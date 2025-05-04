using JetBrains.Annotations;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UniversalNet;

public static class PipeUility
{

    [MustDisposeResource]
    public readonly struct ReadResult : IDisposable
    {
        public required PipeReader Reader { get; init; }
        public required ReadOnlySequence<byte> Buffer { get; init; }
        public required SequencePosition AdvancedTo { get; init; }

        public void Dispose()
        {
            Reader.AdvanceTo(AdvancedTo);
            GC.SuppressFinalize(this);
        }
    }

    public static async Task<ReadResult> Read(PipeReader input, int length, CancellationToken token)
    {
        var result = await input.ReadAtLeastAsync(length, token).ConfigureAwait(false);

        if (result.IsCanceled || result.IsCompleted)
        {
            throw new OperationCanceledException();
        }

        return new ReadResult
        {
            Reader = input,
            Buffer = result.Buffer.Slice(result.Buffer.Start, length),
            AdvancedTo = result.Buffer.GetPosition(length)
        };
    }

    public static async Task<int> ReadInt(PipeReader input, CancellationToken token)
    {
        using var read = await Read(input, sizeof(int), token).ConfigureAwait(false);

        var result = BitConverter.ToInt32(read.Buffer.ToArray());

        result = IPAddress.NetworkToHostOrder(result);

        return result;
    }

}
