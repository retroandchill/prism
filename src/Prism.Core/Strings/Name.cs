// @file Symbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Prism.Core.Strings;

public readonly struct Name
    : IEquatable<Name>,
        IEquatable<ReadOnlySpan<char>>,
        IEquatable<string>,
        IComparable<Name>,
        IEqualityOperators<Name, string, bool>,
        IComparisonOperators<Name, Name, bool>
{
    private const int IdNone = 0;

    public NameEntryId Id { get; }

    public bool IsNone => Id.IsNone;

    public static Name None => default;

    private Name(NameEntryId id)
    {
        Id = id;
    }

    public Name(ReadOnlySpan<char> text, FindName find = FindName.Add)
        : this(LookupName(text, find)) { }

    private static NameEntryId LookupName(ReadOnlySpan<char> text, FindName find)
    {
        if (text.IsEmpty)
        {
            return NameEntryId.None;
        }

        return find == FindName.Add ? NamePool.Instance.Store(text) : NamePool.Instance.Find(text);
    }

    public override bool Equals(object? obj)
    {
        return obj is Name other && Equals(other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Name other)
    {
        return Id == other.Id;
    }

    public bool Equals(ReadOnlySpan<char> other)
    {
        return Equals(other, StringComparison.Ordinal);
    }

    public bool Equals(ReadOnlySpan<char> other, StringComparison comparison)
    {
        return ToString().Equals(other, comparison);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(string? other)
    {
        return Equals(other.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(string? other, StringComparison comparison)
    {
        return Equals(other.AsSpan(), comparison);
    }

    public int CompareTo(Name other)
    {
        return Id.CompareTo(other.Id);
    }

    public int LexicalCompareTo(Name other, StringComparison comparison = StringComparison.Ordinal)
    {
        return Id.CompareLexical(other.Id, comparison);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return Id.ToUnstableInt() != IdNone ? NamePool.Instance.Resolve(Id) : string.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Name left, Name right)
    {
        return left.Equals(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Name left, Name right)
    {
        return !(left == right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Name left, ReadOnlySpan<char> right)
    {
        return left.Equals(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Name left, ReadOnlySpan<char> right)
    {
        return !(left == right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Name left, string? right)
    {
        return left.Equals(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Name left, string? right)
    {
        return !(left == right);
    }

    public static bool operator >(Name left, Name right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(Name left, Name right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static bool operator <(Name left, Name right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(Name left, Name right)
    {
        return left.CompareTo(right) <= 0;
    }
}
