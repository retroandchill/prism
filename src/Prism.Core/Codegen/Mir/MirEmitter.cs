// @file MirEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Prism.Core.BoundTree;
using Prism.Core.Compiling;
using Prism.Core.Mappers;
using Prism.Core.Mir;
using Prism.Core.Semantic;
using Prism.Core.Symbols;

namespace Prism.Core.Codegen.Mir;

internal sealed class MirEmitter
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

    private readonly Compilation _compilation;
    private readonly MirTypeMapper _typeMapper;

    public MirEmitter(Compilation compilation)
    {
        _compilation = compilation;
        _typeMapper = new MirTypeMapper(compilation);
    }

    public MirModule Emit()
    {
        var globals = EmitGlobals();
        var (functions, globalInitializer) = EmitFunctions(globals);
        return new MirModule(
            _compilation.AssemblyName,
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
        var symbols = _compilation.GetGlobalVariables();
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
        var initializer = _compilation.GetBoundInitializer(symbol);
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
        var symbols = _compilation.GetGlobalFunctions();
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

        return (builder.ToImmutable(), null);
    }

    private MirFunction EmitFunction(
        FunctionSymbol symbol,
        Dictionary<FunctionSymbol, MirFunctionId> functionIds,
        IReadOnlyDictionary<VariableSymbol, MirGlobal> globals
    )
    {
        var body = _compilation.GetBoundBody(symbol);
        var builder = new MirFunctionBuilder(
            functionIds[symbol],
            symbol.ToDisplayString(),
            _typeMapper.Map(symbol.ReturnType)
        );

        var context = new MirEmissionContext(
            symbol,
            body,
            builder,
            _typeMapper,
            globals,
            functionIds
        );

        EmitParameters(symbol, context);

        if (body is not null)
        {
            var entry = builder.AddBlock("entry");
            builder.SetEntryBlock(entry.Id);
            context.SetCurrentBlock(entry);

            EmitStatement(body, context);

            if (!entry.IsTerminated && symbol.ReturnsVoid)
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
        context.CurrentBlock.AddInstruction(
            new MirEvalInstruction(EmitExpression(expression.Expression, context))
        );
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
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(loopHead.Id));
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

    private MirUnaryValue EmitSimpleUnary(
        BoundUnaryOperation operation,
        MirUnaryOp mirOp,
        MirEmissionContext context
    )
    {
        var operand = EmitExpression(operation.Operand, context);
        var resultType = _typeMapper.Map(operation.Type);

        return new MirUnaryValue(mirOp, operand, resultType);
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
        var oldValue = new MirReadValue(place, place.Type);
        context.CurrentBlock.AddInstruction(
            new MirAssignInstruction(new MirLocalPlace(temp.Id, temp.Type), oldValue)
        );

        var one = CreateUnitConstant(operation.Operand.Type, valueType);
        var binaryOp = arithmeticKind switch
        {
            UnaryArithmeticKind.Increment => MirBinaryOp.Add,
            UnaryArithmeticKind.Decrement => MirBinaryOp.Subtract,
            _ => throw new ArgumentOutOfRangeException(nameof(arithmeticKind)),
        };
        var newValue = new MirBinaryValue(binaryOp, oldValue, one, valueType);

        context.CurrentBlock.AddInstruction(new MirAssignInstruction(place, newValue));

        return resultKind switch
        {
            UnaryResultKind.Prefix => newValue,
            UnaryResultKind.Postfix => oldValue,
            _ => throw new ArgumentOutOfRangeException(nameof(resultKind)),
        };
    }

    private MirBinaryValue EmitOperation(BoundBinaryOperation operation, MirEmissionContext context)
    {
        var left = EmitExpression(operation.Left, context);
        var right = EmitExpression(operation.Right, context);
        var type = _typeMapper.Map(operation.Type);
        var mirOp = operation.Operation.ToMirBinaryOperation();
        return new MirBinaryValue(mirOp, left, right, type);
    }

    private MirNullValue EmitAssignment(
        BoundAssignmentOperation operation,
        MirEmissionContext context
    )
    {
        var place = EmitPlace(operation.Left, context);
        var value = EmitExpression(operation.Right, context);
        if (operation.Operation == AssignmentOperation.Simple)
        {
            context.CurrentBlock.AddInstruction(new MirAssignInstruction(place, value));
        }
        else
        {
            var binaryOperation = operation.Operation.ToMirBinaryOperation();
            var loadPlace = new MirReadValue(place, place.Type);
            var binaryValue = new MirBinaryValue(binaryOperation, loadPlace, value, place.Type);
            context.CurrentBlock.AddInstruction(new MirAssignInstruction(place, binaryValue));
        }

        return MirNullValue.Instance;
    }

    private MirValue EmitConditional(BoundConditional operation, MirEmissionContext context)
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
        return EmitScalarConversion(value, conversion.Conversion, target);
    }

    private static MirValue EmitScalarConversion(
        MirValue operand,
        Conversion conversion,
        MirType target
    )
    {
        var source = operand.Type;
        if (source == target)
            return operand;

        if (conversion.IsNumeric || conversion.IsCharacter)
        {
            if (source is MirIntType sourceInt && target is MirIntType targetInt)
            {
                if (sourceInt.Bits < targetInt.Bits)
                {
                    return sourceInt.Signed
                        ? new MirConvertValue(operand, MirConversionKind.SignExtend, target)
                        : new MirConvertValue(operand, MirConversionKind.ZeroExtend, target);
                }

                Debug.Assert(sourceInt.Bits > targetInt.Bits);
                return new MirConvertValue(operand, MirConversionKind.Truncate, target);
            }

            if (source is MirFloatType sourceFloat && target is MirFloatType targetFloat)
            {
                if (sourceFloat.Bits < targetFloat.Bits)
                {
                    return new MirConvertValue(operand, MirConversionKind.FloatExtend, target);
                }

                Debug.Assert(sourceFloat.Bits > targetFloat.Bits);
                return new MirConvertValue(operand, MirConversionKind.FloatTruncate, target);
            }

            if (source is MirFloatType && target is MirIntType { Signed: var targetSigned })
            {
                return targetSigned
                    ? new MirConvertValue(operand, MirConversionKind.FloatToSignedInt, target)
                    : new MirConvertValue(operand, MirConversionKind.FloatToUnsignedInt, target);
            }

            if (source is MirIntType { Signed: var sourceSigned } && target is MirFloatType)
            {
                return sourceSigned
                    ? new MirConvertValue(operand, MirConversionKind.SignedIntToFloat, target)
                    : new MirConvertValue(operand, MirConversionKind.UnsignedIntToFloat, target);
            }
        }

        throw new InvalidOperationException("If we get here, the conversion is invalid");
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
