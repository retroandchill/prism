// @file MetadataVariableSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Semantic;
using Prism.Diagnostics;
using Prism.Metadata;
using Prism.Syntax;
using ConstantValue = Prism.Constants.ConstantValue;
using VariableDefinition = Prism.Metadata.VariableDefinition;

namespace Prism.Core.Symbols.Metadata;

internal sealed class MetadataVariableSymbol(
    VariableDefinition definition,
    MetadataAssemblySymbol containingAssembly
) : VariableSymbol
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
    public override VariableKind Kind => definition.Kind;

    // TODO: Once we start encoding MIR into the code, we may use this to handle aggregate initialize of classes
    public override bool HasInitializer => false;

    public override ConstantValue? ConstantValue => definition.ConstantValue;

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
