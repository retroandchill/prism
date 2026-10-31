// @file TableRowReader.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;

namespace Prism.Core.Metadata;

internal sealed class TableRowReader<TRow>
    where TRow : unmanaged
{
    private readonly TRow[] _rows;

    public TableRowReader(ReadOnlyMemory<byte> buffer, ArchiveReaderState state, int count)
    {
        var reader = new ArchiveReader(buffer.Span, state);
        _rows = reader.ReadMetadataRows<TRow>(count);
    }

    public int Count => _rows.Length;

    public ref readonly TRow GetRow(int rowId)
    {
        return ref _rows[rowId - 1];
    }

    public ReadOnlySpan<TRow> AsSpan() => _rows;
}
