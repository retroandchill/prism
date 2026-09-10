using System.Collections.Immutable;
using System.Text;
using Prism.Core.Configuration;
using Prism.Core.Diagnostics;
using Prism.Core.Semantic;
using Prism.Core.Syntax;
using Prism.Core.Utils;

namespace Prism.Core.Symbols.Intrinsic;

internal sealed class IntrinsicNamedTypeSymbol : NamedTypeSymbol
{
    internal IntrinsicNamedTypeSymbol(SpecialType type, NamespaceSymbol containing)
        : base(GetBuiltInTypeName(type), containing, type) { }

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

    public override ImmutableArray<Location> Locations => [];
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override ImmutableArray<Symbol> GetMembers() => [];

    public override ImmutableArray<Symbol> GetMembers(string name) => [];

    public override bool IsDynamicallySized => SpecialType == SpecialType.Str;

    public override ulong GetSizeInBytes(CompilationSettings settings)
    {
        if (IsDynamicallySized)
            throw new InvalidOperationException("Cannot get size of dynamically sized type");

        return SpecialType switch
        {
            SpecialType.Void => throw new InvalidOperationException("Void has no size"),
            SpecialType.Bool or SpecialType.I8 or SpecialType.U8 or SpecialType.Char => 1,
            SpecialType.I16 or SpecialType.U16 or SpecialType.Char16 => 2,
            SpecialType.I32 or SpecialType.U32 or SpecialType.F32 or SpecialType.Rune => 4,
            SpecialType.I64 or SpecialType.U64 or SpecialType.F64 => 8,
            SpecialType.I128 or SpecialType.U128 => 16,
            SpecialType.ISize or SpecialType.USize => unchecked(
                (ulong)settings.PointerWidth.BitWidth / 8
            ),
            _ => throw new ArgumentException("Unknown special type"),
        };
    }
}
