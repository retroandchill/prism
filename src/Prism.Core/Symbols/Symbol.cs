using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Prism.Core.Binding;
using Prism.Core.Compiling;
using Prism.Diagnostics;
using Prism.Metadata;
using Prism.Syntax;
using Prism.Text;

namespace Prism.Core.Symbols;

public closed class Symbol : IEquatable<Symbol>
{
    public abstract string Name { get; }

    public virtual string MetadataName => Name;

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

    internal LexicalSortKey GetLexicalSortKey()
    {
        var firstLocation = TryGetFirstLocation();
        if (firstLocation is null)
            return LexicalSortKey.NotInSource;

        var declaringCompilation = DeclaringCompilation;
        Debug.Assert(declaringCompilation is not null);
        return new LexicalSortKey((SourceLocation)firstLocation, declaringCompilation);
    }

    public virtual bool IsImplicitlyDeclared => false;

    public abstract Symbol? ContainingSymbol { get; }

    public virtual AssemblySymbol? ContainingAssembly => ContainingSymbol?.ContainingAssembly;

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

    public virtual MemberContainerSymbol? ContainingMemberContainer
    {
        get
        {
            var containing = ContainingSymbol;
            var container = containing as TypeSymbol;
            return ReferenceEquals(containing, container)
                ? container
                : containing?.ContainingMemberContainer;
        }
    }

    public virtual TypeSymbol? ContainingType
    {
        get
        {
            var containing = ContainingSymbol;
            var containingType = containing as TypeSymbol;
            return ReferenceEquals(containing, containingType)
                ? containingType
                : containing?.ContainingType;
        }
    }

    public abstract ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

    public virtual ImmutableArray<AttributeData> GetAttributes()
    {
        return [];
    }

    public virtual bool IsDefinedInSourceTree(ISourceDocument tree, TextSpan? definedWithin)
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
        ISourceDocument tree,
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

    public override string ToString() => ToDisplayString();

    internal virtual Compilation? DeclaringCompilation => ContainingAssembly?.DeclaringCompilation;

    private protected void AddDeclarationDiagnostics(BindingContext diagnostics)
    {
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        compilation.DeclarationDiagnostics.AddRange(diagnostics.AccumulatedDiagnostics);
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

    private protected string ProduceMetadataName(SyntaxTree syntaxTree)
    {
        return DeclaredVisibility != DeclaredVisibility.File
            ? Name
            : $"{Name}_{syntaxTree.Md5Hash}";
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Symbol);
    }

    public bool Equals(Symbol? other)
    {
        return other is not null && Equals(other, TypeCompareKind.ConsiderEverything);
    }

    public virtual bool Equals(Symbol other, TypeCompareKind kind)
    {
        return ReferenceEquals(this, other);
    }

    public override int GetHashCode()
    {
        return GetHashCode(TypeCompareKind.ConsiderEverything);
    }

    public virtual int GetHashCode(TypeCompareKind kind)
    {
        return RuntimeHelpers.GetHashCode(this);
    }

    public static bool operator ==(Symbol? left, Symbol? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        return left is not null && left.Equals(right);
    }

    public static bool operator !=(Symbol? left, Symbol? right)
    {
        return !(left == right);
    }
}
