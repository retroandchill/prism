// @file MetadataVariableSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.BoundTree;
using Prism.BoundTree.Symbols;
using Prism.Constants;
using Prism.Metadata.Symbols;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;
using Prism.Utils;

namespace Prism.Compiler.Symbols.Metadata;

internal sealed class MetadataVariableSymbol : IBoundVariableSymbolData, ILocatableSymbolData
{
    private ImmutableArray<AttributeData> _attributes;
    private readonly VariableDefinition _definition;
    private readonly MetadataAssemblySymbol _containingAssembly;
    private BoundVariableInitializer? _boundInitializer;

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

    public bool HasInitializer => _definition.Initializer is not null;

    public ConstantValue? ConstantValue => _definition.ConstantValue;

    public BoundVariableInitializer GetBoundInitializer(
        CancellationToken cancellationToken = default
    )
    {
        return LazyComputation.LazyCompute(ref _boundInitializer, this, static @this =>
            @this._containingAssembly.GetBoundInitializer(@this.PublicSymbol, @this._definition)
        );
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
}
