// @file TextSpan.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Prism.Core.Source;

public readonly struct TextSpan(string source, int position, int length)
    : IEquatable<TextSpan>,
        IEqualityOperators<TextSpan, TextSpan, bool>
{
    public string Source { get; } = source;
    public int Position { get; } = position;
    public int Length { get; private init; } = length;

    public TextSpan(string source)
        : this(source, 0, source.Length) { }

    public static TextSpan Empty => new(string.Empty);

    public bool AtEnd => Position + Length == Source.Length;

    public char this[int index] =>
        index < Length
            ? Source[Position + index]
            : throw new ArgumentOutOfRangeException(
                nameof(index),
                "Index is greater than the length of the span"
            );

    public override bool Equals(object? obj) => obj is TextSpan other && Equals(other);

    public bool Equals(TextSpan other)
    {
        return ReferenceEquals(Source, other.Source)
            && Position == other.Position
            && Length == other.Length;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RuntimeHelpers.GetHashCode(Source), Position, Length);
    }

    public static bool operator ==(TextSpan left, TextSpan right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TextSpan left, TextSpan right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return Source is not null ? ToStringValue() : "(<none>)";
    }

    public string ToStringValue()
    {
        return Source.Substring(Position, Length);
    }

    public ReadOnlySpan<char> ToReadOnlySpan()
    {
        return Source.AsSpan(Position, Length);
    }

    public static implicit operator ReadOnlySpan<char>(TextSpan span)
    {
        return span.ToReadOnlySpan();
    }

    public TextSpan First(int length)
    {
        if (length > Length)
            throw new ArgumentOutOfRangeException(
                nameof(length),
                "Length is greater than the length of the span"
            );

        return this with
        {
            Length = length,
        };
    }

    public TextSpan Slice(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        if (index >= Length)
            throw new ArgumentOutOfRangeException(
                nameof(index),
                "Index is greater than the length of the span"
            );

        return new TextSpan(Source, Position + index, Length - index);
    }

    public TextSpan Slice(int index, int count)
    {
        return Slice(index).First(count);
    }
}
