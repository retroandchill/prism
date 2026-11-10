// @file MetadataNamedTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Frozen;
using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Metadata;
using Prism.Core.Semantic;
using Prism.Core.Syntax;
using Prism.Utils;
using ZLinq;

namespace Prism.Core.Symbols.Metadata;

internal sealed class MetadataNamedTypeSymbol(
    TypeDefinition definition,
    MetadataAssemblySymbol containingAssembly
) : NamedTypeSymbol
{
    private ImmutableArray<Symbol> _members;
    private ImmutableArray<AttributeData> _attributes;
    private ThreeState _isDefaultConstructible;

    public override string Name => definition.Name;
    public override DeclaredVisibility DeclaredVisibility => definition.Visibility;
    public override NamedTypeKind Kind => definition.Kind;
    public override SpecialType SpecialType => definition.SpecialType;
    public override ImmutableArray<Location> Locations => containingAssembly.Locations;
    public override Symbol ContainingSymbol => ContainingMemberContainer;

    public override MemberContainerSymbol ContainingMemberContainer
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                containingAssembly.GetOrAddMemberContainer(definition.Parent),
                null
            );
            return field;
        }
    }

    public override AssemblySymbol ContainingAssembly => containingAssembly;
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override bool IsDynamicallySized => SpecialType == SpecialType.Str;

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
            .Types.AsValueEnumerable()
            .Select(Symbol (x) => containingAssembly.GetOrAddType(x))
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
            .Concat(
                definition
                    .AttributeParameters.AsValueEnumerable()
                    .Select(Symbol (x) => containingAssembly.GetOrAddParameter(x))
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

    public override ImmutableArray<AttributeData> GetAttributes()
    {
        if (!_attributes.IsDefault)
            return _attributes;

        ImmutableInterlocked.InterlockedInitialize(
            ref _attributes,
            containingAssembly.GetAttributes(definition)
        );
        return _attributes;
    }

    public override bool IsDefaultConstructible
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
