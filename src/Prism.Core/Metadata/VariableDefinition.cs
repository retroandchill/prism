// @file VariableDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Prism.Core.Semantic;
using Prism.Core.Symbols;

namespace Prism.Core.Metadata;

internal sealed class VariableDefinition(
    VariableHandle handle,
    VariableAttributes attributes,
    string name,
    TypeHandle type,
    ConstantHandle constantValue,
    MetadataReader reader
)
{
    public string Name { get; } = name;
    private Lazy<ConstantValue?>? _constantValue;

    public VariableKind Kind => attributes.Kind;

    public DeclaredVisibility Visibility => attributes.Visibility;

    public TypeDescriptor Type
    {
        get
        {
            if (field.Value is not null)
                return field;

            Interlocked.CompareExchange(
                ref Unsafe.As<TypeDescriptor, object?>(ref field),
                reader.GetTypeDescriptor(type).Value,
                null
            );
            return field;
        }
    }

    public ConstantValue? ConstantValue
    {
        get
        {
            if (_constantValue is not null)
                return _constantValue.Value;

            Interlocked.CompareExchange(
                ref _constantValue,
                new Lazy<ConstantValue?>(
                    () =>
                        !constantValue.IsNone
                            ? reader.GetConstant(constantValue)
                            : (ConstantValue?)null,
                    LazyThreadSafetyMode.PublicationOnly
                ),
                null
            );
            return _constantValue.Value;
        }
    }

    public ImmutableArray<CustomAttributeDefinition> Attributes
    {
        get
        {
            if (!field.IsDefault)
                return field;

            var mappings = reader.CustomAttributesByMember[handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(reader.GetCustomAttribute)]
            );
            return field;
        }
    }
}
