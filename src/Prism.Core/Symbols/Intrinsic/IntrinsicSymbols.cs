using Prism.Core.Utils;

namespace Prism.Core.Symbols.Intrinsic;

internal static class IntrinsicSymbols
{
    public static readonly IntrinsicNamespaceSymbol GlobalNamespace;

    private static readonly IntrinsicNamedTypeSymbol Void;
    private static readonly IntrinsicNamedTypeSymbol Bool;
    private static readonly IntrinsicNamedTypeSymbol I8;
    private static readonly IntrinsicNamedTypeSymbol I16;
    private static readonly IntrinsicNamedTypeSymbol I32;
    private static readonly IntrinsicNamedTypeSymbol I64;
    private static readonly IntrinsicNamedTypeSymbol I128;
    private static readonly IntrinsicNamedTypeSymbol ISize;
    private static readonly IntrinsicNamedTypeSymbol U8;
    private static readonly IntrinsicNamedTypeSymbol U16;
    private static readonly IntrinsicNamedTypeSymbol U32;
    private static readonly IntrinsicNamedTypeSymbol U64;
    private static readonly IntrinsicNamedTypeSymbol U128;
    private static readonly IntrinsicNamedTypeSymbol USize;
    private static readonly IntrinsicNamedTypeSymbol F32;
    private static readonly IntrinsicNamedTypeSymbol F64;
    private static readonly IntrinsicNamedTypeSymbol Char;
    private static readonly IntrinsicNamedTypeSymbol Char16;
    private static readonly IntrinsicNamedTypeSymbol Rune;
    private static readonly IntrinsicNamedTypeSymbol Str;

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

        Void = new IntrinsicNamedTypeSymbol(SpecialType.Void, GlobalNamespace);
        Bool = new IntrinsicNamedTypeSymbol(SpecialType.Bool, GlobalNamespace);
        I8 = new IntrinsicNamedTypeSymbol(SpecialType.I8, GlobalNamespace);
        I16 = new IntrinsicNamedTypeSymbol(SpecialType.I16, GlobalNamespace);
        I32 = new IntrinsicNamedTypeSymbol(SpecialType.I32, GlobalNamespace);
        I64 = new IntrinsicNamedTypeSymbol(SpecialType.I64, GlobalNamespace);
        I128 = new IntrinsicNamedTypeSymbol(SpecialType.I128, GlobalNamespace);
        ISize = new IntrinsicNamedTypeSymbol(SpecialType.ISize, GlobalNamespace);
        U8 = new IntrinsicNamedTypeSymbol(SpecialType.U8, GlobalNamespace);
        U16 = new IntrinsicNamedTypeSymbol(SpecialType.U16, GlobalNamespace);
        U32 = new IntrinsicNamedTypeSymbol(SpecialType.U32, GlobalNamespace);
        U64 = new IntrinsicNamedTypeSymbol(SpecialType.U64, GlobalNamespace);
        U128 = new IntrinsicNamedTypeSymbol(SpecialType.U128, GlobalNamespace);
        USize = new IntrinsicNamedTypeSymbol(SpecialType.USize, GlobalNamespace);
        F32 = new IntrinsicNamedTypeSymbol(SpecialType.F32, GlobalNamespace);
        F64 = new IntrinsicNamedTypeSymbol(SpecialType.F64, GlobalNamespace);
        Char = new IntrinsicNamedTypeSymbol(SpecialType.Char, GlobalNamespace);
        Char16 = new IntrinsicNamedTypeSymbol(SpecialType.Char16, GlobalNamespace);
        Rune = new IntrinsicNamedTypeSymbol(SpecialType.Rune, GlobalNamespace);
        Str = new IntrinsicNamedTypeSymbol(SpecialType.Str, GlobalNamespace);

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
