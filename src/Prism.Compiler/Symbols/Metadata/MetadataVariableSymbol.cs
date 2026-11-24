// @file MetadataVariableSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Compiler.Semantic;
using Prism.Diagnostics;
using Prism.Metadata;
using Prism.Semantic;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;
using Prism.Syntax;
using ConstantValue = Prism.Constants.ConstantValue;
using VariableDefinition = Prism.Metadata.VariableDefinition;

namespace Prism.Compiler.Symbols.Metadata;

internal sealed class MetadataVariableSymbol : IVariableSymbolData, ILocatableSymbolData
{
    private ImmutableArray<AttributeData> _attributes;
    private readonly VariableDefinition _definition;
    private readonly MetadataAssemblySymbol _containingAssembly;

    public MetadataVariableSymbol(
        VariableDefinition definition,
        MetadataAssemblySymbol containingAssembly
    )
    {
        _definition = definition;
        _containingAssembly = containingAssembly;
        PublicSymbol = new VariableSymbol(this);
    }

    public VariableSymbol PublicSymbol { get; }
    public string Name => _definition.Name;
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

    public AssemblySymbol ContainingAssembly => _containingAssembly.PublicSymbol;

    public TypeSymbol Type
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                _containingAssembly.GetReferencedType(_definition.Type),
                null
            );
            return field;
        }
    }

    public bool IsMutable => false;
    public VariableKind Kind => _definition.Kind;

    // TODO: Once we start encoding MIR into the code, we may use this to handle aggregate initialize of classes
    public bool HasInitializer => false;

    public ConstantValue? ConstantValue => _definition.ConstantValue;

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
}
