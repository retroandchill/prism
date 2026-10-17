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

public sealed class MetadataHeader
{
    public const uint HeaderSequence = 0x50524953; // PRIS

    public const ushort Version = 1;

    public required TargetArchitecture TargetArchitecture { get; init; }
    public required TargetOperatingSystem OperatingSystem { get; init; }
    public required ToolchainKind Toolchain { get; init; }

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

        return new MetadataHeader
        {
            TargetArchitecture = ReadChecked<TargetArchitecture>(reader),
            OperatingSystem = ReadChecked<TargetOperatingSystem>(reader),
            Toolchain = ReadChecked<ToolchainKind>(reader),
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
