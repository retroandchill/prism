// @file SourcePosition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using Prism.Core.Utils;

namespace Prism.Core.Text;

public readonly struct SourcePosition
    : IEquatable<SourcePosition>,
        IComparable<SourcePosition>,
        IEqualityOperators<SourcePosition, SourcePosition, bool>,
        IComparisonOperators<SourcePosition, SourcePosition, bool>,
        ISpanFormattable
{
    public int Line { get; }

    public int Column { get; }

    public bool IsNone => Line == 0 && Column == 0;

    public static SourcePosition None => default;

    public SourcePosition(int line, int column)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(line);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(column);

        Line = line;
        Column = column;
    }

    public override bool Equals(object? obj)
    {
        return obj is SourcePosition other && Equals(other);
    }

    public bool Equals(SourcePosition other)
    {
        return Line == other.Line && Column == other.Column;
    }

    public int CompareTo(SourcePosition other)
    {
        return Line != other.Line ? Line.CompareTo(other.Line) : Column.CompareTo(other.Column);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Line, Column);
    }

    public static bool operator ==(SourcePosition left, SourcePosition right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SourcePosition left, SourcePosition right)
    {
        return !left.Equals(right);
    }

    public static bool operator >(SourcePosition left, SourcePosition right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(SourcePosition left, SourcePosition right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static bool operator <(SourcePosition left, SourcePosition right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(SourcePosition left, SourcePosition right)
    {
        return left.CompareTo(right) <= 0;
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
                $"Unsupported {nameof(SourcePosition)} format '{format.ToString()}'."
            );

        return destination.TryWrite(provider, $"{Line}:{Column}", out charsWritten);
    }

    private static bool IsDefaultFormat(ReadOnlySpan<char> format)
    {
        return format.Length == 0 || (format.Length == 1 && (format[0] == 'G' || format[0] == 'g'));
    }
}
