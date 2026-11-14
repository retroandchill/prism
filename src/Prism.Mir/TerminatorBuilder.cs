// @file TerminatorBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Semantic.Symbols;

namespace Prism.Mir;

internal closed record TerminatorBuilder;

internal sealed record ReturnTerminatorBuilder : TerminatorBuilder
{
    private ReturnTerminatorBuilder() { }

    public static readonly ReturnTerminatorBuilder Instance = new();
}

internal sealed record CallTerminatorBuilder(
    FunctionSymbol Function,
    ImmutableArray<Operand> Arguments,
    Place? Destination,
    BasicBlockBuilder NextBlock,
    BasicBlockBuilder? UnwindBlock = null
) : TerminatorBuilder;

internal sealed record BranchTerminatorBuilder(
    Operand Condition,
    BasicBlockBuilder TrueBlock,
    BasicBlockBuilder FalseBlock
) : TerminatorBuilder;

internal sealed record UnreachableTerminatorBuilder : TerminatorBuilder
{
    private UnreachableTerminatorBuilder() { }

    public static readonly UnreachableTerminatorBuilder Instance = new();
}
