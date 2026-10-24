// @file BackPatchWritter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;

namespace Prism.Core.Metadata;

internal struct BackPatchWriter(Memory<byte> buffer) : IBufferWriter<byte>
{
    private int _position;

    private int Remaining => buffer.Length - _position;

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        return Remaining - sizeHint > 0
            ? buffer.Span[_position..]
            : throw new InvalidOperationException("Not enough space remaining in buffer");
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        return Remaining - sizeHint > 0
            ? buffer[_position..]
            : throw new InvalidOperationException("Not enough space remaining in buffer");
    }

    public void Advance(int count)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            _position + count,
            buffer.Length,
            nameof(count)
        );
        _position += count;
    }
}
