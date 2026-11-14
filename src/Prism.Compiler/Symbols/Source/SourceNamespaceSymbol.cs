using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Prism.Compiler.Binding;
using Prism.Compiler.Declarations;
using Prism.Compiler.Symbols.Synthesized;
using Prism.Diagnostics;
using Prism.Semantic;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;
using Prism.Syntax;
using Prism.Text;
using Prism.Utils;
using ZLinq;

namespace Prism.Compiler.Symbols.Source;

internal sealed class SourceNamespaceSymbol
    : SourceMemberContainerSymbol,
        INamespaceSymbolData,
        ICompletableSymbolData
{
    private readonly MergedNamespaceDeclaration _mergedDeclaration;
    private readonly Lock _memberChecksLock = new();

    public SourceNamespaceSymbol(
        Compilation compilation,
        MergedNamespaceDeclaration declaration,
        AssemblySymbol assembly,
        NamespaceSymbol? containingSymbol = null
    )
        : base(compilation)
    {
        _mergedDeclaration = declaration;
        ContainingAssembly = assembly;
        ContainingNamespace = containingSymbol;

        PublicSymbol = new NamespaceSymbol(this);
        foreach (var decl in _mergedDeclaration.Declarations)
        {
            DeclaringCompilation.CacheSymbol(decl.SyntaxReference.Syntax, PublicSymbol);
        }
    }

    public new NamespaceSymbol PublicSymbol
    {
        get => Unsafe.As<NamespaceSymbol>(base.PublicSymbol);
        private init => base.PublicSymbol = value;
    }

    public override string Name => _mergedDeclaration.Name;

    public override Symbol ContainingSymbol => (Symbol?)ContainingNamespace ?? ContainingAssembly;

    public override NamespaceSymbol? ContainingNamespace { get; }

    public override MemberContainerSymbol? ContainingMemberContainer => ContainingNamespace;

    public override ImmutableArray<Location> Locations => _mergedDeclaration.NameLocations;

    public override AssemblySymbol ContainingAssembly { get; }

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedCompareExchange(
                ref field,
                [.. _mergedDeclaration.Declarations.Select(d => d.SyntaxReference)],
                default
            );
            return field;
        }
    }

    public override bool IsDefinedInSourceTree(ISourceDocument tree, TextSpan? definedWithin)
    {
        if (PublicSymbol.IsGlobal)
            return true;

        foreach (
            var reference in _mergedDeclaration
                .Declarations.AsValueEnumerable()
                .Select(declaration => declaration.SyntaxReference)
                .Where(reference => ReferenceEquals(reference.SyntaxTree, tree))
        )
        {
            if (definedWithin is null)
                return true;

            var syntax = SymbolHelpers.GetNamespaceDeclarationSyntax(reference);
            if (syntax.FullSpan.IntersectsWith(definedWithin.Value))
                return true;
        }

        return false;
    }

    public override void ForceComplete(
        SourceLocation? location,
        Predicate<Symbol>? filter,
        CancellationToken cancellationToken
    )
    {
        if (filter?.Invoke(PublicSymbol) == false)
            return;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var incompletePart = CompletionState.NextIncompletePart;
            switch (incompletePart)
            {
                case CompletionPart.Members:
                    _ = GetNameToMembersMap();
                    break;
                case CompletionPart.MembersCompleted:
                {
                    var allCompleted = true;

                    foreach (var member in GetMembers())
                    {
                        ForceCompleteMemberConditionally(
                            location,
                            filter,
                            member,
                            cancellationToken
                        );
                        allCompleted &= member.IsComplete(CompletionPart.Members);
                    }

                    if (allCompleted)
                    {
                        CompletionState.MarkPartComplete(CompletionPart.MembersCompleted);
                        break;
                    }
                    var allParts =
                        location is null && filter is null
                            ? CompletionPart.NamespaceAll
                            : CompletionPart.NamespaceAll & ~CompletionPart.MembersCompleted;
                    CompletionState.MarkPartComplete(allParts);
                    return;
                }
                case CompletionPart.StartChecks or CompletionPart.FinishChecks:
                    LazyMemberChecks();
                    break;
                case CompletionPart.None:
                    return;
                default:
                    // Any other values are for other kinds of symbols
                    CompletionState.MarkPartComplete(
                        CompletionPart.All & ~CompletionPart.NamespaceAll
                    );
                    break;
            }

            CompletionState.WaitPartComplete(incompletePart, cancellationToken);
        }
    }

    private void LazyMemberChecks()
    {
        if (CompletionState.IsComplete(CompletionPart.FinishChecks))
            return;

        using var scope = _memberChecksLock.EnterScope();
        if (!CompletionState.MarkPartComplete(CompletionPart.StartChecks))
            return;

        using var context = BindingContext.Create();
        try
        {
            foreach (var (name, members) in GetNameToMembersMap())
            {
                ValidateMembers(name, members, context);
            }
            AddDeclarationDiagnostics(context);
        }
        finally
        {
            CompletionState.MarkPartComplete(CompletionPart.FinishChecks);
        }
    }

    protected override ImmutableDictionary<string, ImmutableArray<Symbol>> MakeNameToMembersMap(
        BindingContext context
    )
    {
        var result = new Dictionary<string, ImmutableArray<Symbol>.Builder>();

        foreach (var symbol in _mergedDeclaration.Members.Select(BuildSymbol))
        {
            result.GetOrAdd(symbol.Name, ImmutableArray.CreateBuilder<Symbol>).Add(symbol);
        }

        foreach (
            var syntax in _mergedDeclaration
                .Declarations.AsValueEnumerable()
                .SelectMany(x => GetSyntaxMembers(x).AsValueEnumerable())
        )
        {
            Symbol? symbol = syntax switch
            {
                NamespaceDeclarationSyntax or TypeDeclarationSyntax => null,
                GlobalVariableDeclarationSyntax variable => BuildSymbol(variable),
                FunctionDeclarationSyntax function => BuildSymbol(function),
                _ => null,
            };
            if (symbol is null)
                continue;

            result.GetOrAdd(symbol.Name, ImmutableArray.CreateBuilder<Symbol>).Add(symbol);
        }

        AddSynthesizedMembers(result);

        return result.ToImmutableDictionary(x => x.Key, x => x.Value.DrainToImmutable());

        SyntaxList<DeclarationSyntax> GetSyntaxMembers(SingleNamespaceDeclaration x)
        {
            return x.SyntaxReference.Syntax switch
            {
                CompilationUnitSyntax cu => cu.Members,
                NamespaceDeclarationSyntax ns => ns.Members,
                _ => new SyntaxList<DeclarationSyntax>(),
            };
        }
    }

    private Symbol BuildSymbol(MergedDeclaration declaration)
    {
        return declaration switch
        {
            MergedNamespaceDeclaration ns => new SourceNamespaceSymbol(
                DeclaringCompilation,
                ns,
                ContainingAssembly,
                PublicSymbol
            ).PublicSymbol,
            MergedTypeDeclaration t => BuildSymbol(t),
        };
    }

    private NamedTypeSymbol BuildSymbol(MergedTypeDeclaration declaration)
    {
        return declaration.Kind switch
        {
            DeclarationKind.Namespace => throw new InvalidOperationException(
                "Namespace declaration cannot be built as a type"
            ),
            DeclarationKind.Class => new SourceClassSymbol(
                DeclaringCompilation,
                declaration,
                PublicSymbol
            ).PublicSymbol,
            DeclarationKind.Attribute => new SourceAttributeSymbol(
                DeclaringCompilation,
                declaration,
                PublicSymbol
            ).PublicSymbol,
            _ => throw new InvalidOperationException("Unknown declaration kind"),
        };
    }

    private VariableSymbol BuildSymbol(GlobalVariableDeclarationSyntax variableDeclaration)
    {
        return new SourceGlobalVariableSymbol(
            DeclaringCompilation,
            PublicSymbol,
            variableDeclaration
        ).PublicSymbol;
    }

    private FunctionSymbol BuildSymbol(FunctionDeclarationSyntax functionDeclaration)
    {
        return new SourceFunctionSymbol(
            DeclaringCompilation,
            PublicSymbol,
            functionDeclaration
        ).PublicSymbol;
    }

    private void AddSynthesizedMembers(
        Dictionary<string, ImmutableArray<Symbol>.Builder> nameToMembersMap
    )
    {
        if (!PublicSymbol.IsGlobal)
            return;

        var globalCtor = new SynthesizedGlobalConstructor(
            DeclaringCompilation,
            PublicSymbol
        ).PublicSymbol;
        nameToMembersMap
            .GetOrAdd(globalCtor.Name, ImmutableArray.CreateBuilder<Symbol>)
            .Add(globalCtor);
    }

    public NamespaceKind NamespaceKind => NamespaceKind.Assembly;
}
