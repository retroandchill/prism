// @file CustomAttributeDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Semantic;
using Prism.Core.Utils;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Core.Metadata;

public interface IAttributeHolderDefinition
{
    ImmutableArray<CustomAttributeDefinition> CustomAttributes { get; }
}

public union AttributeHolderDefinition(
    TypeDefinition,
    VariableDefinition,
    FunctionDefinition,
    ParameterDefinition
)
{
    public readonly AttributeHolderHandle Handle
    {
        get
        {
            return this switch
            {
                FunctionDefinition functionDefinition => functionDefinition.Handle,
                ParameterDefinition parameterDefinition => parameterDefinition.Handle,
                TypeDefinition typeDefinition => typeDefinition.Handle,
                VariableDefinition variableDefinition => variableDefinition.Handle,
            };
        }
    }
}

public sealed partial class CustomAttributeDefinition
{
    private readonly BlobHandle _arguments;
    private readonly MetadataReader? _reader;

    internal CustomAttributeDefinition(
        AttributeHolderHandle parent,
        NamedTypeHandle type,
        BlobHandle arguments,
        MetadataReader reader
    )
    {
        _arguments = arguments;
        _reader = reader;
        ParentHandle = parent;
        TypeHandle = type;
    }

    [MapperConstructor]
    private CustomAttributeDefinition(NamedType type, ImmutableArray<ConstantValue> arguments)
    {
        Type = type;
        Arguments = arguments;
    }

    public AttributeHolderHandle ParentHandle { get; private set; }

    public AttributeHolderDefinition Parent
    {
        get
        {
            if (field.Value is not null)
                return field;

            Debug.Assert(_reader is not null);
            UnionInterlocked.Initialize(ref field, _reader.GetAttributeHolder(ParentHandle));
            return field;
        }
        private set
        {
            field = value;
            ParentHandle = value.Handle;
        }
    }

    public NamedTypeHandle TypeHandle { get; private init; }

    public NamedType Type
    {
        get
        {
            if (field.Value is not null)
                return field;

            Debug.Assert(_reader is not null);
            UnionInterlocked.Initialize(ref field, _reader.GetNamedType(TypeHandle));
            return field;
        }
        private init
        {
            field = value;
            TypeHandle = value.Handle;
        }
    }

    public ImmutableArray<ConstantValue> Arguments
    {
        get
        {
            if (!field.IsDefault)
                return field;

            if (_arguments.IsNone)
            {
                ImmutableInterlocked.InterlockedInitialize(ref field, []);
                return field;
            }

            Debug.Assert(_reader is not null);
            var argHandles = _reader.GetBlob<ImmutableArray<ConstantHandle>>(_arguments);
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. argHandles.Select(_reader.GetConstant)]
            );
            return field;
        }
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target, UseReferenceHandling = true)]
    [UseStaticMapper(typeof(MemberContainerDefinition.Mapper))]
    [UseStaticMapper(typeof(TypeDefinition.Mapper))]
    [UseStaticMapper(typeof(TypeMapper))]
    [UseStaticMapper(typeof(FunctionDefinition.Mapper))]
    internal static partial class Mapper
    {
        [MapperIgnoreTarget(nameof(TypeHandle))]
        [MapperIgnoreTarget(nameof(ParentHandle))]
        public static partial CustomAttributeDefinition ToDefinition(
            CustomAttributeDefinitionBuilder builder,
            [ReferenceHandler] IReferenceHandler handler
        );

        private static AttributeHolderDefinition ToDefinition(
            AttributeHolderDefinitionBuilder builder,
            [ReferenceHandler] IReferenceHandler handler
        )
        {
            return builder switch
            {
                TypeDefinitionBuilder def => def.ToDefinition(handler),
                VariableDefinitionBuilder def => def.ToDefinition(handler),
                FunctionDefinitionBuilder def => def.ToDefinition(handler),
                ParameterDefinitionBuilder def => def.ToDefinition(handler),
            };
        }
    }
}
