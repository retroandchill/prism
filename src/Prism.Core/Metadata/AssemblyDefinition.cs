// @file AssemblyDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Nerdbank.Streams;
using Prism.Core.Configuration;

namespace Prism.Core.Metadata;

public sealed class AssemblyDefinition : AssemblyDescriptor, IDisposable
{
    public const uint HeaderSequence = 0x50524953; // PRIS

    public const ushort SchemaVersion = 1;

    public TargetInfo Target { get; }

    private readonly MetadataReader? _reader;
    private readonly Lock _readerLock = new();

    public ImmutableArray<AssemblyReference> ReferencedAssemblies
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Read(ref field, reader => reader.ReadReferencedAssemblies());
            return field;
        }
        private init;
    }

    internal AssemblyDefinition(AssemblyDefinitionBuilder builder)
        : base(builder.Identity, new MetadataToken(TokenType.Assembly, 1))
    {
        Target = builder.Target;
        ReferencedAssemblies = [.. builder.ReferencedAssemblies];
    }

    internal AssemblyDefinition(AssemblyIdentity identity, TargetInfo info, MetadataReader reader)
        : base(identity, new MetadataToken(TokenType.Assembly, 1))
    {
        Target = info;
        _reader = reader;
    }

    internal void Read<T>(
        ref ImmutableArray<T> value,
        Func<MetadataReader, ImmutableArray<T>> factory
    )
    {
        Debug.Assert(_reader is not null);
        using var scope = _readerLock.EnterScope();
        using var readerScope = _reader.EnterScope();
        value = factory(_reader);
    }

    public static AssemblyDefinition Load(Stream stream)
    {
        using var reader = new BinaryReader(stream);

        var header = reader.ReadUInt32();
        if (header != HeaderSequence)
        {
            throw new InvalidDataException("Invalid header sequence");
        }

        var schemaVersion = reader.ReadUInt16();
        if (schemaVersion != SchemaVersion)
        {
            throw new InvalidDataException("Invalid version");
        }

        var targetArchitecture = ReadChecked<TargetArchitecture>(reader);
        var operatingSystem = ReadChecked<TargetOperatingSystem>(reader);
        var toolchain = ReadChecked<ToolchainKind>(reader);

        var length = reader.ReadInt32();
        var utf8 = reader.ReadBytes(length);
        var name = Encoding.UTF8.GetString(utf8);
        var version = new Version(
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32()
        );

        var identity = new AssemblyIdentity(name, version);
        var target = new TargetInfo(targetArchitecture, operatingSystem, toolchain);

        var sequence = new Sequence<byte>();
        stream.CopyTo(sequence.AsStream());

        return new AssemblyDefinition(
            identity,
            target,
            new MetadataReader(sequence.AsReadOnlySequence)
        );

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

    public void Dispose()
    {
        _reader?.Dispose();
    }
}
