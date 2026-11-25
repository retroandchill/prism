using System.Collections.Immutable;
using System.Numerics;

namespace Prism.Semantic.Conversions;

public readonly struct Conversion
    : IEquatable<Conversion>,
        IEqualityOperators<Conversion, Conversion, bool>
{
    internal ConversionKind Kind { get; }
    private readonly SpecialData? _specialData;

    private Conversion(ConversionKind kind, SpecialData? specialData = null)
    {
        Kind = kind;
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

    internal static Conversion UnwrapNullable(params ImmutableArray<Conversion> nestedConversions)
    {
        return new Conversion(
            ConversionKind.UnwrapNullable,
            !nestedConversions.IsEmpty ? new NestedData(nestedConversions) : null
        );
    }

    internal static Conversion NullToNullable => new(ConversionKind.NullToNullable);

    public bool Exists => Kind != ConversionKind.NoConversion;

    public bool IsIdentity => Kind == ConversionKind.Identity;

    public bool IsImplicit => Kind.IsImplicit;

    public bool IsExplicit => Exists && !Kind.IsImplicit;

    public bool IsNumeric =>
        Kind is ConversionKind.ImplicitNumeric or ConversionKind.ExplicitNumeric;

    public bool IsCharacter =>
        Kind is ConversionKind.ImplicitCharacter or ConversionKind.ExplicitCharacter;

    public bool IsReference =>
        Kind is ConversionKind.ImplicitReference or ConversionKind.ExplicitReference;

    public bool IsSpan => Kind is ConversionKind.ImplicitSpan;

    public bool IsNullable =>
        Kind is ConversionKind.ImplicitNullable or ConversionKind.UnwrapNullable;

    public bool IsUnwrapping => Kind is ConversionKind.UnwrapNullable;

    public bool IsNullToNullable => Kind is ConversionKind.NullToNullable;

    public ImmutableArray<Conversion> UnderlyingConversions =>
        _specialData is NestedData nestedData ? nestedData.NestedConversions : [];

    private closed record SpecialData;

    private sealed record NestedData(ImmutableArray<Conversion> NestedConversions) : SpecialData
    {
        public bool Equals(NestedData? other)
        {
            return other is not null && NestedConversions.SequenceEqual(other.NestedConversions);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            foreach (var conversion in NestedConversions)
            {
                hashCode.Add(conversion);
            }
            return hashCode.ToHashCode();
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is Conversion other && Equals(other);
    }

    public bool Equals(Conversion other)
    {
        return Kind == other.Kind && Equals(_specialData, other._specialData);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Kind, _specialData);
    }

    public static bool operator ==(Conversion left, Conversion right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Conversion left, Conversion right)
    {
        return !left.Equals(right);
    }
}
