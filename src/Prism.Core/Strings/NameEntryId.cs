// @file NameEntryId.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Prism.Core.Strings;

public readonly struct NameEntryId
    : IEquatable<NameEntryId>,
        IComparable<NameEntryId>,
        IComparisonOperators<NameEntryId, NameEntryId, bool>
{
    private readonly uint _value;

    public bool IsNone => _value == 0;

    public static NameEntryId None => default;

    private NameEntryId(uint value)
    {
        _value = value;
    }

    public uint ToUnstableInt() => _value;

    public static NameEntryId FromUnstableInt(uint value) => new(value);

    public override bool Equals(object? obj)
    {
        return obj is NameEntryId other && Equals(other);
    }

    public bool Equals(NameEntryId other)
    {
        return _value == other._value;
    }

    public override int GetHashCode()
    {
        return new NameEntryHandle(this).GetHashCode();
    }

    public int CompareTo(NameEntryId other)
    {
        return _value.CompareTo(other._value);
    }

    public int CompareLexical(
        NameEntryId other,
        StringComparison comparison = StringComparison.Ordinal
    )
    {
        var pool = NamePool.Instance;
        var name1 = pool.Resolve(this);
        var name2 = pool.Resolve(other);
        return name1.CompareTo(name2, comparison);
    }

    public static bool operator ==(NameEntryId left, NameEntryId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(NameEntryId left, NameEntryId right)
    {
        return !left.Equals(right);
    }

    public static bool operator >(NameEntryId left, NameEntryId right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(NameEntryId left, NameEntryId right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static bool operator <(NameEntryId left, NameEntryId right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(NameEntryId left, NameEntryId right)
    {
        return left.CompareTo(right) <= 0;
    }
}
