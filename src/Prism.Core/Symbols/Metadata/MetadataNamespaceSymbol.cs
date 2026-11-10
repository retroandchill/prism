using System.Collections.Frozen;
using System.Collections.Immutable;
using Prism.Core.Compiling;
using Prism.Diagnostics;
using Prism.Syntax;
using ZLinq;
using NamespaceDefinition = Prism.Metadata.NamespaceDefinition;

namespace Prism.Core.Symbols.Metadata;

internal sealed class MetadataNamespaceSymbol(
    NamespaceDefinition definition,
    MetadataAssemblySymbol containingAssembly
) : NamespaceSymbol
{
    private ImmutableArray<Symbol> _members;

    public override NamespaceKind NamespaceKind => NamespaceKind.Assembly;
    public override Compilation? ContainingCompilation => null;

    public override string Name => definition.Name;
    public override Symbol ContainingSymbol => (Symbol?)ContainingNamespace ?? containingAssembly;
    public override ImmutableArray<Location> Locations => containingAssembly.Locations;
    public override AssemblySymbol ContainingAssembly => containingAssembly;

    public override NamespaceSymbol? ContainingNamespace
    {
        get
        {
            if (definition.Parent is null)
                return null;

            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                containingAssembly.GetOrAddNamespace(definition.Parent),
                null
            );
            return field;
        }
    }

    public override MemberContainerSymbol? ContainingMemberContainer => ContainingNamespace;
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override ImmutableArray<Symbol> GetMembers()
    {
        if (!_members.IsDefault)
            return _members;

        ImmutableInterlocked.InterlockedInitialize(ref _members, ComputeMembers());
        return _members;
    }

    public override ImmutableArray<Symbol> GetMembers(string name)
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
        return definition
            .NestedNamespaces.AsValueEnumerable()
            .Select(Symbol (x) => containingAssembly.GetOrAddNamespace(x))
            .Concat(
                definition
                    .Types.AsValueEnumerable()
                    .Select(Symbol (x) => containingAssembly.GetOrAddType(x))
            )
            .Concat(
                definition
                    .Variables.AsValueEnumerable()
                    .Select(Symbol (x) => containingAssembly.GetOrAddVariable(x))
            )
            .Concat(
                definition
                    .Functions.AsValueEnumerable()
                    .Select(Symbol (x) => containingAssembly.GetOrAddFunction(x))
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
