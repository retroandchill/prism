// @file ControlFlowGraphBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.BoundTree;
using Prism.Core.Symbols;

namespace Prism.Core.FlowAnalysis;

internal sealed class ControlFlowGraphBuilder
{
    private readonly List<ControlFlowBasicBlock> _blocks = [];
    private readonly Stack<LoopContext> _loopStack = [];

    private readonly ImmutableHashSet<Symbol>.Builder _addressedLocals =
        ImmutableHashSet.CreateBuilder<Symbol>(ReferenceEqualityComparer.Instance);

    private ControlFlowBasicBlock _entry = null!;
    private ControlFlowBasicBlock _exit = null!;
    private ControlFlowBasicBlock? _current;
    private int _nextOrdinal;

    private readonly record struct LoopContext(
        LabelSymbol Label,
        ControlFlowBasicBlock BreakTarget,
        ControlFlowBasicBlock ContinueTarget
    );

    private ControlFlowGraphBuilder() { }

    public static ControlFlowGraph Build(BoundBody body)
    {
        var builder = new ControlFlowGraphBuilder();
        return builder.BuildCore(body.TopLevelStatement);
    }

    private ControlFlowGraph BuildCore(BoundStatement statement)
    {
        _entry = NewBlock(isEntry: true);
        _exit = NewBlock(isExit: true);

        _current = _entry;

        var firstBodyBlock = NewBlock();

        Connect(_entry, firstBodyBlock, ControlFlowBranchKind.FallThrough);
        _current = firstBodyBlock;

        BuildStatement(statement);

        if (_current is not null)
        {
            Connect(_current, _exit, ControlFlowBranchKind.FallThrough);
            _current = null;
        }

        return new ControlFlowGraph(_entry, _exit, [.. _blocks], _addressedLocals.ToImmutable());
    }

    private ControlFlowBasicBlock NewBlock(bool isEntry = false, bool isExit = false)
    {
        var block = new ControlFlowBasicBlock(_nextOrdinal++, isEntry, isExit);
        _blocks.Add(block);
        return block;
    }

    private ControlFlowBasicBlock EnsureCurrentBlock()
    {
        if (_current is not null)
            return _current;

        _current = NewBlock();
        return _current;
    }

    private static void Connect(
        ControlFlowBasicBlock from,
        ControlFlowBasicBlock to,
        ControlFlowBranchKind kind,
        BoundExpression? condition = null
    )
    {
        var edge = new ControlFlowEdge(from, to, kind, condition);
        from.Outgoing.Add(edge);
        to.Incoming.Add(edge);
    }

    private void TerminateCurrentBlock(
        ControlFlowBasicBlock target,
        ControlFlowBranchKind kind,
        BoundExpression? condition = null
    )
    {
        var current = EnsureCurrentBlock();
        Connect(current, target, kind, condition);
        _current = null;
    }

    private void AppendStatement(BoundStatement statement)
    {
        EnsureCurrentBlock().Statements.Add(statement);
    }

