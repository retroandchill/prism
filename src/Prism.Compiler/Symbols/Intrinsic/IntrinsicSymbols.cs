using Prism.Compiler.Semantic;
using Prism.Metadata;
using Prism.Semantic;

namespace Prism.Compiler.Symbols.Intrinsic;

internal static class IntrinsicSymbols
{
    public static readonly NamespaceSymbol GlobalNamespace;

    private static readonly NamedTypeSymbol Void;
    private static readonly NamedTypeSymbol Bool;
    private static readonly NamedTypeSymbol I8;
    private static readonly NamedTypeSymbol I16;
    private static readonly NamedTypeSymbol I32;
    private static readonly NamedTypeSymbol I64;
    private static readonly NamedTypeSymbol I128;
    private static readonly NamedTypeSymbol ISize;
    private static readonly NamedTypeSymbol U8;
    private static readonly NamedTypeSymbol U16;
    private static readonly NamedTypeSymbol U32;
    private static readonly NamedTypeSymbol U64;
    private static readonly NamedTypeSymbol U128;
    private static readonly NamedTypeSymbol USize;
    private static readonly NamedTypeSymbol F32;
    private static readonly NamedTypeSymbol F64;
    private static readonly NamedTypeSymbol Char;
    private static readonly NamedTypeSymbol Char16;
    private static readonly NamedTypeSymbol Rune;
    private static readonly NamedTypeSymbol Str;

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
        var globalNamespace = new IntrinsicNamespaceSymbol(CommonNames.Global, null!);
        GlobalNamespace = new NamespaceSymbol(globalNamespace);
        var stdNamespace = new IntrinsicNamespaceSymbol(CommonNames.Std, GlobalNamespace);
        var wrappedStdNamespace = new NamespaceSymbol(stdNamespace);
        globalNamespace.Members = [wrappedStdNamespace];

        Void = new NamedTypeSymbol(
            new IntrinsicPrimitiveSymbol(SpecialType.Void, wrappedStdNamespace)
        );
        Bool = new NamedTypeSymbol(
            new IntrinsicPrimitiveSymbol(SpecialType.Bool, wrappedStdNamespace)
        );
        I8 = new NamedTypeSymbol(new IntrinsicPrimitiveSymbol(SpecialType.I8, wrappedStdNamespace));
        I16 = new NamedTypeSymbol(
            new IntrinsicPrimitiveSymbol(SpecialType.I16, wrappedStdNamespace)
        );
        I32 = new NamedTypeSymbol(
            new IntrinsicPrimitiveSymbol(SpecialType.I32, wrappedStdNamespace)
        );
        I64 = new NamedTypeSymbol(
            new IntrinsicPrimitiveSymbol(SpecialType.I64, wrappedStdNamespace)
        );
        I128 = new NamedTypeSymbol(
            new IntrinsicPrimitiveSymbol(SpecialType.I128, wrappedStdNamespace)
        );
        ISize = new NamedTypeSymbol(
            new IntrinsicPrimitiveSymbol(SpecialType.ISize, wrappedStdNamespace)
        );
        U8 = new NamedTypeSymbol(new IntrinsicPrimitiveSymbol(SpecialType.U8, wrappedStdNamespace));
        U16 = new NamedTypeSymbol(
            new IntrinsicPrimitiveSymbol(SpecialType.U16, wrappedStdNamespace)
        );
        U32 = new NamedTypeSymbol(
            new IntrinsicPrimitiveSymbol(SpecialType.U32, wrappedStdNamespace)
        );
        U64 = new NamedTypeSymbol(
            new IntrinsicPrimitiveSymbol(SpecialType.U64, wrappedStdNamespace)
        );
        U128 = new NamedTypeSymbol(
            new IntrinsicPrimitiveSymbol(SpecialType.U128, wrappedStdNamespace)
        );
        USize = new NamedTypeSymbol(
            new IntrinsicPrimitiveSymbol(SpecialType.USize, wrappedStdNamespace)
        );
        F32 = new NamedTypeSymbol(
            new IntrinsicPrimitiveSymbol(SpecialType.F32, wrappedStdNamespace)
        );
        F64 = new NamedTypeSymbol(
            new IntrinsicPrimitiveSymbol(SpecialType.F64, wrappedStdNamespace)
        );
        Char = new NamedTypeSymbol(
            new IntrinsicPrimitiveSymbol(SpecialType.Char, wrappedStdNamespace)
        );
        Char16 = new NamedTypeSymbol(
            new IntrinsicPrimitiveSymbol(SpecialType.Char16, wrappedStdNamespace)
        );
        Rune = new NamedTypeSymbol(
            new IntrinsicPrimitiveSymbol(SpecialType.Rune, wrappedStdNamespace)
        );
        Str = new NamedTypeSymbol(
            new IntrinsicPrimitiveSymbol(SpecialType.Str, wrappedStdNamespace)
        );

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
