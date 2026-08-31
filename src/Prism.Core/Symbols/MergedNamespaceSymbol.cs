using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Compiling;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using ZLinq;

namespace Prism.Core.Symbols;

internal sealed class MergedNamespaceSymbol : NamespaceSymbol
{
    private readonly ImmutableArray<NamespaceSymbol> _namespaces;
    private ImmutableArray<Symbol> _members;
    private readonly ConcurrentDictionary<string, ImmutableArray<Symbol>> _nameToMembers = new();

    public MergedNamespaceSymbol(
        string name,
        Symbol? containingSymbol,
        Compilation compilation,
        ImmutableArray<NamespaceSymbol> namespaces
    )
        : base(name, containingSymbol)
    {
        ContainingCompilation = compilation;
        _namespaces = namespaces;
    }

    public static NamespaceSymbol Create(
        Compilation compilation,
        NamespaceSymbol? containingSymbol,
        ImmutableArray<NamespaceSymbol> namespaces
    )
    {
        Debug.Assert(!namespaces.IsDefaultOrEmpty, "Cannot merge an empty set of namespaces");

        if (namespaces.Length == 1)
            return namespaces[0];

        return new MergedNamespaceSymbol(
            namespaces[0].Name,
            containingSymbol,
            compilation,
            namespaces
        );
    }

    public override ImmutableArray<Location> Locations
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Interlocked.CompareExchange(
                ref field,
                [
                    .. _namespaces
                        .AsValueEnumerable()
                        .SelectMany(x => x.Locations.AsValueEnumerable()),
                ],
                default
            );
            return field;
        }
    }

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Interlocked.CompareExchange(
                ref field,
                [
                    .. _namespaces
                        .AsValueEnumerable()
                        .SelectMany(x => x.DeclaringSyntaxReferences.AsValueEnumerable()),
                ],
                default
            );
            return field;
        }
    }

    public override ImmutableArray<Symbol> GetMembers()
    {
        if (!_members.IsDefault)
            return _members;

        var namespaceGroups = _namespaces
            .AsValueEnumerable()
            .SelectMany(x => x.GetMembers().AsValueEnumerable())
            .OfType<NamespaceSymbol>()
            .GroupBy(x => x.Name)
            .ToDictionary(x => x.Key, x => x.ToImmutableArray());

        var emittedNamespaces = new HashSet<string>();
        var mergedMembers = ImmutableArray.CreateBuilder<Symbol>();

        foreach (
            var member in _namespaces
                .AsValueEnumerable()
                .SelectMany(x => x.GetMembers().AsValueEnumerable())
        )
        {
            if (member is NamespaceSymbol ns)
            {
                if (emittedNamespaces.Add(ns.Name))
                {
                    mergedMembers.Add(
                        Create(ContainingCompilation, this, namespaceGroups[ns.Name])
                    );
                }

                continue;
            }

            mergedMembers.Add(member);
        }

        Interlocked.CompareExchange(ref _members, mergedMembers.DrainToImmutable(), default);
        return _members;
    }

    public override ImmutableArray<Symbol> GetMembers(string name)
    {
        return _nameToMembers.GetOrAdd(name, static (n, self) => self.ComputeMembers(n), this);
    }

    private ImmutableArray<Symbol> ComputeMembers(string name)
    {
        return
        [
            .. _namespaces
                .AsValueEnumerable()
                .SelectMany(x => x.GetMembers(name).AsValueEnumerable()),
        ];
    }

    public override NamespaceKind NamespaceKind => NamespaceKind.Compilation;
    public override Compilation ContainingCompilation { get; }

    internal override void ForceComplete(
        SourceLocation? location,
        Predicate<Symbol>? filter,
        CancellationToken cancellationToken
    )
    {
        foreach (var ns in _namespaces)
        {
            ns.ForceComplete(location, filter, cancellationToken);
        }
    }
}
