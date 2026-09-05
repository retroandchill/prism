// @file MirEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.BoundTree;
using Prism.Core.Compiling;
using Prism.Core.Mappers;
using Prism.Core.Mir;
using Prism.Core.Semantic;
using Prism.Core.Symbols;

namespace Prism.Core.Codegen.Mir;

internal sealed class MirEmitter(Compilation compilation)
{
    private enum UnaryResultKind : byte
    {
        Prefix,
        Postfix,
    }

    private enum UnaryArithmeticKind : byte
    {
        Increment,
        Decrement,
    }

    private enum LogicalOperation : byte
    {
        And,
        Or,
    }

    private readonly MirTypeMapper _typeMapper = new(compilation);

    public MirModule Emit()
    {
        var globals = EmitGlobals();
        var (functions, globalInitializer) = EmitFunctions(globals);
        return new MirModule(
            compilation.AssemblyName,
            [.. globals.Values],
            functions,
            globalInitializer
        );
    }

    private OrderedDictionary<VariableSymbol, MirGlobal> EmitGlobals()
    {
        var mapping = new OrderedDictionary<VariableSymbol, MirGlobal>(
            ReferenceEqualityComparer.Instance
        );
        var symbols = compilation.GetGlobalVariables();
        var nextGlobalId = 0;
        foreach (var symbol in symbols)
        {
            mapping.Add(symbol, EmitGlobal(symbol, ref nextGlobalId));
        }

        return mapping;
    }

    private MirGlobal EmitGlobal(VariableSymbol symbol, ref int nextGlobalId)
    {
        return new MirGlobal(
            new MirGlobalId(nextGlobalId++),
            symbol.ToDisplayString(),
            _typeMapper.Map(symbol.Type),
            symbol.IsMutable,
            EmitGlobalInitializer(symbol)
        );
    }

    private MirGlobalInitializer EmitGlobalInitializer(VariableSymbol symbol)
    {
        var initializer = compilation.GetBoundInitializer(symbol);
        if (initializer is null)
            return MirNoGlobalInitializer.Instance;

        if (initializer.ConstantValue is { } constant)
        {
            return new MirConstantGlobalInitializer(
                new MirConstantValue(constant, _typeMapper.Map(symbol.Type))
            );
        }

        return MirComputedGlobalInitializer.Instance;
    }

    private (ImmutableArray<MirFunction>, MirFunctionId?) EmitFunctions(
        IReadOnlyDictionary<VariableSymbol, MirGlobal> globals
    )
    {
        var symbols = compilation.GetGlobalFunctions();
        // Add one more to account for the possibility of emitting a global initializer
        var builder = ImmutableArray.CreateBuilder<MirFunction>(symbols.Length + 1);
        var nextFunctionId = 0;

        var functionIds = new Dictionary<FunctionSymbol, MirFunctionId>(
            ReferenceEqualityComparer.Instance
        );
        foreach (var symbol in symbols)
        {
            functionIds.Add(symbol, new MirFunctionId(nextFunctionId++));
        }

        foreach (var symbol in symbols)
        {
            builder.Add(EmitFunction(symbol, functionIds, globals));
        }

        var moduleInitializer = EmitModuleInitializer(
            new MirFunctionId(nextFunctionId + 1),
            globals,
            functionIds
        );
        if (moduleInitializer is not null)
        {
            builder.Add(moduleInitializer);
        }

        return (builder.ToImmutable(), moduleInitializer?.Id);
    }

