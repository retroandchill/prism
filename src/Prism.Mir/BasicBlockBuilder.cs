// @file BasicBlockBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Semantic.Symbols;

namespace Prism.Mir;

public sealed class BasicBlockBuilder
{
    public BasicBlockId Id { get; }

    public string Name { get; }

    private readonly List<Statement> _statements = [];
    private TerminatorBuilder? _terminator;

    internal BasicBlockBuilder(BasicBlockId id, string name)
    {
        Id = id;
        Name = name;
    }

    internal IReadOnlyList<Statement> Statements => _statements;

    internal TerminatorBuilder Terminator =>
        _terminator ?? throw new InvalidOperationException("Terminator not set");

    public void AddAssignment(Place target, RValue value)
    {
        _statements.Add(new AssignmentStatement(target, value));
    }

    public void StorageLive(Place place)
    {
        _statements.Add(new StorageLiveStatement(place));
    }

    public void AddStorageDead(Place place)
    {
        _statements.Add(new StorageDeadStatement(place));
    }

    public void AddMakeNull(Place place)
    {
        _statements.Add(new MakeNullStatement(place));
    }

    private void EnsureTerminatorNotSet()
    {
        if (_terminator is not null)
            throw new InvalidOperationException("Terminator already set");
    }

    public void SetReturn()
    {
        EnsureTerminatorNotSet();
        _terminator = ReturnTerminatorBuilder.Instance;
    }

    public void SetCallTerminator(
        FunctionSymbol function,
        ImmutableArray<Operand> arguments,
        Place? target,
        BasicBlockBuilder next,
        BasicBlockBuilder? unwind = null
    )
    {
        EnsureTerminatorNotSet();
        _terminator = new CallTerminatorBuilder(function, arguments, target, next, unwind);
    }

    public void SetBranchTerminator(
        Operand condition,
        BasicBlockBuilder whenTrue,
        BasicBlockBuilder whenFalse
    )
    {
        EnsureTerminatorNotSet();
        _terminator = new BranchTerminatorBuilder(condition, whenTrue, whenFalse);
    }

    public void SetUnreachable()
    {
        EnsureTerminatorNotSet();
        _terminator = UnreachableTerminatorBuilder.Instance;
    }
}
