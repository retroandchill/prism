// @file MetadataHeader.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NetEscapades.EnumGenerators;
using Prism.Core.Configuration;

namespace Prism.Core.Metadata;

[EnumExtensions]
public enum MetadataArchitecture : byte
{
    X86 = 0x10,
    X64 = 0x20,
    Arm = 0x30,
    Arm64 = 0x40,
}

[EnumExtensions]
public enum MetadataOperatingSystem : byte
{
    Linux = 0x10,
    Windows = 0x20,
    MacOS = 0x30,
    Freestanding = 0x40,
}

[EnumExtensions]
public enum MetadataToolchain : byte
{
    Msvc = 0x10,
    Clang = 0x20,
    Gnu = 0x30,
}

public sealed class MetadataHeader
{
    public const uint HeaderSequence = 0x50524953; // PRIS

    public const ushort Version = 1;

    public required MetadataArchitecture TargetArchitecture { get; init; }
    public required MetadataOperatingSystem OperatingSystem { get; init; }
    public required MetadataToolchain Toolchain { get; init; }

    public void Serialize(Stream stream)
    {
        using var writer = new BinaryWriter(stream);
        writer.Write(HeaderSequence);
        writer.Write(Version);
        writer.Write(TargetArchitecture.AsUnderlyingType());
        writer.Write(OperatingSystem.AsUnderlyingType());
        writer.Write(Toolchain.AsUnderlyingType());
    }

    public static MetadataHeader Deserialize(Stream stream)
    {
        using var reader = new BinaryReader(stream);

        var header = reader.ReadUInt32();
        if (header != HeaderSequence)
        {
            throw new InvalidDataException("Invalid header sequence");
        }

        var version = reader.ReadUInt16();
        if (version != Version)
        {
            throw new InvalidDataException("Invalid version");
        }

        // TODO: We need to validate that we're reading valid enum data out
        return new MetadataHeader
        {
            TargetArchitecture = ReadChecked<MetadataArchitecture>(reader),
            OperatingSystem = ReadChecked<MetadataOperatingSystem>(reader),
            Toolchain = ReadChecked<MetadataToolchain>(reader),
        };

        static TEnum ReadChecked<TEnum>(BinaryReader reader)
            where TEnum : unmanaged, Enum
        {
            Debug.Assert(Enum.GetUnderlyingType(typeof(TEnum)) == typeof(byte));
            var value = Unsafe.BitCast<byte, TEnum>(reader.ReadByte());
            return Enum.IsDefined(value)
                ? value
                : throw new InvalidDataException($"Invalid enum value: {value}");
        }
    }
}
