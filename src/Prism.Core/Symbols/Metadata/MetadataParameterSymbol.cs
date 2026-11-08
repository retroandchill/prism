// @file MetadataParameterSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Mappers;
using Prism.Core.Metadata;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Metadata;

internal sealed class MetadataParameterSymbol(
    ParameterDefinition definition,
    MetadataAssemblySymbol containingAssembly
) : ParameterSymbol
{
    private ImmutableArray<AttributeData> _attributes;

    public override string Name => definition.Name;
    public override ImmutableArray<Location> Locations => containingAssembly.Locations;

    public override Symbol ContainingSymbol
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                containingAssembly.GetOrAddFunctionOrAttribute(definition.Parent),
                null
            );
            return field;
        }
    }

    public override AssemblySymbol ContainingAssembly => containingAssembly;
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override TypeSymbol Type
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                containingAssembly.GetReferencedType(definition.Type),
                null
            );
            return field;
        }
    }

    public override bool IsMutable => false;

    public override FunctionSymbol? ContainingFunction => ContainingSymbol as FunctionSymbol;

    public override ParameterDefault? DefaultValue
    {
        get
        {
            if (definition.DefaultValue is null)
                return null;

            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, definition.DefaultValue.ToSymbol(), null);
            return field;
        }
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
}
