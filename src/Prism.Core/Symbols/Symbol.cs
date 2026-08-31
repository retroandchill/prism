using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Compiling;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using Prism.Core.Text;

namespace Prism.Core.Symbols;

public abstract class Symbol
{
    private protected Symbol(string name, Symbol? containingSymbol = null)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }

    public string Name { get; }

    // TODO: For now symbol visibility does not apply but eventually it will
    public virtual DeclaredVisibility DeclaredVisibility => DeclaredVisibility.NotApplicable;

    public abstract ImmutableArray<Location> Locations { get; }

    public Location FirstLocation()
    {
        return TryGetFirstLocation()
            ?? throw new InvalidOperationException("Symbol has no locations");
    }

    public Location? TryGetFirstLocation()
    {
        return Locations.Length > 0 ? Locations[0] : null;
    }

    public Location FirstLocationOrNone()
    {
        return TryGetFirstLocation() ?? Location.None;
    }

    public virtual bool IsImplicitlyDeclared => false;

    public Symbol? ContainingSymbol { get; }

    public virtual AssemblySymbol? ContainingAssembly => ContainingSymbol?.ContainingAssembly;

    public virtual NamespaceSymbol? ContainingNamespace
    {
        get
        {
            for (
                var container = ContainingSymbol;
                container is not null;
                container = container.ContainingSymbol
            )
            {
                if (container is NamespaceSymbol namespaceSymbol)
                {
                    return namespaceSymbol;
                }
            }

            return null;
        }
    }

    public virtual TypeSymbol? ContainingType
    {
        get
        {
            var containingType = ContainingSymbol as TypeSymbol;
            if (ReferenceEquals(ContainingSymbol, containingType))
            {
                return containingType;
            }

            return ContainingSymbol?.ContainingType;
        }
    }

    public abstract ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

    public virtual bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithin)
    {
        if (IsImplicitlyDeclared && DeclaringSyntaxReferences.IsEmpty)
        {
            Debug.Assert(ContainingSymbol is not null);
            return ContainingSymbol.IsDefinedInSourceTree(tree, definedWithin);
        }

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var reference in DeclaringSyntaxReferences)
        {
            if (IsDefinedInSourceTree(reference, tree, definedWithin))
                return true;
        }

        return false;
    }

    internal static bool IsDefinedInSourceTree(
        SyntaxReference reference,
        SyntaxTree tree,
        TextSpan? definedWithin
    )
    {
        return ReferenceEquals(reference.SyntaxTree, tree)
            && (definedWithin is null || reference.SourceSpan.IntersectsWith(definedWithin.Value));
    }

    public abstract void WriteDisplayString(TextWriter writer);

    public string ToDisplayString()
    {
        var writer = new StringWriter();
        WriteDisplayString(writer);
        return writer.ToString();
    }

    internal virtual Compilation? DeclaringCompilation => ContainingAssembly?.DeclaringCompilation;

    private protected void AddDeclarationDiagnostics(DiagnosticBag diagnostics)
    {
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        compilation.DeclarationDiagnostics.AddRange(diagnostics);
    }

    internal virtual bool NeedsCompletion => false;

    internal virtual void ForceComplete(
        SourceLocation? location,
        Predicate<Symbol>? filter,
        CancellationToken cancellationToken
    )
    {
        Debug.Assert(!NeedsCompletion, $"Need to override {nameof(ForceComplete)}");
    }

    internal virtual bool IsComplete(CompletionPart part)
    {
        Debug.Assert(!NeedsCompletion, $"Need to override {nameof(IsComplete)}");
        return true;
    }

    private protected static void ForceCompleteMemberConditionally(
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
