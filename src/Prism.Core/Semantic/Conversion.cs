using System.Diagnostics;

namespace Prism.Core.Semantic;

public readonly struct Conversion
{
    private readonly ConversionKind _kind;

    private Conversion(ConversionKind kind)
    {
        _kind = kind;
    }

    public static Conversion None => new(ConversionKind.NoConversion);

    internal static Conversion GetTrivia(ConversionKind kind)
    {
        Debug.Assert(kind.IsTrivial);
        return new Conversion(kind);
    }

    public bool Exists => _kind != ConversionKind.NoConversion;

    public bool IsIdentity => _kind == ConversionKind.Identity;

    public bool IsImplicit => _kind.IsImplicit;

    public bool IsExplicit => Exists && !_kind.IsImplicit;

    public bool IsNumeric =>
        _kind is ConversionKind.ImplicitNumeric or ConversionKind.ExplicitNumeric;

    public bool IsCharacter =>
        _kind is ConversionKind.ImplicitCharacter or ConversionKind.ExplicitCharacter;
}
