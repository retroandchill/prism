// @file MetadataFunctionSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Prism.Core.Abi;
using Prism.Core.Diagnostics;
using Prism.Core.Metadata;
using Prism.Core.Syntax;
using ZLinq;

namespace Prism.Core.Symbols.Metadata;

internal sealed class MetadataFunctionSymbol(
    FunctionDefinition definition,
    MetadataAssemblySymbol containingAssembly
) : FunctionSymbol
{
    private ImmutableArray<AttributeData> _attributes;

    public override string Name => definition.Name;
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

    public override TypeSymbol ReturnType
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                containingAssembly.GetReferencedType(definition.ReturnType),
                null
            );
            return field;
        }
    }

    public override TypeSymbol? ReceiverType
    {
        get
        {
            if (definition.ReceiverType is not { } receiver)
                return null;

            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                containingAssembly.GetReferencedType(receiver),
                null
            );
            return field;
        }
    }

    public override ImmutableArray<ParameterSymbol> Parameters
    {
        get
        {
            if (!field.IsDefault)
                return field;

            var parameters = new ParameterSymbol[definition.Parameters.Length];
            foreach (var (i, param) in definition.Parameters.AsValueEnumerable().Index())
            {
                parameters[i] = containingAssembly.GetOrAddParameter(param);
            }

            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                ImmutableCollectionsMarshal.AsImmutableArray(parameters)
            );
            return field;
        }
    }

    public override FunctionKind Kind => definition.Kind;
    internal override AbiKind AbiKind => definition.AbiKind;
    public override bool IsExtern => definition.IsExtern;

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
}
