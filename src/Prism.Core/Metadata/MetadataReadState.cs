// @file MetadataReadState.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using MagicArchive;
using MagicArchive.Formatters;

namespace Prism.Core.Metadata;

using ExtraMapping = ConcurrentDictionary<ArchiveReaderState, MetadataReadState>;

internal sealed class MetadataReadState : IDisposable
{
    internal static readonly ExtraMapping Mapping = new(ReferenceEqualityComparer.Instance);
    private readonly ConcurrentDictionary<int, string> _cachedStrings = new();
    private readonly ReadOnlySequence<byte> _stringTable;
    private readonly ArchiveReaderState _state;

    public MetadataReadState(ReadOnlySequence<byte> stringTable, ArchiveReaderState state)
    {
        _stringTable = stringTable;
        _state = state;
        Mapping.TryAdd(_state, this);
    }

    public string? GetString(int offset)
    {
        return offset is not -1
            ? _cachedStrings.GetOrAdd(offset, static (o, @this) => @this.ReadStringValue(o), this)
            : null;
    }

    private string ReadStringValue(int offset)
    {
        var reader = new ArchiveReader(_stringTable, _state);
        reader.Advance(offset);
        string? stringValue = null;
        Utf8StringFormatter.Default.Deserialize(ref reader, ref stringValue);
        Debug.Assert(stringValue is not null);
        return stringValue;
    }

    public void Dispose()
    {
        Mapping.TryRemove(_state, out _);
    }
}

internal static class MetadataReadStateExtensions
{
    extension(in ArchiveReader reader)
    {
        public MetadataReadState? MetadataState =>
            MetadataReadState.Mapping.GetValueOrDefault(reader.State);
    }
}
