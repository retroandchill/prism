using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Prism.Core.Binding;
using Prism.Core.Compiling;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
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

    public override ImmutableArray<Location> Locations
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedCompareExchange(
                ref field,
                [.. _mergedDeclaration.Declarations.Select(Location (d) => d.NameLocation)],
                default
            );
            return field;
        }
    }

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

        Interlocked.CompareExchange(ref _nameToMembersMap, MakeNameToMembersMap(), null);
        _completionState.MarkPartComplete(CompletionPart.Members);
        return _nameToMembersMap;
    }

    private ImmutableDictionary<string, ImmutableArray<Symbol>> MakeNameToMembersMap()
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
            var symbol = syntax switch
            {
                NamespaceDeclarationSyntax => null,
                VariableDeclarationSyntax variable => BuildSymbol(variable),
                FunctionDeclarationSyntax function => BuildSymbol(function),
                _ => null,
            };
            if (symbol is null)
                continue;

            result.GetOrAdd(symbol.Name, ImmutableArray.CreateBuilder<Symbol>).Add(symbol);
        }

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
            _ => throw new ArgumentOutOfRangeException(nameof(declaration), declaration, null),
        };
    }

    private Symbol BuildSymbol(VariableDeclarationSyntax variableDeclaration)
    {
        return new SourceGlobalVariableSymbol(
            variableDeclaration.Identifier.IdentifierName,
            this,
            variableDeclaration
        );
    }

    private Symbol BuildSymbol(FunctionDeclarationSyntax functionDeclaration)
    {
        return new SourceFunctionSymbol(
            functionDeclaration.Identifier.IdentifierName,
            this,
            functionDeclaration
        );
    }

    public override NamespaceKind NamespaceKind => NamespaceKind.Assembly;
    public override Compilation? ContainingCompilation => null;
}
