// @file MetadataBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;
using MagicArchive.Formatters;
using MagicArchive.Utilities;

namespace Prism.Core.Metadata;

internal sealed class MetadataBuilder : IDisposable
{
    private static readonly ArchiveWriterState WriterState = ArchiveWriterStatePool.Rent(
        ArchiveSerializerOptions.LittleEndian
    );

    private bool _disposed;
    private readonly Dictionary<string, int> _stringTableMapping = new();
    private ReusableLinkedArrayBufferWriter _stringTable;

    public MetadataBuilder(AssemblyIdentity assemblyIdentity, TargetInfo targetInfo)
    {
        _stringTable = ReusableLinkedArrayBufferWriterPool.Rent();
    }

    public StringHandle AddString(string str)
    {
        if (_stringTableMapping.TryGetValue(str, out var index))
        {
            return new StringHandle(index);
        }

        var writer = ArchiveWriter.Create(ref _stringTable, WriterState);
        index = _stringTableMapping.Count;
        _stringTableMapping.Add(str, index);
        writer.WriteMetadataString(str);
        return new StringHandle(index);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        ReusableLinkedArrayBufferWriterPool.Return(_stringTable);
    }
}
