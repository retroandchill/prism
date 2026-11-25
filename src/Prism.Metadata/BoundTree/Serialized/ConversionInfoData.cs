// @file ConversionMetadata.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using MagicArchive;
using Riok.Mapperly.Abstractions;

namespace Prism.Metadata.BoundTree.Serialized;

[Archivable(GenerateType.Custom)]
internal readonly partial struct ConversionInfoData
{
    private readonly ConversionKind _kind;

    private readonly SpecialData? _additionalData;

    [MapperConstructor]
    internal ConversionInfoData(ConversionKind kind, SpecialData? additionalData = null)
    {
        _kind = kind;
        _additionalData = additionalData;
    }

    internal closed record SpecialData;

    internal sealed record NestedData(ImmutableArray<ConversionInfoData> NestedConversions)
        : SpecialData;

    static void IArchivable<ConversionInfoData>.Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ConversionInfoData value
    )
    {
        writer.WriteBlittable(value._kind);
        switch (value._additionalData)
        {
            case null:
                break;
            case NestedData(var nestedConversions):
                writer.WriteValue(nestedConversions);
                break;
        }
    }

    static void IArchivable<ConversionInfoData>.Deserialize(
        ref ArchiveReader reader,
        scoped ref ConversionInfoData value
    )
    {
        var kind = reader.ReadBlittable<ConversionKind>();
        var specialData = kind switch
        {
            ConversionKind.ImplicitNullable or ConversionKind.UnwrapNullable => new NestedData(
                reader.ReadValue<ImmutableArray<ConversionInfoData>>()
            ),
            _ => null,
        };

        value = new ConversionInfoData(kind, specialData);
    }
}
