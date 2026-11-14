// @file MetadataNamedTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Frozen;
using System.Collections.Immutable;
using Prism.Compiler.Semantic;
using Prism.Diagnostics;
using Prism.Metadata;
using Prism.Semantic;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;
using Prism.Syntax;
using Prism.Utils;
using ZLinq;
using TypeDefinition = Prism.Metadata.TypeDefinition;

namespace Prism.Compiler.Symbols.Metadata;

internal sealed class MetadataNamedTypeSymbol
    : INamedTypeSymbolData,
        IMemberContainerSymbolData,
        ILocatableSymbolData,
        IVisibilityGuardedSymbolData
{
    private ImmutableArray<Symbol> _members;
    private ImmutableArray<AttributeData> _attributes;
    private ThreeState _isDefaultConstructible;
    private readonly TypeDefinition _definition;
    private readonly MetadataAssemblySymbol _containingAssembly;

    public MetadataNamedTypeSymbol(
        TypeDefinition definition,
        MetadataAssemblySymbol containingAssembly
    )
    {
        _definition = definition;
        _containingAssembly = containingAssembly;
        PublicModel = new NamedTypeSymbol(this);
    }

    public NamedTypeSymbol PublicModel { get; }

    public string Name => _definition.Name;
    public DeclaredVisibility DeclaredVisibility => _definition.Visibility;
    public NamedTypeKind Kind => _definition.Kind;
    public SpecialType SpecialType => _definition.SpecialType;
    public ImmutableArray<Location> Locations => _containingAssembly.Locations;
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

    public AssemblySymbol ContainingAssembly => _containingAssembly.PublicModel;

    public bool IsDynamicallySized => SpecialType == SpecialType.Str;

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
            .Types.AsValueEnumerable()
            .Select(Symbol (x) => _containingAssembly.GetOrAddType(x))
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
            .Concat(
                _definition
                    .AttributeParameters.AsValueEnumerable()
                    .Select(Symbol (x) => _containingAssembly.GetOrAddParameter(x))
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

    public ImmutableArray<AttributeData> GetAttributes()
    {
        if (!_attributes.IsDefault)
            return _attributes;

        ImmutableInterlocked.InterlockedInitialize(
            ref _attributes,
            _containingAssembly.GetAttributes(_definition)
        );
        return _attributes;
    }

    public bool IsDefaultConstructible
    {
        get
        {
            if (_isDefaultConstructible.HasValue)
                return _isDefaultConstructible.Value;

            var isDefaultConstructible = ComputeIsDefaultConstructible();
            Interlocked.Exchange(
                ref _isDefaultConstructible,
                isDefaultConstructible.ToThreeState()
            );
            return isDefaultConstructible;
        }
    }

    private bool ComputeIsDefaultConstructible()
    {
        return GetMembers(CommonNames.Ctor)
            .OfType<FunctionSymbol>()
            .Any(f => f.Parameters.Length == 0);
    }
}
