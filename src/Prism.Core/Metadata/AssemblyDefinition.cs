// @file AssemblyDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using MagicArchive;
using MagicArchive.Utilities;
using NetEscapades.EnumGenerators;
using ZLinq;

namespace Prism.Core.Metadata;

public sealed class AssemblyDefinition : AssemblyDescriptor, IDisposable
{
    public TargetInfo Target { get; }

    private readonly MetadataReader? _reader;

    internal AssemblyDefinition(AssemblyDefinitionBuilder builder)
        : base(builder.Identity, new MetadataToken(TokenType.Assembly, 1))
    {
        Target = builder.Target;
        ReferencedAssemblies = [.. builder.ReferencedAssemblies.AsValueEnumerable().Distinct()];
    }

    internal AssemblyDefinition(AssemblyIdentity identity, TargetInfo info, MetadataReader reader)
        : base(identity, new MetadataToken(TokenType.Assembly, 1))
    {
        Target = info;
        _reader = reader;
    }

    public ImmutableArray<AssemblyReference> ReferencedAssemblies
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                _reader?.ReadReferencedAssemblies() ?? []
            );
            return field;
        }
        private init;
    }

    public static async ValueTask<AssemblyDefinition> LoadAsync(
        Stream stream,
        CancellationToken cancellationToken = default
    )
    {
        var builder = ReusableReadOnlySequenceBuilderPool.Rent();
        try
        {
            await builder.ReadFromStreamAsync(stream, cancellationToken);
            return Load(builder.Build());
        }
        finally
        {
            ReusableReadOnlySequenceBuilderPool.Return(builder);
        }
    }

    public static AssemblyDefinition Load(ReadOnlySequence<byte> sequence)
    {
        using var state = ArchiveReaderStatePool.Rent(ArchiveSerializerOptions.LittleEndian);
        var reader = new ArchiveReader(sequence, state);

        var (identity, targetInfo) = reader.ReadArchivable<MetadataHeader>();
        return new AssemblyDefinition(
            identity,
            targetInfo,
            new MetadataReader(sequence.Slice(reader.ConsumedBytes))
        );
    }

    public async ValueTask WriteAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var tempWriter = ReusableLinkedArrayBufferWriterPool.Rent();
        try
        {
            Write(tempWriter);
            await tempWriter.WriteToAndResetAsync(stream, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        finally
        {
            ReusableLinkedArrayBufferWriterPool.Return(tempWriter);
        }
    }

    public void Write<TBufferWriter>(in TBufferWriter bufferWriter)
        where TBufferWriter : IBufferWriter<byte>
    {
        ref var bufferWriterRef = ref Unsafe.AsRef(in bufferWriter);
        using var state = ArchiveWriterStatePool.Rent(ArchiveSerializerOptions.LittleEndian);
        var writer = ArchiveWriter.Create(ref bufferWriterRef, state);
        WriteHeader(ref writer);
        WriteSections(ref writer);
        writer.Flush();
    }

    private void WriteHeader<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteArchivable(new MetadataHeader(Identity, Target));
    }

    private void WriteSections<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        Span<int> sections = stackalloc int[Sections.Length];
        for (byte i = 0; i < Sections.Length; i++)
        {
            sections[i] = -1;
        }

        var tempBuffer = ReusableLinkedArrayBufferWriterPool.Rent();

        try
        {
            var tempWriter = ArchiveWriter.Create(ref tempBuffer, writer.State);
            using var metadataContext = new MetadataWriteState(writer.State);

            WriteAssemblyReferences(ref tempWriter);
            sections[(int)Section.AssemblyReferences] = tempWriter.WrittenBytes;

            // The heaps must always be written last
            WriteStringTable(ref tempWriter, metadataContext);
            sections[(int)Section.StringTable] = tempWriter.WrittenBytes;

            tempWriter.Flush();

            writer.WriteSpan(sections);
            tempBuffer.WriteToAndReset(ref writer);
        }
        finally
        {
            ReusableLinkedArrayBufferWriterPool.Return(tempBuffer);
        }
    }

    private static void WriteStringTable<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        MetadataWriteState writeState
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        writeState.WriteToTable(ref writer);
    }

    private void WriteAssemblyReferences<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteValue(ReferencedAssemblies);
    }

    public void Dispose()
    {
        _reader?.Dispose();
    }
}
