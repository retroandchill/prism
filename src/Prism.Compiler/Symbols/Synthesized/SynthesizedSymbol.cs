// @file SynthesizedSymbolBase.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Compiler.Symbols.Source;
using Prism.Semantic;
using Prism.Syntax;
using Prism.Text;

namespace Prism.Compiler.Symbols.Synthesized;

internal abstract class SynthesizedSymbol(Compilation declaringCompilation)
    : ICompiledSymbolData,
        IDeclaredSymbolData
{
    public Compilation DeclaringCompilation { get; } = declaringCompilation;

    public Symbol PublicSymbol { get; protected init; } = null!;

    public abstract string Name { get; }

    public virtual string MetadataName => Name;

    public abstract Symbol? ContainingSymbol { get; }

    public virtual AssemblySymbol? ContainingAssembly => DeclaringCompilation.Assembly;

    public virtual NamespaceSymbol? ContainingNamespace
    {
        get
        {
            var containing = ContainingSymbol;
            var container = containing as NamespaceSymbol;
            return ReferenceEquals(containing, container)
                ? container
                : containing?.ContainingNamespace;
        }
    }

    public virtual TypeSymbol? ContainingType
    {
        get
        {
            var containing = ContainingSymbol;
            var container = containing as TypeSymbol;
            return ReferenceEquals(containing, container) ? container : containing?.ContainingType;
        }
    }

    public virtual MemberContainerSymbol? ContainingMemberContainer
    {
        get
        {
            var containing = ContainingSymbol;
            var container = containing as MemberContainerSymbol;
            return ReferenceEquals(containing, container)
                ? container
                : containing?.ContainingMemberContainer;
        }
    }

    public virtual ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public bool IsImplicitlyDeclared => true;

    public virtual bool IsDefinedInSourceTree(ISourceDocument tree, TextSpan? definedWithin)
    {
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var reference in DeclaringSyntaxReferences)
        {
            if (IsDefinedInSourceTree(reference, tree, definedWithin))
                return true;
        }

        return false;
    }

    protected static bool IsDefinedInSourceTree(
        SyntaxReference reference,
        ISourceDocument tree,
        TextSpan? definedWithin
    )
    {
        return ReferenceEquals(reference.SyntaxTree, tree)
            && (definedWithin is null || reference.SourceSpan.IntersectsWith(definedWithin.Value));
    }
}
