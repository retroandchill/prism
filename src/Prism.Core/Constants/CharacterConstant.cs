// @file CharacterConstant.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using MagicArchive;

namespace Prism.Core.Constants;

public enum CharacterKind
{
    C8,
    C16,
    C32,
}

[Archivable(GenerateType.Custom)]
[Union]
public readonly partial record struct CharacterConstant : IUnion
{
    internal CharacterKind Kind { get; }
    internal Rune RawValue { get; }

    private CharacterConstant(CharacterKind kind, Rune value)
    {
        Kind = kind;
        RawValue = value;
    }

    internal static CharacterConstant From(CharacterKind kind, Rune value) => new(kind, value);

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
            return Kind switch
            {
                CharacterKind.C8 => (byte)RawValue.Value,
                CharacterKind.C16 => (char)RawValue.Value,
                CharacterKind.C32 => RawValue,
                _ => throw new InvalidOperationException("Invalid character kind"),
            };
        }
    }

    public bool TryGetValue(out byte value)
    {
        if (Kind == CharacterKind.C8)
        {
            value = (byte)RawValue.Value;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out char value)
    {
        if (Kind == CharacterKind.C16)
        {
            value = (char)RawValue.Value;
            return true;
        }

        value = '\0';
        return false;
    }

    public bool TryGetValue(out Rune value)
    {
        if (Kind == CharacterKind.C32)
        {
            value = RawValue;
            return true;
        }

        value = default;
        return false;
    }

    public bool EquivalentTo(CharacterConstant other)
    {
        return RawValue == other.RawValue;
    }

    public bool NotEquivalentTo(CharacterConstant other)
    {
        return !EquivalentTo(other);
    }

    public CharacterConstant ConvertTo(CharacterKind kind)
    {
        return kind switch
        {
            CharacterKind.C8 => (byte)RawValue.Value,
            CharacterKind.C16 => (char)RawValue.Value,
            CharacterKind.C32 => RawValue,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    static void IArchivable<CharacterConstant>.Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in CharacterConstant value
    ) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SerializeWithHeader<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        CharacterKind kind,
        Rune value
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        SerializeWithHeader(ref writer, (byte)kind, kind, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SerializeWithHeader<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        byte header,
        CharacterKind kind,
        Rune value
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        switch (kind)
        {
            case CharacterKind.C8:
                writer.WriteBlittable(header, (byte)value.Value);
                break;
            case CharacterKind.C16:
                writer.WriteBlittable(header, (char)value.Value);
                break;
            case CharacterKind.C32:
                writer.WriteBlittable(header, value);
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
        value = new CharacterConstant(kind, ReadValue(ref reader, kind));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Rune ReadValue(ref ArchiveReader reader, CharacterKind kind)
    {
        return kind switch
        {
            CharacterKind.C8 => new Rune(reader.ReadBlittable<byte>()),
            CharacterKind.C16 => new Rune(reader.ReadBlittable<char>()),
            CharacterKind.C32 => reader.ReadBlittable<Rune>(),
            _ => throw new ArchiveSerializationException("Invalid character kind"),
        };
    }

    public override string ToString()
    {
        return RawValue.ToString();
    }
}
