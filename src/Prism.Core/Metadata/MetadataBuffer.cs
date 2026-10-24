// @file MetadataBuffer.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;

namespace Prism.Core.Metadata;

internal struct MetadataBuffer() : IBufferWriter<byte>
{
    private const int InitialCapacity = 1024 * 4;

    private byte[] _buffer = new byte[InitialCapacity];

    public int Position { get; private set; }

    private int Capacity => _buffer.Length;

    public ReadOnlySpan<byte> WrittenSpan => _buffer.AsSpan(0, Position);

    public ReadOnlyMemory<byte> WrittenMemory => _buffer.AsMemory(0, Position);

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        Reserve(Position + sizeHint);
        return _buffer.AsSpan(Position);
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        Reserve(Position + sizeHint);
        return _buffer.AsMemory(Position);
    }

    public void Advance(int size)
    {
        Position += size;
    }

    private void Reserve(int sizeHint)
    {
        if (Position + sizeHint > Capacity)
        {
            Grow();
        }
    }

    private void Grow() => Grow(Capacity * 2);

    private void Grow(int requiredCapacity)
    {
        var newCapacity = Math.Max(InitialCapacity, requiredCapacity);
        var newBuffer = new byte[newCapacity];
        WrittenSpan.CopyTo(newBuffer);
        _buffer = newBuffer;
    }

    public void CopyTo(IBufferWriter<byte> writer)
    {
        var span = writer.GetSpan(Position);
        WrittenSpan.CopyTo(span);
        writer.Advance(Position);
    }

    public async ValueTask CopyToAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        await stream.WriteAsync(WrittenMemory, cancellationToken);
    }
}
