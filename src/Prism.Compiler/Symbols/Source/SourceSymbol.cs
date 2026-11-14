// @file SourceSymbolData.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Compiler.Binding;
using Prism.Semantic;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;
using Prism.Syntax;
using Prism.Text;

namespace Prism.Compiler.Symbols.Source;

internal abstract class SourceSymbol(Compilation declaringCompilation)
    : ICompiledSymbolData,
        IDeclaredSymbolData,
        ILocatableSymbolData
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

    public abstract ImmutableArray<Location> Locations { get; }

    public abstract ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

    public bool IsImplicitlyDeclared => false;

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

    protected string ProduceMetadataName(SyntaxTree syntaxTree)
    {
        return
            this is not IVisibilityGuardedSymbolData { DeclaredVisibility: DeclaredVisibility.File }
            ? Name
            : $"{Name}_{syntaxTree.Md5Hash}";
    }

    protected void AddDeclarationDiagnostics(BindingContext diagnostics)
    {
        DeclaringCompilation.DeclarationDiagnostics.AddRange(diagnostics.AccumulatedDiagnostics);
    }

    protected static void ForceCompleteMemberConditionally(
        SourceLocation? location,
        Predicate<Symbol>? predicate,
        Symbol member,
        CancellationToken cancellationToken
    )
    {
        if (
            location is null
            || member.IsDefinedInSourceTree(location.SyntaxTree, location.SourceSpan)
                && predicate?.Invoke(member) != false
        )
        {
            member.ForceComplete(location, predicate, cancellationToken);
        }
    }
}
