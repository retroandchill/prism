using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Prism.Core.Binding;
using Prism.Core.Compiling;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
using Prism.Core.Symbols.Synthesized;
using Prism.Core.Syntax;
using Prism.Core.Text;
using Prism.Core.Utils;
using ZLinq;

namespace Prism.Core.Symbols.Source;

internal sealed class SourceNamespaceSymbol : NamespaceSymbol
{
    private readonly MergedNamespaceDeclaration _mergedDeclaration;
    private ImmutableArray<Symbol> _members;
    private ImmutableDictionary<string, ImmutableArray<Symbol>>? _nameToMembersMap;
    private SymbolCompletionState _completionState;
    private readonly Lock _memberChecksLock = new();

    public SourceNamespaceSymbol(
        MergedNamespaceDeclaration declaration,
        AssemblySymbol assembly,
        Symbol? containingSymbol
    )
        : base(declaration.Name, containingSymbol)
    {
        _mergedDeclaration = declaration;
        ContainingAssembly = assembly;
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        foreach (var decl in _mergedDeclaration.Declarations)
        {
            compilation.CacheSymbol(decl.SyntaxReference.Syntax, this);
        }
    }

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

    public override bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithin)
    {
        if (IsGlobal)
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

    internal override bool NeedsCompletion => true;

    internal override void ForceComplete(
        SourceLocation? location,
        Predicate<Symbol>? filter,
        CancellationToken cancellationToken
    )
    {
        if (filter?.Invoke(this) == false)
            return;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var incompletePart = _completionState.NextIncompletePart;
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
                        _completionState.MarkPartComplete(CompletionPart.MembersCompleted);
                        break;
                    }
                    var allParts =
                        location is null && filter is null
                            ? CompletionPart.NamespaceAll
                            : CompletionPart.NamespaceAll & ~CompletionPart.MembersCompleted;
                    _completionState.MarkPartComplete(allParts);
                    return;
                }
                case CompletionPart.StartChecks or CompletionPart.FinishChecks:
                    LazyMemberChecks();
                    break;
                case CompletionPart.None:
                    return;
                default:
                    // Any other values are for other kinds of symbols
                    _completionState.MarkPartComplete(
                        CompletionPart.All & ~CompletionPart.NamespaceAll
                    );
                    break;
            }

            _completionState.WaitPartComplete(incompletePart, cancellationToken);
        }
    }

    internal override bool IsComplete(CompletionPart part)
    {
        return _completionState.IsComplete(part);
    }

    private void LazyMemberChecks()
    {
        if (_completionState.IsComplete(CompletionPart.FinishChecks))
            return;

        using var scope = _memberChecksLock.EnterScope();
        if (!_completionState.MarkPartComplete(CompletionPart.StartChecks))
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
            _completionState.MarkPartComplete(CompletionPart.FinishChecks);
        }
    }

    public override ImmutableArray<Symbol> GetMembers()
    {
        if (!_members.IsDefault)
            return _members;

        ImmutableInterlocked.InterlockedCompareExchange(ref _members, ComputeMembers(), default);
        return _members;
    }

    public override ImmutableArray<Symbol> GetMembers(string name)
    {
        return GetNameToMembersMap().GetValueOrDefault(name, []);
    }

    private ImmutableArray<Symbol> ComputeMembers()
    {
        var members = GetNameToMembersMap()
            .Values.AsValueEnumerable()
            .SelectMany(v => v.AsValueEnumerable())
            .ToArray();
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        Array.Sort(members, SymbolLocationComparer.Get(compilation));
        return ImmutableCollectionsMarshal.AsImmutableArray(members);
    }

    private ImmutableDictionary<string, ImmutableArray<Symbol>> GetNameToMembersMap()
    {
        if (_nameToMembersMap is not null)
            return _nameToMembersMap;

        using var context = BindingContext.Create();
        if (
            Interlocked.CompareExchange(ref _nameToMembersMap, MakeNameToMembersMap(context), null)
            is not null
        )
            return _nameToMembersMap;

        AddDeclarationDiagnostics(context);
        _completionState.MarkPartComplete(CompletionPart.Members);
        return _nameToMembersMap;
    }

    private ImmutableDictionary<string, ImmutableArray<Symbol>> MakeNameToMembersMap(
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
                ns,
                ContainingAssembly,
                this
            ),
            MergedTypeDeclaration t => BuildSymbol(t),
        };
    }

    private Symbol BuildSymbol(MergedTypeDeclaration declaration)
    {
        return declaration.Kind switch
        {
            DeclarationKind.Namespace => throw new InvalidOperationException(
                "Namespace declaration cannot be built as a type"
            ),
            DeclarationKind.Class => new SourceClassSymbol(declaration, this),
            DeclarationKind.Attribute => new SourceAttributeSymbol(declaration, this),
            _ => throw new InvalidOperationException("Unknown declaration kind"),
        };
    }

    private SourceGlobalVariableSymbol BuildSymbol(
        GlobalVariableDeclarationSyntax variableDeclaration
    )
    {
        return new SourceGlobalVariableSymbol(
            variableDeclaration.Identifier.IdentifierName,
            this,
            variableDeclaration
        );
    }

    private SourceFunctionSymbol BuildSymbol(FunctionDeclarationSyntax functionDeclaration)
    {
        return new SourceFunctionSymbol(
            functionDeclaration.Identifier.IdentifierName,
            this,
            functionDeclaration
        );
    }

    private void AddSynthesizedMembers(
        Dictionary<string, ImmutableArray<Symbol>.Builder> nameToMembersMap
    )
    {
        if (!IsGlobal)
            return;

        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        var globalCtor = new SynthesizedGlobalConstructor(this);
        nameToMembersMap
            .GetOrAdd(globalCtor.Name, ImmutableArray.CreateBuilder<Symbol>)
            .Add(globalCtor);
    }

    public override NamespaceKind NamespaceKind => NamespaceKind.Assembly;
    public override Compilation? ContainingCompilation => null;
}