    private MirFunction? EmitModuleInitializer(
        MirFunctionId functionId,
        IReadOnlyDictionary<VariableSymbol, MirGlobal> globals,
        Dictionary<FunctionSymbol, MirFunctionId> functionIds
    )
    {
        MirFunctionBuilder? builder = null;
        MirEmissionContext? context = null;
        foreach (
            var (symbol, global) in globals.Where(g =>
                g.Value.Initializer is MirComputedGlobalInitializer
            )
        )
        {
            builder ??= new MirFunctionBuilder(
                functionId,
                $"{compilation.AssemblyName}_<g>ModuleInitializer",
                MirVoidType.Instance
            );
            context ??= new MirEmissionContext(builder, _typeMapper, globals, functionIds);

            var entry = context.AddBlock("entry");
            context.SetCurrentBlock(entry);
            builder.SetEntryBlock(entry.Id);

            var boundInitializer = compilation.GetBoundInitializer(symbol);
            Debug.Assert(boundInitializer is not null);
            var value = EmitExpression(boundInitializer, context);
            var place = new MirGlobalPlace(global);
            context.CurrentBlock.AddInstruction(new MirAssignInstruction(place, value));

            if (!entry.IsTerminated)
            {
                context.CurrentBlock.SetTerminator(MirReturnTerminator.Void);
            }
        }

        return builder?.Build();
    }

    private MirFunction EmitFunction(
        FunctionSymbol symbol,
        Dictionary<FunctionSymbol, MirFunctionId> functionIds,
        IReadOnlyDictionary<VariableSymbol, MirGlobal> globals
    )
    {
        var body = compilation.GetBoundBody(symbol);
        var builder = new MirFunctionBuilder(
            functionIds[symbol],
            symbol.ToDisplayString(),
            _typeMapper.Map(symbol.ReturnType)
        );

        var context = new MirEmissionContext(builder, _typeMapper, globals, functionIds);

        EmitParameters(symbol, context);

        if (body is not null)
        {
            var entry = builder.AddBlock("entry");
            builder.SetEntryBlock(entry.Id);
            context.SetCurrentBlock(entry);

            EmitStatement(body, context);

            if (!context.CurrentBlock.IsTerminated && symbol.ReturnsVoid)
            {
                context.CurrentBlock.SetTerminator(MirReturnTerminator.Void);
            }
        }
        else
        {
            var entry = builder.AddBlock("entry");
            builder.SetEntryBlock(entry.Id);
            entry.SetTerminator(MirUnreachableTerminator.Instance);
        }

        return builder.Build();
    }

    private static void EmitParameters(FunctionSymbol symbol, MirEmissionContext context)
    {
        foreach (var parameter in symbol.Parameters)
        {
            context.BindParameter(parameter);
        }
    }

