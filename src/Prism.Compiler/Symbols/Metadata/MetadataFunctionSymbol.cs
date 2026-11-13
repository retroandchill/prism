// @file MetadataFunctionSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Prism.Semantic;
using ZLinq;
using FunctionDefinition = Prism.Metadata.FunctionDefinition;

namespace Prism.Compiler.Symbols.Metadata;

internal sealed class MetadataFunctionSymbol : IFunctionSymbolData, ILocatableSymbolData
{
    private ImmutableArray<AttributeData> _attributes;
    private readonly FunctionDefinition _definition;
    private readonly MetadataAssemblySymbol _containingAssembly;

    public MetadataFunctionSymbol(
        FunctionDefinition definition,
        MetadataAssemblySymbol containingAssembly
    )
    {
        _definition = definition;
        PublicModel = new FunctionSymbol(this);
        _containingAssembly = containingAssembly;
    }

    public FunctionSymbol PublicModel { get; }

    public string Name => _definition.Name;
    public ImmutableArray<Location> Locations => ContainingAssembly.Locations;
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

    public TypeSymbol ReturnType
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                _containingAssembly.GetReferencedType(_definition.ReturnType),
                null
            );
            return field;
        }
    }

    public TypeSymbol? ReceiverType
    {
        get
        {
            if (_definition.ReceiverType is not { } receiver)
                return null;

            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                _containingAssembly.GetReferencedType(receiver),
                null
            );
            return field;
        }
    }

    public ImmutableArray<ParameterSymbol> Parameters
    {
        get
        {
            if (!field.IsDefault)
                return field;

            var parameters = new ParameterSymbol[_definition.Parameters.Length];
            foreach (var (i, param) in _definition.Parameters.AsValueEnumerable().Index())
            {
                parameters[i] = _containingAssembly.GetOrAddParameter(param);
            }

            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                ImmutableCollectionsMarshal.AsImmutableArray(parameters)
            );
            return field;
        }
    }

    public FunctionKind Kind => _definition.Kind;
    public AbiKind AbiKind => _definition.AbiKind;
    public bool IsExtern => _definition.IsExtern;

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
