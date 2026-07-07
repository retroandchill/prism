// @file ResolvedSemanticState.cs
// 
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Frozen;
using Prism.Core.Semantic.Binding;
using Prism.Core.Semantic.Symbols;

namespace Prism.Core.Semantic;

internal sealed class ResolvedSemanticState
{
    public required FrozenDictionary<ValueSymbol, TypeSymbol> ValueTypes { get; init; }
    
    public required FrozenDictionary<VariableSymbol, BoundExpression> Initializers { get; init; }
}