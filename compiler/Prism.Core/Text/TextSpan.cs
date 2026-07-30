// @file TextSpan.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using Prism.Core.Utils;

namespace Prism.Core.Text;

public readonly struct TextSpan
    : IEquatable<TextSpan>,
        IComparable<TextSpan>,
        IEqualityOperators<TextSpan, TextSpan, bool>,
        ISpanFormattable
{
    public int Start { get; }
    public int Length { get; }

    public int End => Start + Length;

    public bool IsEmpty => Length == 0;

    public TextSpan(int start, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (length > int.MaxValue - start)
        {
            throw new ArgumentOutOfRangeException(
                nameof(length),
                "Length is too long relative to the start value"
            );
        }

        Start = start;
        Length = length;
    }

    public override bool Equals(object? obj)
    {
        return obj is TextSpan other && Equals(other);
    }

    public bool Equals(TextSpan other)
    {
        return Start == other.Start && Length == other.Length;
    }

    public int CompareTo(TextSpan other)
    {
        return Start != other.Start ? Start.CompareTo(other.Start) : Length.CompareTo(other.Length);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, Length);
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
        return ToString(null, null);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return SpanFormatHelpers.ToString(this, format, formatProvider);
    }

    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider
    )
    {
        if (!IsDefaultFormat(format))
            throw new FormatException(
                $"Unsupported {nameof(TextSpan)} format '{format.ToString()}'."
            );

        return destination.TryWrite(provider, $"[{Start}..{End})", out charsWritten);
    }

    private static bool IsDefaultFormat(ReadOnlySpan<char> format)
    {
        return format.Length == 0 || (format.Length == 1 && (format[0] == 'G' || format[0] == 'g'));
    }
}