    private void BuildStatement(BoundStatement statement)
    {
        switch (statement)
        {
            case BoundBlock block:
                BuildBlock(block);
                break;
            case BoundVariableDeclaration declaration:
                AppendStatement(declaration);
                CollectAddressedLocals(declaration.Initializer);
                break;
            case BoundExpressionStatement expressionStatement:
                AppendStatement(expressionStatement);
                CollectAddressedLocals(expressionStatement.Expression);
                break;
            case BoundReturnStatement returnStatement:
                BuildReturn(returnStatement);
                break;
            case BoundIfStatement ifStatement:
                BuildIf(ifStatement);
                break;
            case BoundWhileStatement whileStatement:
                BuildWhile(whileStatement);
                break;
            case BoundLoopStatement loopStatement:
                BuildLoop(loopStatement);
                break;
            case BoundForStatement forStatement:
                BuildFor(forStatement);
                break;
            case BoundBreakStatement breakStatement:
                BuildBreak(breakStatement);
                break;
            case BoundContinueStatement continueStatement:
                BuildContinue(continueStatement);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(statement));
        }
    }

    private void BuildBlock(BoundBlock block)
    {
        foreach (var statement in block.Statements)
        {
            BuildStatement(statement);
        }
    }

    private void BuildReturn(BoundReturnStatement statement)
    {
        AppendStatement(statement);
        CollectAddressedLocals(statement.Expression);
        TerminateCurrentBlock(_exit, ControlFlowBranchKind.Return);
    }

    private void BuildIf(BoundIfStatement statement)
    {
        var conditionBlock = EnsureCurrentBlock();
        var thenBlock = NewBlock();
        var mergeBlock = NewBlock();
        var elseBlock = statement.ElseStatement is not null ? NewBlock() : null;

        CollectAddressedLocals(statement.Condition);

        Connect(
            conditionBlock,
            thenBlock,
            ControlFlowBranchKind.ConditionalTrue,
            statement.Condition
        );
        Connect(
            conditionBlock,
            elseBlock ?? mergeBlock,
            ControlFlowBranchKind.ConditionalFalse,
            statement.Condition
        );

        _current = thenBlock;
        BuildStatement(statement.ThenStatement);
        if (_current is not null)
        {
            Connect(_current, mergeBlock, ControlFlowBranchKind.FallThrough);
        }

        if (elseBlock is not null)
        {
            _current = elseBlock;
            BuildStatement(statement.ElseStatement!);
            if (_current is not null)
            {
                Connect(_current, mergeBlock, ControlFlowBranchKind.FallThrough);
            }
        }

        _current = mergeBlock;
    }

    private void BuildWhile(BoundWhileStatement statement)
    {
        var preHeader = EnsureCurrentBlock();
        var headerBlock = NewBlock();
        var bodyBlock = NewBlock();
        var exitBlock = NewBlock();

        Connect(preHeader, headerBlock, ControlFlowBranchKind.FallThrough);

        _loopStack.Push(new LoopContext(statement.Label, exitBlock, headerBlock));

        CollectAddressedLocals(statement.Condition);

        Connect(headerBlock, bodyBlock, ControlFlowBranchKind.ConditionalTrue, statement.Condition);
        Connect(
            headerBlock,
            exitBlock,
            ControlFlowBranchKind.ConditionalFalse,
            statement.Condition
        );

        _current = bodyBlock;
        BuildStatement(statement.Body);
        if (_current is not null)
        {
            Connect(_current, headerBlock, ControlFlowBranchKind.FallThrough);
        }

        _loopStack.Pop();
        _current = exitBlock;
    }

    private void BuildLoop(BoundLoopStatement statement)
    {
        var preHeader = EnsureCurrentBlock();
        var headerBlock = NewBlock();
        var exitBlock = NewBlock();

        Connect(preHeader, headerBlock, ControlFlowBranchKind.FallThrough);

        _loopStack.Push(new LoopContext(statement.Label, exitBlock, headerBlock));

        _current = headerBlock;
        BuildStatement(statement.Body);
        if (_current is not null)
        {
            Connect(_current, headerBlock, ControlFlowBranchKind.FallThrough);
        }

        _loopStack.Pop();
        _current = exitBlock;
    }

    private void BuildFor(BoundForStatement statement)
    {
        if (statement.Variable is not null)
        {
            AppendStatement(statement.Variable);
            CollectAddressedLocals(statement.Variable.Initializer);
        }

        foreach (var initializer in statement.Initializers)
        {
            AppendStatement(new BoundExpressionStatement(initializer.Syntax, initializer));
            CollectAddressedLocals(initializer);
        }

        var preHeader = EnsureCurrentBlock();
        var headerBlock = NewBlock();
        var bodyBlock = NewBlock();
        var incrementBlock = statement.Incrementors.Length > 0 ? NewBlock() : headerBlock;
        var exitBlock = NewBlock();

        Connect(preHeader, headerBlock, ControlFlowBranchKind.FallThrough);

        _loopStack.Push(new LoopContext(statement.Label, exitBlock, incrementBlock));

        if (statement.Condition is not null)
        {
            CollectAddressedLocals(statement.Condition);
            Connect(
                headerBlock,
                bodyBlock,
                ControlFlowBranchKind.ConditionalTrue,
                statement.Condition
            );
            Connect(
                headerBlock,
                exitBlock,
                ControlFlowBranchKind.ConditionalFalse,
                statement.Condition
            );
        }
        else
        {
            Connect(headerBlock, bodyBlock, ControlFlowBranchKind.FallThrough);
        }

        _current = bodyBlock;
        BuildStatement(statement.Body);

        if (_current is not null)
        {
            if (!ReferenceEquals(incrementBlock, headerBlock))
            {
                Connect(_current, incrementBlock, ControlFlowBranchKind.FallThrough);
                _current = incrementBlock;

                foreach (var incrementor in statement.Incrementors)
                {
                    AppendStatement(new BoundExpressionStatement(incrementor.Syntax, incrementor));
                    CollectAddressedLocals(incrementor);
                }
            }

            Connect(EnsureCurrentBlock(), headerBlock, ControlFlowBranchKind.FallThrough);
        }

        _loopStack.Pop();
        _current = exitBlock;
    }

    private void BuildBreak(BoundBreakStatement statement)
    {
        AppendStatement(statement);

        var target = LookupLoop(statement.Label).BreakTarget;
        TerminateCurrentBlock(target, ControlFlowBranchKind.Jump);
    }

    private void BuildContinue(BoundContinueStatement statement)
    {
        AppendStatement(statement);

        var target = LookupLoop(statement.Label).ContinueTarget;
        TerminateCurrentBlock(target, ControlFlowBranchKind.Jump);
    }

    private LoopContext LookupLoop(LabelSymbol label)
    {
        foreach (var loop in _loopStack)
        {
            if (ReferenceEquals(loop.Label, label))
                return loop;
        }

        throw new InvalidOperationException("Loop label not found");
    }

    private void CollectAddressedLocals(BoundExpression? expression)
    {
        if (expression is null)
            return;

        switch (expression)
        {
            case BoundAddressOf addressOf:
                if (TryGetAddressedLocal(addressOf.Operand) is { } local)
                {
                    _addressedLocals.Add(local);
                }

                CollectAddressedLocals(addressOf.Operand);
                break;

            case BoundUnaryOperation unary:
                CollectAddressedLocals(unary.Operand);
                break;

            case BoundBinaryOperation binary:
                CollectAddressedLocals(binary.Left);
                CollectAddressedLocals(binary.Right);
                break;

            case BoundAssignmentOperation assignment:
                CollectAddressedLocals(assignment.Left);
                CollectAddressedLocals(assignment.Right);
                break;

            case BoundConditional conditional:
                CollectAddressedLocals(conditional.Condition);
                CollectAddressedLocals(conditional.WhenTrue);
                CollectAddressedLocals(conditional.WhenFalse);
                break;

            case BoundInvocation invocation:
                foreach (var argument in invocation.Arguments)
                {
                    CollectAddressedLocals(argument);
                }
                break;

            case BoundConversion conversion:
                CollectAddressedLocals(conversion.Operand);
                break;

            case BoundDereference dereference:
                CollectAddressedLocals(dereference.Operand);
                break;

            case BoundIndex index:
                CollectAddressedLocals(index.Operand);
                CollectAddressedLocals(index.Index);
                break;
        }
    }

    private static VariableSymbol? TryGetAddressedLocal(BoundExpression expression)
    {
        return expression switch
        {
            BoundVariableAccess variableAccess => variableAccess.Symbol,
            _ => null,
        };
    }
}
