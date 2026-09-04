using NetEscapades.EnumGenerators;

namespace Prism.Core.Semantic;

[EnumExtensions]
public enum ConversionKind : byte
{
    NoConversion,
    Identity,
    ImplicitNumeric,
    ImplicitCharacter,

    ExplicitNumeric,
    ExplicitCharacter,

    AddReference,
    RemoveReference,
}

public static partial class ConversionKindExtensions
{
    extension(ConversionKind kind)
    {
        public bool IsImplicit =>
            kind
                is ConversionKind.ImplicitNumeric
                    or ConversionKind.ImplicitCharacter
                    or ConversionKind.Identity;

        public bool IsTrivial =>
            kind
                is ConversionKind.Identity
                    or ConversionKind.ImplicitNumeric
                    or ConversionKind.ImplicitCharacter
                    or ConversionKind.ExplicitCharacter
                    or ConversionKind.ExplicitNumeric;
    }
}
