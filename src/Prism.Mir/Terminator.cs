// @file Terminator.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Cysharp.Text;
using Prism.Semantic.Symbols;

namespace Prism.Mir;

public closed record Terminator
{
    public abstract override string ToString();
}

public sealed record ReturnTerminator : Terminator
{
    private ReturnTerminator() { }

    public static readonly ReturnTerminator Instance = new();

    public override string ToString() => "Return";
}

public sealed record CallTerminator(
    FunctionSymbol Function,
    ImmutableArray<Operand> Arguments,
    Place? Destination,
    BasicBlock NextBlock,
    BasicBlock? UnwindBlock = null
) : Terminator
{
    public override string ToString()
    {
        var destination = Destination is not null ? $"{Destination} = " : "";
        var unwind = UnwindBlock is not null ? $"; unwind {UnwindBlock}" : "";

        return $"{destination}call {Function.ToDisplayString()}({string.Join(", ", Arguments)}); then {NextBlock}{unwind}";
    }
}

public sealed record BranchTerminator(
    Operand Condition,
    BasicBlock TrueBlock,
    BasicBlock FalseBlock
) : Terminator
{
    public override string ToString()
    {
        return $"if {Condition} then {TrueBlock} else {FalseBlock}";
    }
}

public sealed record UnreachableTerminator : Terminator
{
    private UnreachableTerminator() { }

    public static readonly UnreachableTerminator Instance = new();

    public override string ToString() => "Unreachable";
}