    private void EmitStatement(BoundStatement statement, MirEmissionContext context)
    {
        switch (statement)
        {
            case BoundBlock boundBlock:
                EmitBlock(boundBlock, context);
                break;
            case BoundVariableDeclaration boundVariableDeclaration:
                EmitLocal(boundVariableDeclaration, context);
                break;
            case BoundExpressionStatement boundExpressionStatement:
                EmitExpressionStatement(boundExpressionStatement, context);
                break;
            case BoundReturnStatement boundReturnStatement:
                EmitReturn(boundReturnStatement, context);
                break;
            case BoundIfStatement boundIfStatement:
                EmitIfStatement(boundIfStatement, context);
                break;
            case BoundWhileStatement boundWhileStatement:
                EmitWhileStatement(boundWhileStatement, context);
                break;
            case BoundLoopStatement boundLoopStatement:
                EmitLoopStatement(boundLoopStatement, context);
                break;
            case BoundForStatement boundForStatement:
                EmitForLoop(boundForStatement, context);
                break;
            case BoundBreakStatement boundBreakStatement:
                EmitBreakStatement(boundBreakStatement, context);
                break;
            case BoundContinueStatement boundContinueStatement:
                EmitContinueStatement(boundContinueStatement, context);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(statement));
        }
    }

    private void EmitBlock(BoundBlock block, MirEmissionContext context)
    {
        foreach (var statement in block.Statements)
        {
            EmitStatement(statement, context);
        }
    }

    private void EmitLocal(BoundVariableDeclaration declaration, MirEmissionContext context)
    {
        var local = context.BindLocal(declaration.Variable);
        if (declaration.Initializer is null)
            return;

        var operand = EmitExpression(declaration.Initializer, context);
        context.CurrentBlock.AddInstruction(
            new MirAssignInstruction(new MirLocalPlace(local.Id, local.Type), operand)
        );
    }

    private void EmitExpressionStatement(
        BoundExpressionStatement expression,
        MirEmissionContext context
    )
    {
        _ = EmitExpression(expression.Expression, context);
    }

    private void EmitReturn(BoundReturnStatement statement, MirEmissionContext context)
    {
        if (statement.Expression is not null)
        {
            var result = EmitExpression(statement.Expression, context);
            context.CurrentBlock.SetTerminator(new MirReturnTerminator(result));
        }
        else
        {
            context.CurrentBlock.SetTerminator(MirReturnTerminator.Void);
        }
    }

    private void EmitIfStatement(BoundIfStatement statement, MirEmissionContext context)
    {
        var thenBlock = context.AddBlock("then.cond");
        var elseBlock = statement.ElseStatement is not null
            ? context.AddDetachedBlock("else.cond")
            : null;
        var mergeBlock = context.AddDetachedBlock("cond.merge");

        var condition = EmitExpression(statement.Condition, context);
        context.CurrentBlock.SetTerminator(
            new MirBranchTerminator(condition, thenBlock.Id, elseBlock?.Id ?? mergeBlock.Id)
        );

        var thenFallsThrough = false;
        context.SetCurrentBlock(thenBlock);
        EmitStatement(statement.ThenStatement, context);
        if (!context.CurrentBlock.IsTerminated)
        {
            context.CurrentBlock.SetTerminator(new MirGotoTerminator(mergeBlock.Id));
            thenFallsThrough = true;
        }

        var elseFallsThrough = false;
        if (elseBlock is not null)
        {
            context.AddBlock(elseBlock);
            context.SetCurrentBlock(elseBlock);
            Debug.Assert(statement.ElseStatement is not null);
            EmitStatement(statement.ElseStatement, context);
            if (!context.CurrentBlock.IsTerminated)
            {
                context.CurrentBlock.SetTerminator(new MirGotoTerminator(mergeBlock.Id));
                elseFallsThrough = true;
            }
        }

        if (!thenFallsThrough && !elseFallsThrough && statement.ElseStatement is not null)
            return;

        context.AddBlock(mergeBlock);
        context.SetCurrentBlock(mergeBlock);
    }

    private void EmitWhileStatement(BoundWhileStatement statement, MirEmissionContext context)
    {
        var loopHead = context.AddBlock("loop.head");
        var loopBody = context.AddDetachedBlock("loop.body");
        var loopTail = context.AddDetachedBlock("loop.tail");
        context.BindLoop(statement.Label, loopTail.Id, loopHead.Id);

        context.CurrentBlock.SetTerminator(new MirGotoTerminator(loopHead.Id));
        context.SetCurrentBlock(loopHead);
        var condition = EmitExpression(statement.Condition, context);
        context.CurrentBlock.SetTerminator(
            new MirBranchTerminator(condition, loopBody.Id, loopTail.Id)
        );

        context.AddBlock(loopBody);
        context.SetCurrentBlock(loopBody);
        EmitStatement(statement.Body, context);
        if (!context.CurrentBlock.IsTerminated)
        {
            context.CurrentBlock.SetTerminator(new MirGotoTerminator(loopHead.Id));
        }

        context.AddBlock(loopTail);
        context.SetCurrentBlock(loopTail);
    }

    private void EmitLoopStatement(BoundLoopStatement statement, MirEmissionContext context)
    {
        var loopHead = context.AddBlock("loop.head");
        var loopTail = context.AddDetachedBlock("loop.tail");
        context.BindLoop(statement.Label, loopTail.Id, loopHead.Id);

        context.CurrentBlock.SetTerminator(new MirGotoTerminator(loopHead.Id));
        context.SetCurrentBlock(loopHead);
        EmitStatement(statement.Body, context);
        if (!context.CurrentBlock.IsTerminated)
        {
            context.CurrentBlock.SetTerminator(new MirGotoTerminator(loopHead.Id));
        }

        context.AddBlock(loopTail);
        context.SetCurrentBlock(loopTail);
    }

    private void EmitForLoop(BoundForStatement loop, MirEmissionContext context)
    {
        if (loop.Variable is not null)
        {
            EmitLocal(loop.Variable, context);
        }

        foreach (var initializer in loop.Initializers)
        {
            EmitExpression(initializer, context);
        }

        var loopHead = context.AddBlock("loop.head");
        var loopBody = loop.Condition is not null ? context.AddDetachedBlock("loop.body") : null;
        var loopIncrement =
            loop.Incrementors.Length > 0 ? context.AddDetachedBlock("loop.increment") : null;
        var loopTail = context.AddDetachedBlock("loop.tail");
        context.BindLoop(loop.Label, loopTail.Id, loopIncrement?.Id ?? loopHead.Id);

        context.CurrentBlock.SetTerminator(new MirGotoTerminator(loopHead.Id));
        context.SetCurrentBlock(loopHead);
        if (loopBody is not null)
        {
            Debug.Assert(loop.Condition is not null);
            var condition = EmitExpression(loop.Condition, context);
            context.CurrentBlock.SetTerminator(
                new MirBranchTerminator(condition, loopBody.Id, loopTail.Id)
            );

            context.AddBlock(loopBody);
            context.SetCurrentBlock(loopBody);
        }

        EmitStatement(loop.Body, context);

        if (!context.CurrentBlock.IsTerminated)
        {
            if (loopIncrement is not null)
            {
                context.CurrentBlock.SetTerminator(new MirGotoTerminator(loopIncrement.Id));

                context.AddBlock(loopIncrement);
                context.SetCurrentBlock(loopIncrement);
                foreach (var incrementor in loop.Incrementors)
                {
                    EmitExpression(incrementor, context);
                }
            }

            context.CurrentBlock.SetTerminator(new MirGotoTerminator(loopHead.Id));
        }

        context.AddBlock(loopTail);
        context.SetCurrentBlock(loopTail);
    }

    private static void EmitBreakStatement(
        BoundBreakStatement statement,
        MirEmissionContext context
    )
    {
        var (breakTarget, _) = context.GetLoopTargets(statement.Label);
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(breakTarget));
    }

    private static void EmitContinueStatement(
        BoundContinueStatement statement,
        MirEmissionContext context
    )
    {
        var (_, continueTarget) = context.GetLoopTargets(statement.Label);
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(continueTarget));
    }

    private MirValue EmitExpression(BoundExpression expression, MirEmissionContext context)
    {
        if (expression.ConstantValue is { } constantValue)
        {
            return new MirConstantValue(constantValue, _typeMapper.Map(expression.Type));
        }

        return expression switch
        {
            BoundVariableAccess access => EmitAccess(access, context),
            BoundParameterAccess access => EmitAccess(access, context),
            BoundUnaryOperation unary => EmitOperation(unary, context),
            BoundBinaryOperation binary => EmitOperation(binary, context),
            BoundAssignmentOperation assignment => EmitAssignment(assignment, context),
            BoundConditional conditional => EmitConditional(conditional, context),
            BoundInvocation invocation => EmitCall(invocation, context),
            BoundConversion conversion => EmitConversion(conversion, context),
            BoundAddressOf addressOf => EmitAddressOf(addressOf, context),
            BoundDereference dereference => EmitDereference(dereference, context),
            BoundBadExpression => throw new InvalidOperationException(
                "Should only emit LLVM IR if the compilation is valid"
            ),
            BoundLiteral => throw new InvalidOperationException(
                "The constant folding should have grabbed this already"
            ),
            _ => throw new InvalidOperationException("We probably added a new expression type"),
        };
    }

    private static MirReadValue EmitAccess(BoundVariableAccess access, MirEmissionContext context)
    {
        if (context.TryGetLocal(access.Symbol) is { } local)
        {
            return new MirReadValue(new MirLocalPlace(local.Id, local.Type), local.Type);
        }

        var global = context.GetGlobal(access.Symbol);
        return new MirReadValue(new MirGlobalPlace(global.Id, global.Type), global.Type);
    }

    private static MirReadValue EmitAccess(BoundParameterAccess access, MirEmissionContext context)
    {
        var local = context.GetLocal(access.Symbol);
        return new MirReadValue(new MirLocalPlace(local.Id, local.Type), local.Type);
    }

    private MirValue EmitOperation(BoundUnaryOperation operation, MirEmissionContext context)
    {
        return operation.Operation switch
        {
            UnaryOperation.Identity => EmitExpression(operation.Operand, context),

            UnaryOperation.Negation => EmitSimpleUnary(operation, MirUnaryOp.Negation, context),

            UnaryOperation.LogicalNot => EmitSimpleUnary(operation, MirUnaryOp.LogicalNot, context),

            UnaryOperation.BitwiseNot => EmitSimpleUnary(operation, MirUnaryOp.BitwiseNot, context),

            UnaryOperation.PreIncrement => EmitUnaryIncrementDecrement(
                operation,
                UnaryResultKind.Prefix,
                UnaryArithmeticKind.Increment,
                context
            ),

            UnaryOperation.PreDecrement => EmitUnaryIncrementDecrement(
                operation,
                UnaryResultKind.Prefix,
                UnaryArithmeticKind.Decrement,
                context
            ),

            UnaryOperation.PostIncrement => EmitUnaryIncrementDecrement(
                operation,
                UnaryResultKind.Postfix,
                UnaryArithmeticKind.Increment,
                context
            ),

            UnaryOperation.PostDecrement => EmitUnaryIncrementDecrement(
                operation,
                UnaryResultKind.Postfix,
                UnaryArithmeticKind.Decrement,
                context
            ),

            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };
    }

    private MirReadValue EmitSimpleUnary(
        BoundUnaryOperation operation,
        MirUnaryOp mirOp,
        MirEmissionContext context
    )
    {
        var temp = context.CreateTemp(_typeMapper.Map(operation.Type));
        var place = new MirLocalPlace(temp);
        var operand = EmitExpression(operation.Operand, context);
        var resultType = _typeMapper.Map(operation.Type);
        context.CurrentBlock.AddInstruction(new MirUnaryInstruction(place, mirOp, operand));
        return new MirReadValue(place, resultType);
    }

    private MirValue EmitUnaryIncrementDecrement(
        BoundUnaryOperation operation,
        UnaryResultKind resultKind,
        UnaryArithmeticKind arithmeticKind,
        MirEmissionContext context
    )
    {
        var place = EmitPlace(operation.Operand, context);
        var valueType = _typeMapper.Map(operation.Operand.Type);

        var temp = context.CreateTemp(valueType, "old");
        var templaPlace = new MirLocalPlace(temp.Id, temp.Type);
        var oldValue = new MirReadValue(templaPlace, templaPlace.Type);
        context.CurrentBlock.AddInstruction(
            new MirAssignInstruction(templaPlace, new MirReadValue(place, place.Type))
        );

        var one = CreateUnitConstant(operation.Operand.Type, valueType);
        var binaryOp = arithmeticKind switch
        {
            UnaryArithmeticKind.Increment => MirBinaryOp.Add,
            UnaryArithmeticKind.Decrement => MirBinaryOp.Subtract,
            _ => throw new ArgumentOutOfRangeException(nameof(arithmeticKind)),
        };
        var result = context.CreateTemp(valueType, "result");
        var resultPlace = new MirLocalPlace(result);
        context.CurrentBlock.AddInstruction(
            new MirBinaryInstruction(resultPlace, binaryOp, oldValue, one)
        );
        var newValue = new MirReadValue(resultPlace, valueType);

        context.CurrentBlock.AddInstruction(new MirAssignInstruction(place, newValue));

        return resultKind switch
        {
            UnaryResultKind.Prefix => newValue,
            UnaryResultKind.Postfix => oldValue,
            _ => throw new ArgumentOutOfRangeException(nameof(resultKind)),
        };
    }

    private MirValue EmitOperation(BoundBinaryOperation operation, MirEmissionContext context)
    {
        var left = EmitExpression(operation.Left, context);
        var type = _typeMapper.Map(operation.Type);
        return EmitBinaryOperation(operation.Operation, type, left, operation.Right, context);
    }

    private MirReadValue EmitBinaryOperation(
        BinaryOperation operation,
        MirType type,
        MirValue left,
        BoundExpression right,
        MirEmissionContext context
    )
    {
        return operation switch
        {
            BinaryOperation.LogicalAnd => EmitLogicalOperation(
                LogicalOperation.And,
                type,
                left,
                right,
                context
            ),
            BinaryOperation.LogicalOr => EmitLogicalOperation(
                LogicalOperation.Or,
                type,
                left,
                right,
                context
            ),
            _ => EmitSimpleBinaryOperation(operation, type, left, right, context),
        };
    }

    private MirReadValue EmitLogicalOperation(
        LogicalOperation operation,
        MirType type,
        MirValue left,
        BoundExpression right,
        MirEmissionContext context
    )
    {
        var temp = context.CreateTemp(type);
        var place = new MirLocalPlace(temp);
        var evalNext = context.AddBlock("eval.right");
        var evalSkip = context.AddDetachedBlock("eval.skip");
        var evalMerge = context.AddDetachedBlock("eval.merge");

        var terminator = operation switch
        {
            LogicalOperation.And => new MirBranchTerminator(left, evalNext.Id, evalSkip.Id),
            LogicalOperation.Or => new MirBranchTerminator(left, evalSkip.Id, evalNext.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null),
        };
        context.CurrentBlock.SetTerminator(terminator);

        context.SetCurrentBlock(evalNext);
        context.CurrentBlock.AddInstruction(
            new MirAssignInstruction(place, EmitExpression(right, context))
        );
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(evalMerge.Id));

        context.SetCurrentBlock(evalSkip);
        context.AddBlock(evalSkip);
        context.CurrentBlock.AddInstruction(
            new MirAssignInstruction(place, CreateBoolConstant(operation == LogicalOperation.Or))
        );
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(evalMerge.Id));

        context.SetCurrentBlock(evalMerge);
        context.AddBlock(evalMerge);
        return new MirReadValue(place, type);
    }

    private MirReadValue EmitSimpleBinaryOperation(
        BinaryOperation operation,
        MirType type,
        MirValue left,
        BoundExpression right,
        MirEmissionContext context
    )
    {
        var rightValue = EmitExpression(right, context);
        var temp = context.CreateTemp(type);
        var place = new MirLocalPlace(temp.Id, temp.Type);
        context.CurrentBlock.AddInstruction(
            new MirBinaryInstruction(place, operation.ToMirBinaryOperation(), left, rightValue)
        );
        return new MirReadValue(place, type);
    }

    private MirNullValue EmitAssignment(
        BoundAssignmentOperation operation,
        MirEmissionContext context
    )
    {
        var place = EmitPlace(operation.Left, context);
        if (operation.Operation == AssignmentOperation.Simple)
        {
            context.CurrentBlock.AddInstruction(
                new MirAssignInstruction(place, EmitExpression(operation.Right, context))
            );
        }
        else
        {
            var binaryOperation = operation.Operation.ToBinaryOperation();
            var loadPlace = new MirReadValue(place, place.Type);
            var binaryValue = EmitBinaryOperation(
                binaryOperation,
                place.Type,
                loadPlace,
                operation.Right,
                context
            );
            context.CurrentBlock.AddInstruction(new MirAssignInstruction(place, binaryValue));
        }

        return MirNullValue.Instance;
    }

    private MirReadValue EmitConditional(BoundConditional operation, MirEmissionContext context)
    {
        var type = _typeMapper.Map(operation.Type);
        var temp = context.CreateTemp(type, "result");
        var place = new MirLocalPlace(temp.Id, temp.Type);

        var thenBlock = context.AddBlock("cond.then");
        var elseBlock = context.AddDetachedBlock("cond.else");
        var mergeBlock = context.AddDetachedBlock("cond.merge");

        var condition = EmitExpression(operation.Condition, context);
        context.CurrentBlock.SetTerminator(
            new MirBranchTerminator(condition, thenBlock.Id, elseBlock.Id)
        );

        context.SetCurrentBlock(thenBlock);
        context.CurrentBlock.AddInstruction(
            new MirAssignInstruction(place, EmitExpression(operation.WhenTrue, context))
        );
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(mergeBlock.Id));

        context.AddBlock(elseBlock);
        context.SetCurrentBlock(elseBlock);
        context.CurrentBlock.AddInstruction(
            new MirAssignInstruction(place, EmitExpression(operation.WhenFalse, context))
        );
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(mergeBlock.Id));

        context.AddBlock(mergeBlock);
        context.SetCurrentBlock(mergeBlock);
        return new MirReadValue(place, place.Type);
    }

    private MirValue EmitCall(BoundInvocation call, MirEmissionContext context)
    {
        MirLocalPlace? place;
        if (!call.Function.ReturnsVoid)
        {
            var returnType = _typeMapper.Map(call.Function.ReturnType);
            var local = context.CreateTemp(returnType, "result");
            place = new MirLocalPlace(local.Id, local.Type);
        }
        else
        {
            place = null;
        }

        var functionId = context.GetReferencedFunction(call.Function);
        context.CurrentBlock.AddInstruction(
            new MirCallInstruction(place, functionId, EmitExpressionList(call.Arguments, context))
        );
        return place is not null ? new MirReadValue(place, place.Type) : MirNullValue.Instance;
    }

    private ImmutableArray<MirValue> EmitExpressionList(
        ImmutableArray<BoundExpression> arguments,
        MirEmissionContext context
    )
    {
        return [.. arguments.Select(arg => EmitExpression(arg, context))];
    }

    private MirValue EmitConversion(BoundConversion conversion, MirEmissionContext context)
    {
        var value = EmitExpression(conversion.Operand, context);
        var target = _typeMapper.Map(conversion.Type);
        return EmitScalarConversion(value, conversion.Conversion, target, context);
    }

    private static MirValue EmitScalarConversion(
        MirValue operand,
        Conversion conversion,
        MirType target,
        MirEmissionContext context
    )
    {
        var source = operand.Type;
        if (source == target)
            return operand;

        if (conversion is { IsNumeric: false, IsCharacter: false })
            throw new InvalidOperationException("If we get here, the conversion is invalid");

        switch (source)
        {
            case MirIntType sourceInt when target is MirIntType targetInt:
            {
                if (sourceInt.Bits < targetInt.Bits)
                {
                    return sourceInt.Signed
                        ? EmitConversionValue(
                            operand,
                            MirConversionKind.SignExtend,
                            target,
                            context
                        )
                        : EmitConversionValue(
                            operand,
                            MirConversionKind.ZeroExtend,
                            target,
                            context
                        );
                }

                Debug.Assert(sourceInt.Bits > targetInt.Bits);
                return EmitConversionValue(operand, MirConversionKind.Truncate, target, context);
            }
            case MirFloatType sourceFloat when target is MirFloatType targetFloat:
            {
                if (sourceFloat.Bits < targetFloat.Bits)
                {
                    return EmitConversionValue(
                        operand,
                        MirConversionKind.FloatExtend,
                        target,
                        context
                    );
                }

                Debug.Assert(sourceFloat.Bits > targetFloat.Bits);
                return EmitConversionValue(
                    operand,
                    MirConversionKind.FloatTruncate,
                    target,
                    context
                );
            }
            case MirFloatType when target is MirIntType { Signed: var targetSigned }:
                return targetSigned
                    ? EmitConversionValue(
                        operand,
                        MirConversionKind.FloatToSignedInt,
                        target,
                        context
                    )
                    : EmitConversionValue(
                        operand,
                        MirConversionKind.FloatToUnsignedInt,
                        target,
                        context
                    );
            case MirIntType { Signed: var sourceSigned } when target is MirFloatType:
                return sourceSigned
                    ? EmitConversionValue(
                        operand,
                        MirConversionKind.SignedIntToFloat,
                        target,
                        context
                    )
                    : EmitConversionValue(
                        operand,
                        MirConversionKind.UnsignedIntToFloat,
                        target,
                        context
                    );
            default:
                throw new InvalidOperationException("If we get here, the conversion is invalid");
        }
    }

    private static MirReadValue EmitConversionValue(
        MirValue operand,
        MirConversionKind kind,
        MirType target,
        MirEmissionContext context
    )
    {
        var temp = context.CreateTemp(target);
        var place = new MirLocalPlace(temp);
        context.CurrentBlock.AddInstruction(new MirConvertInstruction(place, kind, operand));
        return new MirReadValue(place, target);
    }

    private MirAddressOfValue EmitAddressOf(BoundAddressOf operation, MirEmissionContext context)
    {
        var place = EmitPlace(operation.Operand, context);
        return new MirAddressOfValue(place, place.Type);
    }

    private MirReadValue EmitDereference(BoundDereference expression, MirEmissionContext context)
    {
        var place = EmitPlace(expression, context);
        return new MirReadValue(place, place.Type);
    }

    private MirPlace EmitPlace(BoundExpression expression, MirEmissionContext context)
    {
        return expression switch
        {
            BoundVariableAccess access => EmitPlace(access, context),
            BoundParameterAccess access => EmitPlace(access, context),
            BoundDereference dereference => EmitPlace(dereference, context),
            _ => throw new InvalidOperationException(
                $"Cannot emit place for expression of type {expression.GetType()}"
            ),
        };
    }

    private static MirPlace EmitPlace(BoundVariableAccess access, MirEmissionContext context)
    {
        if (context.TryGetLocal(access.Symbol) is { } local)
        {
            return new MirLocalPlace(local.Id, local.Type);
        }

        var global = context.GetGlobal(access.Symbol);
        return new MirGlobalPlace(global.Id, global.Type);
    }

    private static MirLocalPlace EmitPlace(BoundParameterAccess access, MirEmissionContext context)
    {
        var local = context.GetLocal(access.Symbol);
        return new MirLocalPlace(local.Id, local.Type);
    }

    private MirDerefPlace EmitPlace(BoundDereference dereference, MirEmissionContext context)
    {
        var pointer = EmitExpression(dereference.Operand, context);
        var type = _typeMapper.Map(dereference.Type);
        return new MirDerefPlace(pointer, type);
    }

    private static MirConstantValue CreateBoolConstant(bool value)
    {
        return new MirConstantValue(ConstantValue.Boolean(value), MirBoolType.Instance);
    }

    private static MirConstantValue CreateUnitConstant(TypeSymbol sourceType, MirType targetType)
    {
        return sourceType.SpecialType switch
        {
            SpecialType.I8 => new MirConstantValue(ConstantValue.I8(1), targetType),
            SpecialType.I16 => new MirConstantValue(ConstantValue.I16(1), targetType),
            SpecialType.I32 => new MirConstantValue(ConstantValue.I32(1), targetType),
            SpecialType.I64 => new MirConstantValue(ConstantValue.I64(1), targetType),
            SpecialType.I128 => new MirConstantValue(ConstantValue.I128(1), targetType),

            SpecialType.ISize => new MirConstantValue(ConstantValue.ISize(1), targetType),

            SpecialType.U8 => new MirConstantValue(ConstantValue.U8(1), targetType),
            SpecialType.U16 => new MirConstantValue(ConstantValue.U16(1), targetType),
            SpecialType.U32 => new MirConstantValue(ConstantValue.U32(1), targetType),
            SpecialType.U64 => new MirConstantValue(ConstantValue.U64(1), targetType),
            SpecialType.U128 => new MirConstantValue(ConstantValue.U128(1), targetType),

            SpecialType.USize => new MirConstantValue(ConstantValue.USize(1), targetType),

            SpecialType.F32 => new MirConstantValue(ConstantValue.F32(1), targetType),
            SpecialType.F64 => new MirConstantValue(ConstantValue.F64(1), targetType),

            _ => throw new InvalidOperationException(
                "Increment/decrement requires a numeric operand."
            ),
        };
    }
}
