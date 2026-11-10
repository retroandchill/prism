using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using MagicArchive;
using MagicArchive.Utilities;

namespace Prism.Metadata;

internal static class MetadataSerializationUtils
{
    extension<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        public void WriteMetadataString(string value)
        {
            if (value.Length == 0)
            {
                writer.WriteCollectionHeader(0);
                return;
            }

            var maxUtf8Size = Encoding.UTF8.GetMaxByteCount(value.Length);
            ref var destPointer = ref writer.GetSpanReference(maxUtf8Size + sizeof(int));
            var dest = MemoryMarshal.CreateSpan(
                ref Unsafe.Add(ref destPointer, sizeof(int)),
                maxUtf8Size
            );
            var status = Utf8.FromUtf16(
                value,
                dest,
                out _,
                out var bytesWritten,
                replaceInvalidSequences: false
            );
            if (status != OperationStatus.Done)
            {
                ArchiveSerializationException.ThrowFailedEncoding(status);
            }

            if (!writer.IsByteSwapping)
                Unsafe.WriteUnaligned(ref destPointer, bytesWritten);
            else
                Unsafe.WriteUnaligned(
                    ref destPointer,
                    BinaryPrimitives.ReverseEndianness(bytesWritten)
                );
            writer.Advance(bytesWritten + sizeof(int));
        }

        public void WriteMetadataBytes(ReadOnlySpan<byte> value)
        {
            if (value.IsEmpty)
                return;

            var byteCount = value.Length;
            ref var dest = ref writer.GetSpanReference(byteCount);
            ref var src = ref MemoryMarshal.GetReference(value);

            Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)byteCount);
            writer.Advance(byteCount);
        }

        public void WriteMetadataRows<T>(ReadOnlySpan<T> rows)
            where T : struct, IArchivable<T>
        {
            if (rows.IsEmpty)
                return;

            if (!writer.IsByteSwapping && BlittableMarshalling.IsBlittable<T>())
            {
                var rawBytes = MemoryMarshal.AsBytes(rows);
                writer.WriteMetadataBytes(rawBytes);
                return;
            }

            foreach (ref readonly var row in rows)
            {
                T.Serialize(ref writer, in row);
            }
        }
    }

    extension(ref ArchiveReader reader)
    {
        public string ReadMetadataString()
        {
            if (!reader.TryReadCollectionHeader(out var utf8Length))
            {
                throw new InvalidOperationException("Expected a non-null string");
            }

            ref var spanRef = ref reader.GetSpanReference(utf8Length);

            var maxBufferSize = Encoding.UTF8.GetMaxCharCount(utf8Length);
            var buffer = ArrayPool<char>.Shared.Rent(maxBufferSize);
            try
            {
                var src = MemoryMarshal.CreateReadOnlySpan(ref spanRef, utf8Length);
                var status = Utf8.ToUtf16(
                    src,
                    buffer,
                    out _,
                    out var strLength,
                    replaceInvalidSequences: false
                );
                if (status != OperationStatus.Done)
                {
                    ArchiveSerializationException.ThrowFailedEncoding(status);
                }

                reader.Advance(utf8Length);
                return new string(buffer.AsSpan(0, strLength));
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        public T[] ReadMetadataRows<T>(int count)
        {
            if (count == 0)
                return [];

            if (!reader.IsByteSwapping && BlittableMarshalling.IsBlittable<T>())
            {
                return reader.ReadBlittableArray<T>(count);
            }

            var result = new T[count];
            var formatter = reader.GetFormatter<T>();
            for (var i = 0; i < count; i++)
            {
                formatter.Deserialize(ref reader, ref result[i]);
            }

            return result;
        }

        private T[] ReadBlittableArray<T>(int length)
        {
            var byteCount = length * Unsafe.SizeOf<T>();
            ref var src = ref reader.GetSpanReference(byteCount);
            var result = GC.AllocateUninitializedArray<T>(length);

            ref var dest = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetArrayDataReference(result));
            Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)byteCount);
            reader.Advance(byteCount);

            return result;
        }
    }

    extension(ArchiveSerializer)
    {
        public static T? Deserialize<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T
        >(
            Stream stream,
            ArchiveSerializerOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            if (
                stream is MemoryStream memoryStream
                && memoryStream.TryGetBuffer(out var streamBuffer)
            )
            {
                cancellationToken.ThrowIfCancellationRequested();
                T? value = default;
                var bytesRead = ArchiveSerializer.Deserialize(
                    streamBuffer.AsSpan(checked((int)memoryStream.Position)),
                    ref value,
                    options
                );
                memoryStream.Seek(bytesRead, SeekOrigin.Current);
                return value;
            }

            var builder = ReusableReadOnlySequenceBuilderPool.Rent();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                builder.ReadFromStream(stream);

                // If single buffer, we can avoid ReadOnlySequence build cost.
                if (builder.TryGetSingleMemory(out var memory))
                {
                    return ArchiveSerializer.Deserialize<T>(memory.Span, options);
                }
                else
                {
                    var seq = builder.Build();
                    var result = ArchiveSerializer.Deserialize<T>(seq, options);
                    return result;
                }
            }
            finally
            {
                ReusableReadOnlySequenceBuilderPool.Return(builder);
            }
        }
    }
}
