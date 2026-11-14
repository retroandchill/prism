using System.Collections.Frozen;
using System.Collections.Immutable;
using Prism.Diagnostics;
using Prism.Semantic;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;
using Prism.Syntax;
using ZLinq;
using NamespaceDefinition = Prism.Metadata.NamespaceDefinition;

namespace Prism.Compiler.Symbols.Metadata;

internal sealed class MetadataNamespaceSymbol : INamespaceSymbolData, ILocatableSymbolData
{
    private ImmutableArray<Symbol> _members;
    private readonly NamespaceDefinition _definition;
    private readonly MetadataAssemblySymbol _containingAssembly;

    public MetadataNamespaceSymbol(
        NamespaceDefinition definition,
        MetadataAssemblySymbol containingAssembly
    )
    {
        _definition = definition;
        _containingAssembly = containingAssembly;
        PublicModel = new NamespaceSymbol(this);
    }

    public NamespaceSymbol PublicModel { get; }

    public NamespaceKind NamespaceKind => NamespaceKind.Assembly;

    public string Name => _definition.Name;
    public Symbol ContainingSymbol =>
        (Symbol?)ContainingNamespace ?? _containingAssembly.PublicModel;
    public AssemblySymbol ContainingAssembly => _containingAssembly.PublicModel;
    public ImmutableArray<Location> Locations => _containingAssembly.Locations;

    public NamespaceSymbol? ContainingNamespace
    {
        get
        {
            if (_definition.Parent is null)
                return null;

            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                _containingAssembly.GetOrAddNamespace(_definition.Parent),
                null
            );
            return field;
        }
    }

    public TypeSymbol? ContainingType => null;

    public MemberContainerSymbol? ContainingMemberContainer => ContainingNamespace;

    public ImmutableArray<Symbol> GetMembers()
    {
        if (!_members.IsDefault)
            return _members;

        ImmutableInterlocked.InterlockedInitialize(ref _members, ComputeMembers());
        return _members;
    }

    public ImmutableArray<Symbol> GetMembers(string name)
    {
        return MembersByName.GetValueOrDefault(name, []);
    }

    private FrozenDictionary<string, ImmutableArray<Symbol>> MembersByName
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, ComputeMembersByName(), null);
            return field;
        }
    }

    private ImmutableArray<Symbol> ComputeMembers()
    {
        return _definition
            .NestedNamespaces.AsValueEnumerable()
            .Select(Symbol (x) => _containingAssembly.GetOrAddNamespace(x))
            .Concat(
                _definition
                    .Types.AsValueEnumerable()
                    .Select(Symbol (x) => _containingAssembly.GetOrAddType(x))
            )
            .Concat(
                _definition
                    .Variables.AsValueEnumerable()
                    .Select(Symbol (x) => _containingAssembly.GetOrAddVariable(x))
            )
            .Concat(
                _definition
                    .Functions.AsValueEnumerable()
                    .Select(Symbol (x) => _containingAssembly.GetOrAddFunction(x))
            )
            .ToImmutableArray();
    }

    private FrozenDictionary<string, ImmutableArray<Symbol>> ComputeMembersByName()
    {
        return GetMembers()
            .AsValueEnumerable()
            .GroupBy(x => x.Name)
            .ToFrozenDictionary(x => x.Key, x => x.ToImmutableArray());
    }
}
