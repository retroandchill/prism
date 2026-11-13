// @file MetadataVariableSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Compiler.Semantic;
using Prism.Diagnostics;
using Prism.Metadata;
using Prism.Semantic;
using Prism.Syntax;
using ConstantValue = Prism.Constants.ConstantValue;
using VariableDefinition = Prism.Metadata.VariableDefinition;

namespace Prism.Compiler.Symbols.Metadata;

internal sealed class MetadataVariableSymbol(
    VariableDefinition definition,
    MetadataAssemblySymbol containingAssembly
) : IVariableSymbolData, ILocatableSymbolData
{
    private ImmutableArray<AttributeData> _attributes;

    public VariableSymbol PublicModel { get; }
    public string Name => definition.Name;
    public ImmutableArray<Location> Locations => containingAssembly.Locations;
    public Symbol ContainingSymbol => ContainingMemberContainer;

    public MemberContainerSymbol ContainingMemberContainer
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

    public AssemblySymbol ContainingAssembly => containingAssembly.PublicModel;

    public TypeSymbol Type
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

    public bool IsMutable => false;
    public VariableKind Kind => definition.Kind;

    // TODO: Once we start encoding MIR into the code, we may use this to handle aggregate initialize of classes
    public bool HasInitializer => false;

    public ConstantValue? ConstantValue => definition.ConstantValue;

    public ImmutableArray<AttributeData> GetAttributes()
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
