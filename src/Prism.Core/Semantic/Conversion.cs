using System.Collections.Immutable;
using System.Diagnostics;

namespace Prism.Core.Semantic;

public readonly struct Conversion
{
    private readonly ConversionKind _kind;
    private readonly SpecialData? _specialData;

    private Conversion(ConversionKind kind, SpecialData? specialData = null)
    {
        _kind = kind;
        _specialData = specialData;
    }

    public static Conversion None => new(ConversionKind.NoConversion);
    internal static Conversion Identity => new(ConversionKind.Identity);
    internal static Conversion ImplicitNumeric => new(ConversionKind.ImplicitNumeric);
    internal static Conversion ImplicitCharacter => new(ConversionKind.ImplicitCharacter);
    internal static Conversion ImplicitReference => new(ConversionKind.ImplicitReference);
    internal static Conversion ImplicitSpan => new(ConversionKind.ImplicitSpan);

    internal static Conversion ImplicitNullable(params ImmutableArray<Conversion> nestedConversions)
    {
        return new Conversion(
            ConversionKind.ImplicitNullable,
            !nestedConversions.IsEmpty ? new NestedData(nestedConversions) : null
        );
    }

    internal static Conversion ExplicitNumeric => new(ConversionKind.ExplicitNumeric);
    internal static Conversion ExplicitCharacter => new(ConversionKind.ExplicitCharacter);
    internal static Conversion ExplicitReference => new(ConversionKind.ExplicitReference);

    internal static Conversion ExplicitNullable(params ImmutableArray<Conversion> nestedConversions)
    {
        return new Conversion(
            ConversionKind.ExplicitNullable,
            !nestedConversions.IsEmpty ? new NestedData(nestedConversions) : null
        );
    }

    public bool Exists => _kind != ConversionKind.NoConversion;

    public bool IsIdentity => _kind == ConversionKind.Identity;

    public bool IsImplicit => _kind.IsImplicit;

    public bool IsExplicit => Exists && !_kind.IsImplicit;

    public bool IsNumeric =>
        _kind is ConversionKind.ImplicitNumeric or ConversionKind.ExplicitNumeric;

    public bool IsCharacter =>
        _kind is ConversionKind.ImplicitCharacter or ConversionKind.ExplicitCharacter;

    public bool IsReference =>
        _kind is ConversionKind.ImplicitReference or ConversionKind.ExplicitReference;

    public bool IsSpan => _kind is ConversionKind.ImplicitSpan;

    public bool IsNullable =>
        _kind is ConversionKind.ImplicitNullable or ConversionKind.ExplicitNullable;

    public ImmutableArray<Conversion> UnderlyingConversions =>
        _specialData is NestedData nestedData ? nestedData.NestedConversions : [];

    private abstract record SpecialData;

    private sealed record NestedData(ImmutableArray<Conversion> NestedConversions) : SpecialData;
}
