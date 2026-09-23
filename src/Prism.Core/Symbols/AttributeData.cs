// @file AttributeData.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Semantic;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols;

public readonly record struct AttributeArgument(ParameterSymbol Parameter, ConstantValue Value);

public sealed class AttributeData
{
    internal AttributeData(
        SyntaxReference? syntaxReference,
        NamedTypeSymbol attribute,
        ImmutableArray<AttributeArgument> arguments
    )
    {
        Debug.Assert(attribute.Kind == NamedTypeKind.Attribute);

        SyntaxReference = syntaxReference;
        Attribute = attribute;
        Arguments = arguments;
    }

    public SyntaxReference? SyntaxReference { get; }

    public NamedTypeSymbol Attribute { get; }

    public ImmutableArray<AttributeArgument> Arguments { get; }
}
