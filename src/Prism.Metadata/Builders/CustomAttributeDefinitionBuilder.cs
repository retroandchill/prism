// @file CustomAttributeDefinitionBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Constants;

namespace Prism.Metadata.Builders;

public readonly union AttributeHolderDefinitionBuilder(
    TypeDefinitionBuilder,
    VariableDefinitionBuilder,
    FunctionDefinitionBuilder,
    ParameterDefinitionBuilder,
    EnumOrdinalDefinitionBuilder
);

public sealed class CustomAttributeDefinitionBuilder
{
    internal CustomAttributeDefinitionBuilder(
        CustomAttributeHandle handle,
        TypeDescriptorBuilder type,
        AttributeHolderDefinitionBuilder parent
    )
    {
        Handle = handle;
        Type = type;
        Parent = parent;
    }

    public CustomAttributeHandle Handle { get; }

    public TypeDescriptorBuilder Type { get; }

    public AttributeHolderDefinitionBuilder Parent { get; }

    public List<ConstantValue> Arguments { get; } = [];
}
