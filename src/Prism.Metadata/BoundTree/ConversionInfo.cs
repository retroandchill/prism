// @file ConversionMetadata.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Metadata.BoundTree;

public readonly struct ConversionInfo
{
    internal ConversionKind Kind { get; }

    internal SpecialData? AdditionalData { get; }

    internal ConversionInfo(ConversionKind kind, SpecialData? additionalData = null)
    {
        Kind = kind;
        AdditionalData = additionalData;
    }

    public static ConversionInfo None => new(ConversionKind.NoConversion);
    public static ConversionInfo Identity => new(ConversionKind.Identity);
    public static ConversionInfo ImplicitNumeric => new(ConversionKind.ImplicitNumeric);
    public static ConversionInfo ImplicitCharacter => new(ConversionKind.ImplicitCharacter);
    public static ConversionInfo ImplicitReference => new(ConversionKind.ImplicitReference);
    public static ConversionInfo ImplicitSpan => new(ConversionKind.ImplicitSpan);

    public static ConversionInfo ImplicitNullable(
        params ImmutableArray<ConversionInfo> nestedConversions
    )
    {
        return new ConversionInfo(
            ConversionKind.ImplicitNullable,
            !nestedConversions.IsEmpty ? new NestedData(nestedConversions) : null
        );
    }

    public static ConversionInfo ExplicitNumeric => new(ConversionKind.ExplicitNumeric);
    public static ConversionInfo ExplicitCharacter => new(ConversionKind.ExplicitCharacter);
    public static ConversionInfo ExplicitReference => new(ConversionKind.ExplicitReference);

    public static ConversionInfo UnwrapNullable(
        params ImmutableArray<ConversionInfo> nestedConversions
    )
    {
        return new ConversionInfo(
            ConversionKind.UnwrapNullable,
            !nestedConversions.IsEmpty ? new NestedData(nestedConversions) : null
        );
    }

    public static ConversionInfo NullToNullable => new(ConversionKind.NullToNullable);

    internal closed record SpecialData;

    internal sealed record NestedData(ImmutableArray<ConversionInfo> NestedConversions)
        : SpecialData;
}
