using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Semantic;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Intrinsic;

internal sealed class IntrinsicPrimitiveSymbol : NamedTypeSymbol
{
    internal IntrinsicPrimitiveSymbol(SpecialType type, NamespaceSymbol containing)
    {
        ContainingNamespace = containing;
        SpecialType = type;
    }

    private static string GetBuiltInTypeName(SpecialType type)
    {
        return type switch
        {
            SpecialType.Void => CommonNames.Void,
            SpecialType.Bool => CommonNames.Bool,
            SpecialType.I8 => CommonNames.Int8,
            SpecialType.I16 => CommonNames.Int16,
            SpecialType.I32 => CommonNames.Int32,
            SpecialType.I64 => CommonNames.Int64,
            SpecialType.I128 => CommonNames.Int128,
            SpecialType.ISize => CommonNames.ISize,
            SpecialType.U8 => CommonNames.UInt8,
            SpecialType.U16 => CommonNames.UInt16,
            SpecialType.U32 => CommonNames.UInt32,
            SpecialType.U64 => CommonNames.UInt64,
            SpecialType.U128 => CommonNames.UInt128,
            SpecialType.USize => CommonNames.USize,
            SpecialType.F32 => CommonNames.Float32,
            SpecialType.F64 => CommonNames.Float64,
            SpecialType.Char => CommonNames.Char,
            SpecialType.Char16 => CommonNames.Char16,
            SpecialType.Rune => CommonNames.Rune,
            SpecialType.Str => CommonNames.Str,
            SpecialType.None => throw new InvalidOperationException("Invalid special type"),
            _ => throw new InvalidOperationException("Invalid special type"),
        };
    }

    public override string Name => GetBuiltInTypeName(SpecialType);
    public override Symbol ContainingSymbol => ContainingNamespace;
    public override NamedTypeKind Kind => NamedTypeKind.Class;
    public override SpecialType SpecialType { get; }

    public override NamespaceSymbol ContainingNamespace { get; }
    public override MemberContainerSymbol ContainingMemberContainer => ContainingNamespace;

    public override ImmutableArray<Location> Locations => [];
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override ImmutableArray<Symbol> GetMembers() => [];

    public override ImmutableArray<Symbol> GetMembers(string name) => [];

    public override bool IsDynamicallySized => SpecialType == SpecialType.Str;
    public override bool IsDefaultConstructible => SpecialType != SpecialType.Str;
}
