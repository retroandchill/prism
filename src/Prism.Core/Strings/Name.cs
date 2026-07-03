// @file Name.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Prism.Core.Strings;

public enum FindName : byte
{
    Find,
    Add,
}

public readonly struct Name
    : IEquatable<Name>,
        IEquatable<ReadOnlySpan<char>>,
        IEquatable<string>,
        IComparable<Name>
{
    public const int MaxLength = 256;
    internal const int MaxBlockBits = 13;
    internal const int BlockOffsetBits = 16;
    internal const int MaxBlocks = 1 << MaxBlockBits;
    internal const int BlockOffsets = 1 << BlockOffsetBits;

    public NameEntryId Id { get; }

    public bool IsNone => Id.IsNone;

    public static int Count => NamePool.Instance.NumEntries;

    public Name(KnownName name)
    {
        Id = NameEntryId.FromKnownName(name);
    }

    public Name(ReadOnlySpan<char> name, FindName findType = FindName.Add)
    {
        Id = LookupName(name, findType);
    }

    public static void Reserve(int numBlocks, int numNames)
    {
        NamePool.Instance.Reserve(numBlocks, numNames);
    }

    public static string GetStringValue(KnownName name)
    {
        var pool = NamePool.Instance;
        return pool.Resolve(pool.Find(name));
    }

    public static string GetStringValue(NameEntryId id)
    {
        return NamePool.Instance.Resolve(id);
    }

    private static NameEntryId LookupName(ReadOnlySpan<char> name, FindName findType)
    {
        if (name.Length > MaxLength)
        {
            throw new ArgumentException("Name too long");
        }

        var pool = NamePool.Instance;
        if (findType == FindName.Add)
        {
            return pool.Store(name);
        }

        Debug.Assert(findType == FindName.Find);
        return pool.Find(name);
    }

    public KnownName? ToKnownName()
    {
        return NamePool.Instance.FindKnown(Id);
    }

    public static implicit operator Name(ReadOnlySpan<char> name) => new(name);

    public static implicit operator Name(string name) => new(name);

    public override bool Equals(object? obj)
    {
        return obj is Name other && Equals(other);
    }

    public bool Equals(Name other)
    {
        throw new NotImplementedException();
    }

    public bool Equals(ReadOnlySpan<char> other)
    {
        throw new NotImplementedException();
    }

    public bool Equals(string? other)
    {
        throw new NotImplementedException();
    }

    public int CompareLexical(Name other, StringComparison comparison)
    {
        return Id.CompareLexical(other.Id, comparison);
    }

    public int CompareTo(Name other)
    {
        return Id.CompareTo(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString() => GetStringValue(Id);
}
