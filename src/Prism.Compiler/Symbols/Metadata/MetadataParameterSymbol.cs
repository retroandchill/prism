// @file MetadataParameterSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Compiler.Mappers;
using Prism.Diagnostics;
using Prism.Semantic;
using Prism.Syntax;
using ParameterDefinition = Prism.Metadata.ParameterDefinition;

namespace Prism.Compiler.Symbols.Metadata;

internal sealed class MetadataParameterSymbol : IParameterSymbolData, ILocatableSymbolData
{
    private ImmutableArray<AttributeData> _attributes;
    private readonly ParameterDefinition _definition;
    private readonly MetadataAssemblySymbol _containingAssembly;

    public MetadataParameterSymbol(
        ParameterDefinition definition,
        MetadataAssemblySymbol containingAssembly
    )
    {
        _definition = definition;
        _containingAssembly = containingAssembly;
        PublicModel = new ParameterSymbol(this);
    }

    public ParameterSymbol PublicModel { get; }
    public string Name => _definition.Name;
    public ImmutableArray<Location> Locations => _containingAssembly.Locations;

    public Symbol ContainingSymbol
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                _containingAssembly.GetOrAddFunctionOrAttribute(_definition.Parent),
                null
            );
            return field;
        }
    }

    public AssemblySymbol ContainingAssembly => _containingAssembly.PublicModel;
    public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

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

    public FunctionSymbol? ContainingFunction => ContainingSymbol as FunctionSymbol;

    public ParameterDefault? DefaultValue
    {
        get
        {
            if (_definition.DefaultValue is null)
                return null;

            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, _definition.DefaultValue.ToSymbol(), null);
            return field;
        }
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
