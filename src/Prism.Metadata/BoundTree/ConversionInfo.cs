// @file ConversionMetadata.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Riok.Mapperly.Abstractions;

namespace Prism.Metadata.BoundTree;

public enum ConversionInfoKind : byte
{
    NoConversion,
    Identity,

    ImplicitNumeric,
    ImplicitCharacter,
    ImplicitReference,
    ImplicitSpan,
    ImplicitNullable,

    ExplicitNumeric,
    ExplicitCharacter,
    ExplicitReference,

    UnwrapNullable,
    NullToNullable,
}

public readonly struct ConversionInfo
{
    public ConversionInfoKind Kind { get; }

    internal SpecialData? AdditionalData { get; }

    [MapperConstructor]
    internal ConversionInfo(ConversionInfoKind kind, SpecialData? additionalData = null)
    {
        Kind = kind;
        AdditionalData = additionalData;
    }

    public static ConversionInfo None => new(ConversionInfoKind.NoConversion);
    public static ConversionInfo Identity => new(ConversionInfoKind.Identity);
    public static ConversionInfo ImplicitNumeric => new(ConversionInfoKind.ImplicitNumeric);
    public static ConversionInfo ImplicitCharacter => new(ConversionInfoKind.ImplicitCharacter);
    public static ConversionInfo ImplicitReference => new(ConversionInfoKind.ImplicitReference);
    public static ConversionInfo ImplicitSpan => new(ConversionInfoKind.ImplicitSpan);

    public static ConversionInfo ImplicitNullable(
        params ImmutableArray<ConversionInfo> nestedConversions
    )
    {
        return new ConversionInfo(
            ConversionInfoKind.ImplicitNullable,
            !nestedConversions.IsEmpty ? new NestedData(nestedConversions) : null
        );
    }

    public static ConversionInfo ExplicitNumeric => new(ConversionInfoKind.ExplicitNumeric);
    public static ConversionInfo ExplicitCharacter => new(ConversionInfoKind.ExplicitCharacter);
    public static ConversionInfo ExplicitReference => new(ConversionInfoKind.ExplicitReference);

    public static ConversionInfo UnwrapNullable(
        params ImmutableArray<ConversionInfo> nestedConversions
    )
    {
        return new ConversionInfo(
            ConversionInfoKind.UnwrapNullable,
            !nestedConversions.IsEmpty ? new NestedData(nestedConversions) : null
        );
    }

    public static ConversionInfo NullToNullable => new(ConversionInfoKind.NullToNullable);

    public ImmutableArray<ConversionInfo> UnderlyingConversions =>
        AdditionalData is NestedData nestedData ? nestedData.NestedConversions : [];

    internal closed record SpecialData;

    internal sealed record NestedData(ImmutableArray<ConversionInfo> NestedConversions)
        : SpecialData;
}
