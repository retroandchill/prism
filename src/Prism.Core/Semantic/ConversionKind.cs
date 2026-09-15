using NetEscapades.EnumGenerators;

namespace Prism.Core.Semantic;

[EnumExtensions]
public enum ConversionKind : byte
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

public static partial class ConversionKindExtensions
{
    extension(ConversionKind kind)
    {
        public bool IsImplicit =>
            kind
                is ConversionKind.ImplicitNumeric
                    or ConversionKind.ImplicitCharacter
                    or ConversionKind.Identity
                    or ConversionKind.ImplicitReference
                    or ConversionKind.ImplicitSpan
                    or ConversionKind.ImplicitNullable
                    or ConversionKind.UnwrapNullable
                    or ConversionKind.NullToNullable;
    }
}
