using Prism.Core.Utils;

namespace Prism.Core.Symbols;

internal static class SpecialTypeFacts
{
    public static SpecialType FromMetadataName(
        string metadataName,
        NamespaceSymbol? containingSymbol
    )
    {
        if (
            containingSymbol
            is not {
                DeclaringCompilation.Settings.BuildingCoreLibrary: true,
                Name: CommonNames.Std,
                ContainingNamespace.IsGlobal: true
            }
        )
            return SpecialType.None;

        return metadataName switch
        {
            CommonNames.Void => SpecialType.Void,
            CommonNames.Bool => SpecialType.Bool,
            CommonNames.Int8 => SpecialType.I8,
            CommonNames.Int16 => SpecialType.I16,
            CommonNames.Int32 => SpecialType.I32,
            CommonNames.Int64 => SpecialType.I64,
            CommonNames.Int128 => SpecialType.I128,
            CommonNames.ISize => SpecialType.ISize,
            CommonNames.UInt8 => SpecialType.U8,
            CommonNames.UInt16 => SpecialType.U16,
            CommonNames.UInt32 => SpecialType.U32,
            CommonNames.UInt64 => SpecialType.U64,
            CommonNames.UInt128 => SpecialType.U128,
            CommonNames.USize => SpecialType.USize,
            CommonNames.Float32 => SpecialType.F32,
            CommonNames.Float64 => SpecialType.F64,
            CommonNames.Char => SpecialType.Char,
            CommonNames.Char16 => SpecialType.Char16,
            CommonNames.Rune => SpecialType.Rune,
            CommonNames.Str => SpecialType.Str,
            _ => SpecialType.None,
        };
    }
}
