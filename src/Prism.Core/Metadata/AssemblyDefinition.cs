// @file AssemblyDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MagicArchive;
using MagicArchive.Utilities;

namespace Prism.Core.Metadata;

public sealed class AssemblyDefinition : AssemblyDescriptor, IDisposable
{
    public TargetInfo Target { get; }

    private readonly MetadataReader? _reader;
    private readonly Lock _readerLock = new();

    internal AssemblyDefinition(AssemblyDefinitionBuilder builder)
        : base(builder.Identity, new MetadataToken(TokenType.Assembly, 1))
    {
        Target = builder.Target;
        ReferencedAssemblies = [.. builder.ReferencedAssemblies];
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

            Read(ref field, reader => reader.ReadReferencedAssemblies());
            return field;
        }
        private init;
    }

    internal void Read<T>(
        ref ImmutableArray<T> value,
        Func<MetadataReader, ImmutableArray<T>> factory
    )
    {
        Debug.Assert(_reader is not null);
        using var scope = _readerLock.EnterScope();
        using var readerScope = _reader.EnterScope();
        value = factory(_reader);
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
            return builder.TryGetSingleMemory(out var memory)
                ? Load(memory)
                : Load(builder.Build());
        }
        finally
        {
            ReusableReadOnlySequenceBuilderPool.Return(builder);
        }
    }

    public static AssemblyDefinition Load(ReadOnlyMemory<byte> memory)
    {
        using var state = ArchiveReaderStatePool.Rent(ArchiveSerializerOptions.LittleEndian);
        var reader = new ArchiveReader(memory.Span, state);

        var (identity, targetInfo) = reader.ReadArchivable<MetadataHeader>();
        return new AssemblyDefinition(
            identity,
            targetInfo,
            new MetadataReader(new ReadOnlySequence<byte>(memory[reader.ConsumedBytes..]))
        );
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
    }

    private void WriteHeader<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteArchivable(new MetadataHeader(Identity, Target));
    }

    public void Dispose()
    {
        _reader?.Dispose();
    }
}
