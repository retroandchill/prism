// @file HandleAttributeInfo.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Prism.Core.Metadata;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;

namespace Prism.SourceGenerator.Models.Attributes;

[AttributeInfoType<CompositeHandleAttribute>]
public readonly record struct CompositeHandleAttributeInfo(ImmutableArray<ITypeSymbol> Types)
{
    public CompositeHandleAttributeInfo(ITypeSymbol[] types)
        : this(types.ToImmutableArray()) { }
}
