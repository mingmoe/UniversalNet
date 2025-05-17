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
using System.Threading;
using System.Threading.Tasks;

namespace UniversalNet;

public static class PipeUility
{
    public readonly struct ReadResult
    {
        public required PipeReader Reader { get; init; }

        public required ReadOnlySequence<byte> Buffer { get; init; }

        public required long Offset { get; init; }

        public required long Length { get; init; }

        public readonly SequencePosition SlicedStart => Buffer.GetPosition(Offset);
        public readonly SequencePosition SlicedEnd => Buffer.GetPosition(Offset + Length);

        public readonly ReadOnlySequence<byte> SlicedBuffer => Buffer.Slice(Offset, Length);

        public ReadResult() { }

        public readonly void Examine()
        {
            Reader.AdvanceTo(Buffer.Start, SlicedEnd);
        }

        public readonly void Consume()
        {
            Reader.AdvanceTo(SlicedEnd);
        }
    }

    public static ReadResult? TryRead(PipeReader input, long length, long offset = 0)
    {
        var result = input.TryRead(out var read);

        if (!result)
        {
            return null;
        }

        if (read.Buffer.Length < (offset + length))
        {
            input.AdvanceTo(read.Buffer.Start, read.Buffer.End);
            return null;
        }

        return new ReadResult()
        {
            Reader = input,
            Buffer = read.Buffer,
            Length = length,
            Offset = offset
        };
    }

    public static (int, ReadResult)? TryReadInternetInt(PipeReader input, long offset = 0)
    {
        var read = TryRead(input, sizeof(int), offset);

        if (read == null)
        {
            return null;
        }

        var result = BitConverter.ToInt32(read.Value.SlicedBuffer.ToArray());

        result = IPAddress.NetworkToHostOrder(result);

        return (result, read.Value);
    }

}
