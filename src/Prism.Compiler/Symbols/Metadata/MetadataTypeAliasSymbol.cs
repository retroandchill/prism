using System.Collections.Immutable;
using Prism.Metadata;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;
using Prism.Utils;

namespace Prism.Compiler.Symbols.Metadata;

internal sealed class MetadataTypeAliasSymbol
    : ITypeAliasSymbolData,
        IMemberContainerSymbolData,
        ILocatableSymbolData
{
    private readonly TypeAlias _definition;
    private readonly MetadataAssemblySymbol _containingAssembly;

    public MetadataTypeAliasSymbol(TypeAlias definition, MetadataAssemblySymbol containingAssembly)
    {
        _definition = definition;
        _containingAssembly = containingAssembly;
        PublicSymbol = new TypeAliasSymbol(this);
    }

    public TypeAliasSymbol PublicSymbol { get; }

    public ImmutableArray<Location> Locations => _containingAssembly.Locations;

    public string Name => _definition.Name;
    public Symbol ContainingSymbol => ContainingMemberContainer;

    public MemberContainerSymbol ContainingMemberContainer
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                _containingAssembly.GetOrAddMemberContainer(_definition.Parent),
                null
            );
            return field;
        }
    }

    public AssemblySymbol ContainingAssembly => _containingAssembly.PublicSymbol;

    public bool IsDynamicallySized => AliasedType.IsDynamicallySized;
    public bool IsDefaultConstructible => AliasedType.IsDefaultConstructible;

    public TypeSymbol AliasedType
    {
        get
        {
            return LazyComputation.LazyCompute(ref field, this, static @this =>
                @this._containingAssembly.GetReferencedType(@this._definition.AliasedType)
            );
        }
    }

    public ImmutableArray<Symbol> GetMembers() => AliasedType.GetMembers();

    public ImmutableArray<Symbol> GetMembers(string name) => AliasedType.GetMembers(name);
}
