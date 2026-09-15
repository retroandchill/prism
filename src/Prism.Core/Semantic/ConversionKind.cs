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
    ExplicitNullable,
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
                    or ConversionKind.ImplicitNullable;

        public bool IsTrivial =>
            kind
                is ConversionKind.Identity
                    or ConversionKind.ImplicitNumeric
                    or ConversionKind.ImplicitCharacter
                    or ConversionKind.ImplicitReference
                    or ConversionKind.ImplicitSpan
                    or ConversionKind.ImplicitNullable
                    or ConversionKind.ExplicitCharacter
                    or ConversionKind.ExplicitNumeric
                    or ConversionKind.ExplicitReference
                    or ConversionKind.ExplicitNullable;
    }
}
