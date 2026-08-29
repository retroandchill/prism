namespace Prism.Core.Symbols;

public enum SpecialType : byte
{
    None,
    Void,
    Bool,
    I8,
    I16,
    I32,
    I64,
    I128,
    ISize,
    U8,
    U16,
    U32,
    U64,
    U128,
    USize,
    F32,
    F64,
    Char,
    Char16,
    Rune,
    Str,
}

public static class SpecialTypeExtensions
{
    extension(SpecialType type)
    {
        public bool IsInteger =>
            type
                is SpecialType.I8
                    or SpecialType.I16
                    or SpecialType.I32
                    or SpecialType.I64
                    or SpecialType.I128
                    or SpecialType.ISize
                    or SpecialType.U8
                    or SpecialType.U16
                    or SpecialType.U32
                    or SpecialType.U64
                    or SpecialType.U128
                    or SpecialType.USize;

        public bool IsSignedInteger =>
            type
                is SpecialType.I8
                    or SpecialType.I16
                    or SpecialType.I32
                    or SpecialType.I64
                    or SpecialType.I128
                    or SpecialType.ISize;

        public bool IsUnsignedInteger =>
            type
                is SpecialType.U8
                    or SpecialType.U16
                    or SpecialType.U32
                    or SpecialType.U64
                    or SpecialType.U128
                    or SpecialType.USize;

        public bool IsFloatingPoint => type is SpecialType.F32 or SpecialType.F64;

        public bool IsNumeric => type.IsInteger || type.IsFloatingPoint;

        public bool IsCharacter =>
            type is SpecialType.Char or SpecialType.Char16 or SpecialType.Rune;
    }
}
