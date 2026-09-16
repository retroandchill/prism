using Prism.Core.Utils;

namespace Prism.Core.Symbols.Intrinsic;

internal static class IntrinsicSymbols
{
    public static readonly IntrinsicNamespaceSymbol GlobalNamespace;

    private static readonly IntrinsicPrimitiveSymbol Void;
    private static readonly IntrinsicPrimitiveSymbol Bool;
    private static readonly IntrinsicPrimitiveSymbol I8;
    private static readonly IntrinsicPrimitiveSymbol I16;
    private static readonly IntrinsicPrimitiveSymbol I32;
    private static readonly IntrinsicPrimitiveSymbol I64;
    private static readonly IntrinsicPrimitiveSymbol I128;
    private static readonly IntrinsicPrimitiveSymbol ISize;
    private static readonly IntrinsicPrimitiveSymbol U8;
    private static readonly IntrinsicPrimitiveSymbol U16;
    private static readonly IntrinsicPrimitiveSymbol U32;
    private static readonly IntrinsicPrimitiveSymbol U64;
    private static readonly IntrinsicPrimitiveSymbol U128;
    private static readonly IntrinsicPrimitiveSymbol USize;
    private static readonly IntrinsicPrimitiveSymbol F32;
    private static readonly IntrinsicPrimitiveSymbol F64;
    private static readonly IntrinsicPrimitiveSymbol Char;
    private static readonly IntrinsicPrimitiveSymbol Char16;
    private static readonly IntrinsicPrimitiveSymbol Rune;
    private static readonly IntrinsicPrimitiveSymbol Str;

    public static NamedTypeSymbol GetType(SpecialType type)
    {
        return type switch
        {
            SpecialType.None => throw new InvalidOperationException("Not a special type"),
            SpecialType.Void => Void,
            SpecialType.Bool => Bool,
            SpecialType.I8 => I8,
            SpecialType.I16 => I16,
            SpecialType.I32 => I32,
            SpecialType.I64 => I64,
            SpecialType.I128 => I128,
            SpecialType.ISize => ISize,
            SpecialType.U8 => U8,
            SpecialType.U16 => U16,
            SpecialType.U32 => U32,
            SpecialType.U64 => U64,
            SpecialType.U128 => U128,
            SpecialType.USize => USize,
            SpecialType.F32 => F32,
            SpecialType.F64 => F64,
            SpecialType.Char => Char,
            SpecialType.Char16 => Char16,
            SpecialType.Rune => Rune,
            SpecialType.Str => Str,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    static IntrinsicSymbols()
    {
        GlobalNamespace = new IntrinsicNamespaceSymbol(CommonNames.Global, null);
        var stdNamespace = new IntrinsicNamespaceSymbol(CommonNames.Std, GlobalNamespace);
        GlobalNamespace.Members = [stdNamespace];

        Void = new IntrinsicPrimitiveSymbol(SpecialType.Void, GlobalNamespace);
        Bool = new IntrinsicPrimitiveSymbol(SpecialType.Bool, GlobalNamespace);
        I8 = new IntrinsicPrimitiveSymbol(SpecialType.I8, GlobalNamespace);
        I16 = new IntrinsicPrimitiveSymbol(SpecialType.I16, GlobalNamespace);
        I32 = new IntrinsicPrimitiveSymbol(SpecialType.I32, GlobalNamespace);
        I64 = new IntrinsicPrimitiveSymbol(SpecialType.I64, GlobalNamespace);
        I128 = new IntrinsicPrimitiveSymbol(SpecialType.I128, GlobalNamespace);
        ISize = new IntrinsicPrimitiveSymbol(SpecialType.ISize, GlobalNamespace);
        U8 = new IntrinsicPrimitiveSymbol(SpecialType.U8, GlobalNamespace);
        U16 = new IntrinsicPrimitiveSymbol(SpecialType.U16, GlobalNamespace);
        U32 = new IntrinsicPrimitiveSymbol(SpecialType.U32, GlobalNamespace);
        U64 = new IntrinsicPrimitiveSymbol(SpecialType.U64, GlobalNamespace);
        U128 = new IntrinsicPrimitiveSymbol(SpecialType.U128, GlobalNamespace);
        USize = new IntrinsicPrimitiveSymbol(SpecialType.USize, GlobalNamespace);
        F32 = new IntrinsicPrimitiveSymbol(SpecialType.F32, GlobalNamespace);
        F64 = new IntrinsicPrimitiveSymbol(SpecialType.F64, GlobalNamespace);
        Char = new IntrinsicPrimitiveSymbol(SpecialType.Char, GlobalNamespace);
        Char16 = new IntrinsicPrimitiveSymbol(SpecialType.Char16, GlobalNamespace);
        Rune = new IntrinsicPrimitiveSymbol(SpecialType.Rune, GlobalNamespace);
        Str = new IntrinsicPrimitiveSymbol(SpecialType.Str, GlobalNamespace);

        stdNamespace.Members =
        [
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
        ];
    }
}
