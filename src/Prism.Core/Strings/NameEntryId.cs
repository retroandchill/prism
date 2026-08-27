// @file NameEntryId.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Prism.Core.Strings;

public readonly struct NameEntryId
    : IEquatable<NameEntryId>,
        IComparable<NameEntryId>,
        IEqualityOperators<NameEntryId, NameEntryId, bool>
{
    private readonly uint _value;

    public bool IsNone => _value == 0;

    private NameEntryId(uint value)
    {
        _value = value;
    }

    public uint ToUnstableInt()
    {
        return _value;
    }

    public static NameEntryId FromUnstableInt(uint value)
    {
        return new NameEntryId(value);
    }

    public static NameEntryId FromKnownName(KnownName name)
    {
        return name == KnownName.None ? default : FromValidName(name);
    }

    private static NameEntryId FromValidName(KnownName name)
    {
        return NamePool.Instance.Find(name);
    }

    public override bool Equals(object? obj)
    {
        return obj is NameEntryId other && Equals(other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(NameEntryId other)
    {
        return _value == other._value;
    }

    public override int GetHashCode()
    {
        return new NameEntryHandle(this).GetHashCode();
    }

    public int CompareLexical(
        NameEntryId id,
        StringComparison comparison = StringComparison.Ordinal
    )
    {
        if (_value == id._value)
            return 0;

        var pool = NamePool.Instance;
        var left = pool.Resolve(this);
        var right = pool.Resolve(id);
        return left.CompareTo(right, comparison);
    }

    public int CompareTo(NameEntryId other)
    {
        return _value.CompareTo(other._value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(NameEntryId left, NameEntryId right)
    {
        return left.Equals(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(NameEntryId left, NameEntryId right)
    {
        return !(left == right);
    }
}
