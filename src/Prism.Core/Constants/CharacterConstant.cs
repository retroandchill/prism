// @file CharacterConstant.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using MagicArchive;

namespace Prism.Core.Constants;

internal enum CharacterKind
{
    C8,
    C16,
    C32,
}

[Archivable(GenerateType.Custom)]
[Union]
public readonly partial struct CharacterConstant : IUnion
{
    private readonly CharacterKind _kind;
    private readonly Rune _value;

    private CharacterConstant(CharacterKind kind, Rune value)
    {
        _kind = kind;
        _value = value;
    }

    public CharacterConstant(byte value)
        : this(CharacterKind.C8, new Rune(value)) { }

    public CharacterConstant(char value)
        : this(CharacterKind.C16, new Rune(value)) { }

    public CharacterConstant(Rune value)
        : this(CharacterKind.C32, value) { }

    public object Value
    {
        get
        {
            return _kind switch
            {
                CharacterKind.C8 => (byte)_value.Value,
                CharacterKind.C16 => (char)_value.Value,
                CharacterKind.C32 => _value,
                _ => throw new InvalidOperationException("Invalid character kind"),
            };
        }
    }

    public bool TryGetValue(out byte value)
    {
        if (_kind == CharacterKind.C8)
        {
            value = (byte)_value.Value;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out char value)
    {
        if (_kind == CharacterKind.C16)
        {
            value = (char)_value.Value;
            return true;
        }

        value = '\0';
        return false;
    }

    public bool TryGetValue(out Rune value)
    {
        if (_kind == CharacterKind.C32)
        {
            value = _value;
            return true;
        }

        value = default;
        return false;
    }

    static void IArchivable<CharacterConstant>.Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in CharacterConstant value
    )
    {
        switch (value._kind)
        {
            case CharacterKind.C8:
                writer.WriteBlittable(value._kind, (byte)value._value.Value);
                break;
            case CharacterKind.C16:
                writer.WriteBlittable(value._kind, (char)value._value.Value);
                break;
            case CharacterKind.C32:
                writer.WriteBlittable(value._kind, value._value);
                break;
            default:
                throw new ArchiveSerializationException("Invalid character kind");
        }
    }

    static void IArchivable<CharacterConstant>.Deserialize(
        ref ArchiveReader reader,
        scoped ref CharacterConstant value
    )
    {
        var kind = reader.ReadBlittable<CharacterKind>();
        value = kind switch
        {
            CharacterKind.C8 => reader.ReadBlittable<byte>(),
            CharacterKind.C16 => reader.ReadBlittable<char>(),
            CharacterKind.C32 => reader.ReadBlittable<Rune>(),
            _ => throw new ArchiveSerializationException("Invalid character kind"),
        };
    }
}
