// @file DefaultLoopBinder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Compiler.Symbols;
using Prism.Semantic;

namespace Prism.Compiler.Binding;

internal sealed class DefaultLoopBinder(Binder next, LabelSymbol loopLabel)
    : LoopBinder(next, loopLabel)
{
    protected override ImmutableArray<VariableSymbol> BuildLocalVariables()
    {
        return [];
    }
}
