using NetEscapades.EnumGenerators;

namespace Prism.Semantic.Conversions;

[EnumExtensions]
internal enum ConversionKind : byte
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

internal static partial class ConversionKindExtensions
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
