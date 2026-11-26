using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Semantic;
using Prism.Semantic.Symbols;
using Prism.Syntax;

namespace Prism.Compiler.Symbols;

internal static class SymbolHelpers
{
    public static void CacheSymbol(this Compilation compilation, SyntaxNode node, Symbol symbol)
    {
        var semanticModel = compilation.GetSemanticModel(node.SyntaxTree);
        semanticModel.State.CacheSymbol(symbol, node);
    }

    public static SyntaxNode GetNamespaceDeclarationSyntax(SyntaxReference reference)
    {
        var node = reference.Syntax;
        while (node is NameSyntax)
        {
            node = node.Parent;
        }

        Debug.Assert(node is CompilationUnitSyntax or NamespaceDeclarationSyntax);
        return node;
    }

    extension(Symbol symbol)
    {
        internal LexicalSortKey GetLexicalSortKey()
        {
            var firstLocation = symbol.TryGetFirstLocation();
            if (firstLocation is null)
                return LexicalSortKey.NotInSource;

            var declaringCompilation = symbol.DeclaringCompilation;
            Debug.Assert(declaringCompilation is not null);
            return new LexicalSortKey((SourceLocation)firstLocation, declaringCompilation);
        }

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences =>
            symbol.Data is ICompiledSymbolData compiledSymbol
                ? compiledSymbol.DeclaringSyntaxReferences
                : [];

        internal Compilation? DeclaringCompilation =>
            symbol.Data is ICompiledSymbolData compiledSymbol
                ? compiledSymbol.DeclaringCompilation
                : null;
    }

    extension(NamespaceSymbol ns)
    {
        public Compilation? ContainingCompilation =>
            ns.Data is MergedNamespaceSymbol mergedNamespace
                ? mergedNamespace.ContainingCompilation
                : null;
    }

    extension(SpecialType type)
    {
        public string BuiltInTypeName
        {
            get
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
                    SpecialType.Ordering => CommonNames.Ordering,
                    SpecialType.None => throw new InvalidOperationException("Invalid special type"),
                    _ => throw new InvalidOperationException("Invalid special type"),
                };
            }
        }
    }
}
