// @file MetadataToken.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using MagicArchive;

namespace Prism.Core.Metadata;

[Archivable]
public readonly partial record struct MetadataToken
    : IEqualityOperators<MetadataToken, MetadataToken, bool>
{
    [ArchiveInclude]
    private readonly uint _value;
    private const uint IdMask = 0x00ffffff;
    private const uint TypeMask = 0xff000000;

    [ArchiveIgnore]
    public uint Id
    {
        get => _value & IdMask;
        init
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, IdMask);
            _value = (uint)Type | value;
        }
    }

    [ArchiveIgnore]
    public TokenType Type
    {
        get => (TokenType)(_value & TypeMask);
        init => _value = (uint)value | Id;
    }

    public static readonly MetadataToken None = new(0U);

    [ArchiveIgnore]
    public bool IsNone => _value == 0U;

    public MetadataToken(uint value)
    {
        _value = value;
    }

    public MetadataToken(TokenType type)
    {
        _value = (uint)type;
    }

    public MetadataToken(TokenType type, uint id)
    {
        ArgumentOutOfRangeException.ThrowIfZero(id);
        _value = (uint)type | id;
    }

    internal static bool IsValidId(int id)
    {
        return id <= IdMask;
    }

    public uint ToUInt32() => _value;

    public bool Equals(MetadataToken other)
    {
        return _value == other._value;
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return $"[{Type}:0x{Id:X4}]";
    }
}
