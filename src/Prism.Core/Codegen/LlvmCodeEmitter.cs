// @file LlvmCodeEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using LLVMSharp.Interop;
using Prism.Core.BoundTree;
using Prism.Core.Compiling;
using Prism.Core.Configuration;
using Prism.Core.Mappers;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using ZLinq;

namespace Prism.Core.Codegen;

internal sealed class LlvmCodeEmitter : IDisposable
{
    private enum UnaryReturnType : byte
    {
        Prefix,
        Postfix,
    }

    private enum UnaryArithmeticType : byte
    {
        Increment,
        Decrement,
    }

    private readonly Compilation _compilation;
    private readonly CodeGenOptions _options;

    private LLVMContextRef _context;
    private LLVMModuleRef _module;
    private LLVMBuilderRef _builder;

    private readonly Dictionary<Symbol, LLVMValueRef> _symbolToValue = new(
        ReferenceEqualityComparer.Instance
    );

    private readonly Dictionary<TypeSymbol, LLVMTypeRef> _symbolToType = new(
        ReferenceEqualityComparer.Instance
    );

    public LlvmCodeEmitter(Compilation compilation, CodeGenOptions options)
    {
        _compilation = compilation;
        _options = options;
        _context = LLVMContextRef.Create();
        _module = _context.CreateModuleWithName(compilation.AssemblyName);
        _builder = _context.CreateBuilder();
    }

    public EmitResult Emit()
    {
        foreach (var global in _compilation.GetGlobalVariables())
        {
            GetOrCreateGlobal(global);
        }

        foreach (var function in _compilation.GetGlobalFunctions())
        {
            GetOrCreateFunction(function);
        }

        FunctionEmissionContext? assemblyInitializerContext = null;
        foreach (var global in _compilation.GetGlobalVariables())
        {
            EmitGlobalInitializer(global, ref assemblyInitializerContext);
        }

        if (assemblyInitializerContext is not null)
        {
            _builder.BuildRetVoid();
            _module.AppendToGlobalCtors(assemblyInitializerContext.Function, 65535);
        }

        foreach (var function in _compilation.GetGlobalFunctions())
        {
            EmitFunctionBody(function);
        }

        if (_compilation.Settings.IsApplication && !EmitEntryPoint())
        {
            return new EmitResult(false, []);
        }

        WriteIR();
        return OutputBinary();
    }

    private bool EmitEntryPoint()
    {
        Debug.Assert(_compilation.Settings.IsApplication);
        var entryPoint = _compilation.GetEntryPoint();
        if (entryPoint is null)
            return false;

        var functionType = LLVMTypeRef.CreateFunction(_context.Int32Type, []);
        var mainFunc = _module.AddFunction("main", functionType);
        var entry = mainFunc.AppendBasicBlock("entry");
        _builder.PositionAtEnd(entry);

        var targetFunction = GetOrCreateFunction(entryPoint);
        var callEntryPoint = _builder.BuildCall2(targetFunction.FunctionType, targetFunction, []);
        _builder.BuildRet(
            entryPoint.ReturnsVoid
                ? LLVMValueRef.CreateConstInt(_context.Int32Type, 0)
                : callEntryPoint
        );

        return true;
    }

    private LLVMValueRef GetOrCreateFunction(FunctionSymbol functionSymbol)
    {
        if (_symbolToValue.TryGetValue(functionSymbol, out var function))
        {
            return function;
        }

        var returnType = GetOrCreateType(functionSymbol.ReturnType);
        using var parameters = functionSymbol
            .Parameters.AsValueEnumerable()
            .Select(p => GetOrCreateType(p.Type))
            .ToArrayPool();

        var name = functionSymbol.Mangle();
        var functionType = LLVMTypeRef.CreateFunction(returnType, parameters.Span, false);
        var func = _module.AddFunction(name, functionType);
        _symbolToValue[functionSymbol] = func;
        return func;
    }

    private LLVMValueRef GetOrCreateGlobal(VariableSymbol symbol)
    {
        if (_symbolToValue.TryGetValue(symbol, out var global))
        {
            return global;
        }

        var type = GetOrCreateType(symbol.Type);

        var name = symbol.Mangle();
        var variable = _module.AddGlobal(type, name);
        _symbolToValue[symbol] = variable;
        return variable;
    }

    private void EmitGlobalInitializer(
        VariableSymbol symbol,
        ref FunctionEmissionContext? assemblyInitializerContext
    )
    {
        var variable = GetOrCreateGlobal(symbol);
        if (!symbol.HasInitializer)
            return;

        var initializer = _compilation.GetBoundInitializer(symbol);
        if (initializer is null)
            return;

        if (initializer.ConstantValue is { } constant)
        {
            variable.Initializer = MakeConstant(constant);
            return;
        }

        if (assemblyInitializerContext is null)
        {
            var functionType = LLVMTypeRef.CreateFunction(_context.VoidType, [], false);
            var initializerName = $"{_compilation.AssemblyName}_<GlobalInitializer>";
            var assemblyInitializer = _module.AddFunction(initializerName, functionType);
            assemblyInitializer.Linkage = LLVMLinkage.LLVMInternalLinkage;
            var block = assemblyInitializer.AppendBasicBlock("entry");

            assemblyInitializerContext = new FunctionEmissionContext(assemblyInitializer);
            _builder.PositionAtEnd(block);
        }

        var initializedValue = EmitExpression(initializer, assemblyInitializerContext);
        _builder.BuildStore(initializedValue, variable);
    }

    private void EmitFunctionBody(FunctionSymbol symbol)
    {
        var function = GetOrCreateFunction(symbol);
        var body = _compilation.GetBoundBody(symbol);
        if (body is null)
        {
            function.Linkage = LLVMLinkage.LLVMAvailableExternallyLinkage;
            return;
        }

        var entry = function.AppendBasicBlock("entry");
        _builder.PositionAtEnd(entry);
        var context = new FunctionEmissionContext(function);

        foreach (
            var (symbolParam, llvmParam) in symbol
                .Parameters.AsValueEnumerable()
                .Zip(function.GetParams())
        )
        {
            if (symbolParam.IsMutable)
            {
                var slot = CreateEntryAlloca(llvmParam.TypeOf, symbolParam.Name, context);
                _builder.BuildStore(llvmParam, slot);
                context.BindStorage(symbolParam, slot);
            }
            else
            {
                context.BindStorage(symbolParam, llvmParam);
            }
        }
        EmitStatement(body, context);

        if (symbol.ReturnsVoid)
        {
            _builder.BuildRetVoid();
        }
    }

    private LLVMTypeRef GetOrCreateType(TypeSymbol symbol)
    {
        if (_symbolToType.TryGetValue(symbol, out var type))
        {
            return type;
        }

        type = CreateType(symbol);
        _symbolToType[symbol] = type;
        return type;
    }

    private LLVMTypeRef CreateType(TypeSymbol symbol)
    {
        return symbol.SpecialType switch
        {
            SpecialType.Void => _context.VoidType,
            SpecialType.Bool or SpecialType.I8 or SpecialType.U8 or SpecialType.Char =>
                _context.Int8Type,
            SpecialType.I16 or SpecialType.U16 or SpecialType.Char16 => _context.Int16Type,
            SpecialType.I32 or SpecialType.U32 or SpecialType.Rune => _context.Int32Type,
            SpecialType.I64 or SpecialType.U64 => _context.Int64Type,
            SpecialType.I128 or SpecialType.U128 => _context.Int128Type,
            SpecialType.ISize or SpecialType.USize => _compilation.Settings.PointerWidth switch
            {
                PointerWidth.X32 => _context.Int32Type,
                PointerWidth.X64 => _context.Int64Type,
                _ => throw new InvalidOperationException("Invalid pointer width"),
            },
            SpecialType.F32 => _context.FloatType,
            SpecialType.F64 => _context.DoubleType,
            _ => throw new NotImplementedException(),
        };
    }

    private LLVMValueRef MakeConstant(in ConstantValue value)
    {
        return value.Kind switch
        {
            ConstantKind.Bool => LLVMValueRef.CreateConstInt(
                _context.Int8Type,
                value.AsBoolean() ? 1UL : 0UL
            ),
            ConstantKind.Char => LLVMValueRef.CreateConstInt(
                _context.Int8Type,
                (ulong)value.AsCharacter().Value
            ),
            ConstantKind.Char16 => LLVMValueRef.CreateConstInt(
                _context.Int16Type,
                (ulong)value.AsCharacter().Value
            ),
            ConstantKind.Rune => LLVMValueRef.CreateConstInt(
                _context.Int32Type,
                (ulong)value.AsCharacter().Value
            ),
            ConstantKind.I8 => LLVMValueRef.CreateConstInt(
                _context.Int8Type,
                unchecked((ulong)value.AsInt64()),
                true
            ),
            ConstantKind.I16 => LLVMValueRef.CreateConstInt(
                _context.Int16Type,
                unchecked((ulong)value.AsInt64()),
                true
            ),
            ConstantKind.I32 => LLVMValueRef.CreateConstInt(
                _context.Int32Type,
                unchecked((ulong)value.AsInt64()),
                true
            ),
            ConstantKind.I64 => LLVMValueRef.CreateConstInt(
                _context.Int64Type,
                unchecked((ulong)value.AsInt64()),
                true
            ),
            ConstantKind.I128 => LLVMValueRef.CreateConstInt(_context.Int128Type, value.AsInt128()),
            ConstantKind.ISize => LLVMValueRef.CreateConstInt(
                _compilation.Settings.PointerWidth switch
                {
                    PointerWidth.X32 => _context.Int32Type,
                    PointerWidth.X64 => _context.Int64Type,
                    _ => throw new InvalidOperationException("Invalid pointer width"),
                },
                value.AsInt64()
            ),
            ConstantKind.U8 => LLVMValueRef.CreateConstInt(_context.Int8Type, value.AsUInt64()),
            ConstantKind.U16 => LLVMValueRef.CreateConstInt(_context.Int16Type, value.AsUInt64()),
            ConstantKind.U32 => LLVMValueRef.CreateConstInt(_context.Int32Type, value.AsUInt64()),
            ConstantKind.U64 => LLVMValueRef.CreateConstInt(_context.Int64Type, value.AsUInt64()),
            ConstantKind.U128 => LLVMValueRef.CreateConstInt(_context.Int64Type, value.AsUInt128()),
            ConstantKind.USize => LLVMValueRef.CreateConstInt(
                _compilation.Settings.PointerWidth switch
                {
                    PointerWidth.X32 => _context.Int32Type,
                    PointerWidth.X64 => _context.Int64Type,
                    _ => throw new InvalidOperationException("Invalid pointer width"),
                },
                value.AsUInt64()
            ),
            ConstantKind.F32 => LLVMValueRef.CreateConstReal(_context.FloatType, value.AsFloat32()),
            ConstantKind.F64 => LLVMValueRef.CreateConstReal(
                _context.DoubleType,
                value.AsFloat64()
            ),
            ConstantKind.Str => _builder.BuildGlobalString(value.AsString()),
            _ => throw new ArgumentException("Invalid constant kind"),
        };
    }

    private LLVMValueRef ConvertByteBoolToI1IfNeeded(LLVMValueRef value)
    {
        return value.TypeOf.IntWidth == 8
            ? _builder.BuildICmp(
                LLVMIntPredicate.LLVMIntNE,
                value,
                LLVMValueRef.CreateConstInt(_context.Int8Type, 0)
            )
            : value;
    }

    private void EmitStatement(BoundStatement statement, FunctionEmissionContext context)
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

    private void EmitBlock(BoundBlock block, FunctionEmissionContext context)
    {
        context.PushScope();

        foreach (var statement in block.Statements)
        {
            EmitStatement(statement, context);
        }

        context.PopScope();
    }

    private void EmitLocal(BoundVariableDeclaration declaration, FunctionEmissionContext context)
    {
        var symbol = declaration.Variable;
        if (!symbol.IsMutable && declaration.Initializer is not null)
        {
            var value = EmitExpression(declaration.Initializer, context);
            context.BindStorage(symbol, value);
            return;
        }

        var type = GetOrCreateType(symbol.Type);
        var slot = CreateEntryAlloca(type, symbol.Name, context);

        context.BindStorage(symbol, slot);

        if (declaration.Initializer is null)
            return;

        _builder.BuildStore(EmitExpression(declaration.Initializer, context), slot);
    }

    private void EmitExpressionStatement(
        BoundExpressionStatement statement,
        FunctionEmissionContext context
    )
    {
        _ = EmitExpression(statement.Expression, context);
    }

    private void EmitReturn(BoundReturnStatement returnStatement, FunctionEmissionContext context)
    {
        if (returnStatement.Expression is null)
        {
            _builder.BuildRetVoid();
            return;
        }

        var expression = EmitExpression(returnStatement.Expression, context);
        _builder.BuildRet(expression);
    }

    private void EmitIfStatement(BoundIfStatement statement, FunctionEmissionContext context)
    {
        var function = context.Function;
        var thenBlock = function.AppendBasicBlock("cond.then");
        var elseBlock = statement.ElseStatement is not null
            ? LLVMBasicBlockRef.CreateInContext(_context, "cond.else")
            : (LLVMBasicBlockRef?)null;
        var mergeBlock = LLVMBasicBlockRef.CreateInContext(_context, "cond.merge");

        var condition = ConvertByteBoolToI1IfNeeded(EmitExpression(statement.Condition, context));
        _builder.BuildCondBr(condition, thenBlock, elseBlock ?? mergeBlock);

        _builder.PositionAtEnd(thenBlock);
        EmitStatement(statement.ThenStatement, context);
        _builder.BuildBr(mergeBlock);

        if (elseBlock is not null)
        {
            function.AppendExistingBasicBlock(elseBlock.Value);
            _builder.PositionAtEnd(elseBlock.Value);
            Debug.Assert(statement.ElseStatement is not null);
            EmitStatement(statement.ElseStatement, context);
            _builder.BuildBr(mergeBlock);
        }

        function.AppendExistingBasicBlock(mergeBlock);
        _builder.PositionAtEnd(mergeBlock);
    }

    private void EmitWhileStatement(BoundWhileStatement statement, FunctionEmissionContext context)
    {
        var function = context.Function;
        var loopHead = function.AppendBasicBlock("loop.head");
        var loopBody = LLVMBasicBlockRef.CreateInContext(_context, "loop.body");
        var loopTail = LLVMBasicBlockRef.CreateInContext(_context, "loop.tail");
        context.PushScope();
        context.BindLabel(statement.Label, loopTail, loopHead);

        _builder.BuildBr(loopHead);
        _builder.PositionAtEnd(loopHead);
        var condition = ConvertByteBoolToI1IfNeeded(EmitExpression(statement.Condition, context));
        _builder.BuildCondBr(condition, loopBody, loopTail);

        function.AppendExistingBasicBlock(loopBody);
        _builder.PositionAtEnd(loopBody);
        EmitStatement(statement.Body, context);
        _builder.BuildBr(loopHead);

        function.AppendExistingBasicBlock(loopTail);
        _builder.PositionAtEnd(loopTail);
        context.PopScope();
    }

    private void EmitLoopStatement(BoundLoopStatement loop, FunctionEmissionContext context)
    {
        context.PushScope();
        var function = context.Function;
        var loopHead = function.AppendBasicBlock("loop.head");
        var loopTail = LLVMBasicBlockRef.CreateInContext(_context, "loop.tail");
        context.BindLabel(loop.Label, loopTail, loopHead);

        _builder.BuildBr(loopHead);
        _builder.PositionAtEnd(loopHead);
        EmitStatement(loop.Body, context);
        _builder.BuildBr(loopHead);

        function.AppendExistingBasicBlock(loopTail);
        _builder.PositionAtEnd(loopTail);
        context.PopScope();
    }

    private void EmitForLoop(BoundForStatement loop, FunctionEmissionContext context)
    {
        context.PushScope();
        if (loop.Variable is not null)
        {
            EmitLocal(loop.Variable, context);
        }
        foreach (var initializer in loop.Initializers)
        {
            EmitExpression(initializer, context);
        }

        var function = context.Function;
        var loopHead = function.AppendBasicBlock("loop.head");
        var loopBody = loop.Condition is not null
            ? LLVMBasicBlockRef.CreateInContext(_context, "loop.body")
            : (LLVMBasicBlockRef?)null;
        var loopTail = LLVMBasicBlockRef.CreateInContext(_context, "loop.tail");
        context.BindLabel(loop.Label, loopTail, loopHead);

        _builder.BuildBr(loopHead);
        _builder.PositionAtEnd(loopHead);
        if (loopBody is not null)
        {
            Debug.Assert(loop.Condition is not null);
            var condition = ConvertByteBoolToI1IfNeeded(EmitExpression(loop.Condition, context));
            _builder.BuildCondBr(condition, loopBody.Value, loopTail);

            function.AppendExistingBasicBlock(loopBody.Value);
            _builder.PositionAtEnd(loopBody.Value);
        }

        EmitStatement(loop.Body, context);
        foreach (var incrementor in loop.Incrementors)
        {
            EmitExpression(incrementor, context);
        }

        _builder.BuildBr(loopHead);

        function.AppendExistingBasicBlock(loopTail);
        _builder.PositionAtEnd(loopTail);
        context.PopScope();
    }

    private void EmitBreakStatement(BoundBreakStatement statement, FunctionEmissionContext context)
    {
        var (breakLabel, _) =
            context.LookupLabels(statement.Label)
            ?? throw new InvalidOperationException("This shouldn't happen");
        _builder.BuildBr(breakLabel);
    }

    private void EmitContinueStatement(
        BoundContinueStatement statement,
        FunctionEmissionContext context
    )
    {
        var (_, continueLabel) =
            context.LookupLabels(statement.Label)
            ?? throw new InvalidOperationException("This shouldn't happen");
        _builder.BuildBr(continueLabel);
    }

    private LLVMValueRef CreateEntryAlloca(
        LLVMTypeRef type,
        string name,
        FunctionEmissionContext context
    )
    {
        var entry = context.Function.EntryBasicBlock;
        using var entryBuilder = _context.CreateBuilder();
        entryBuilder.PositionAtEnd(entry);
        return entryBuilder.BuildAlloca(type, name);
    }

    private LLVMValueRef EmitExpression(BoundExpression expression, FunctionEmissionContext context)
    {
        return expression switch
        {
            BoundBadExpression => throw new InvalidOperationException(
                "Should only emit LLVM IR if the compilation is valid"
            ),
            BoundLiteral literal => MakeConstant(literal.Value),
            BoundVariableAccess access => EmitAccess(access, context),
            BoundParameterAccess access => EmitAccess(access, context),
            BoundUnaryOperation unary => EmitOperation(unary, context),
            BoundBinaryOperation binary => EmitOperation(binary, context),
            BoundAssignmentOperation assignment => EmitAssignment(assignment, context),
            BoundConditional conditional => EmitConditional(conditional, context),
            BoundInvocation invocation => EmitCall(invocation, context),
            BoundConversion conversion => EmitConversion(conversion, context),
            _ => throw new InvalidOperationException("We probably added a new expression type"),
        };
    }

    private LLVMValueRef EmitAccess(BoundVariableAccess access, FunctionEmissionContext context)
    {
        var val = EmitAccessCore(access, context);
        if (access.Symbol is { IsMutable: false, IsGlobal: false, HasInitializer: true })
        {
            return val;
        }

        var type = GetOrCreateType(access.Symbol.Type);
        return _builder.BuildLoad2(type, val);
    }

    private LLVMValueRef EmitAccess(BoundParameterAccess access, FunctionEmissionContext context)
    {
        var val = EmitAccessCore(access, context);
        if (!access.Symbol.IsMutable)
            return val;

        var type = GetOrCreateType(access.Symbol.Type);
        return _builder.BuildLoad2(type, val);
    }

    private LLVMValueRef EmitOperation(
        BoundUnaryOperation operation,
        FunctionEmissionContext context
    )
    {
        switch (operation.Operation)
        {
            case UnaryOperation.Identity:
                return EmitExpression(operation.Operand, context);
            case UnaryOperation.Negation:
            {
                var operand = EmitExpression(operation.Operand, context);
                return operation.Operand.Type.SpecialType.IsInteger
                    ? _builder.BuildNeg(operand)
                    : _builder.BuildFNeg(operand);
            }
            case UnaryOperation.LogicalNot:
            case UnaryOperation.BitwiseNot:
                return _builder.BuildNot(EmitExpression(operation.Operand, context));
            case UnaryOperation.PreIncrement:
                return EmitUnaryIncrementDecrement(
                    operation,
                    UnaryReturnType.Prefix,
                    UnaryArithmeticType.Increment,
                    context
                );
            case UnaryOperation.PreDecrement:
                return EmitUnaryIncrementDecrement(
                    operation,
                    UnaryReturnType.Prefix,
                    UnaryArithmeticType.Decrement,
                    context
                );
            case UnaryOperation.PostIncrement:
                return EmitUnaryIncrementDecrement(
                    operation,
                    UnaryReturnType.Postfix,
                    UnaryArithmeticType.Increment,
                    context
                );
            case UnaryOperation.PostDecrement:
                return EmitUnaryIncrementDecrement(
                    operation,
                    UnaryReturnType.Postfix,
                    UnaryArithmeticType.Decrement,
                    context
                );
            default:
                throw new ArgumentException("Invalid operation");
        }
    }

    private LLVMValueRef EmitUnaryIncrementDecrement(
        BoundUnaryOperation operation,
        UnaryReturnType returnType,
        UnaryArithmeticType direction,
        FunctionEmissionContext context
    )
    {
        var operand = EmitAddress(operation.Operand, context);
        var type = GetOrCreateType(operation.Operand.Type);
        var value = _builder.BuildLoad2(type, operand);
        var updated = direction switch
        {
            UnaryArithmeticType.Increment => _builder.BuildAdd(
                value,
                LLVMValueRef.CreateConstInt(value.TypeOf, 1)
            ),
            UnaryArithmeticType.Decrement => _builder.BuildSub(
                value,
                LLVMValueRef.CreateConstInt(value.TypeOf, 1)
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };
        _builder.BuildStore(updated, operand);
        return returnType switch
        {
            UnaryReturnType.Prefix => updated,
            UnaryReturnType.Postfix => value,
            _ => throw new ArgumentOutOfRangeException(nameof(returnType), returnType, null),
        };
    }

    private LLVMValueRef EmitOperation(
        BoundBinaryOperation operation,
        FunctionEmissionContext context
    )
    {
        var left = EmitExpression(operation.Left, context);
        var right = EmitExpression(operation.Right, context);
        return EmitBinaryOperation(operation.Left.Type, left, right, operation.Operation);
    }

    private LLVMValueRef EmitBinaryOperation(
        TypeSymbol type,
        LLVMValueRef left,
        LLVMValueRef right,
        BinaryOperation operation
    )
    {
        return operation switch
        {
            BinaryOperation.Addition => type.SpecialType.IsInteger
                ? _builder.BuildAdd(left, right)
                : _builder.BuildFAdd(left, right),
            BinaryOperation.Subtraction => type.SpecialType.IsInteger
                ? _builder.BuildSub(left, right)
                : _builder.BuildFSub(left, right),
            BinaryOperation.Multiplication => type.SpecialType.IsInteger
                ? _builder.BuildMul(left, right)
                : _builder.BuildFMul(left, right),
            BinaryOperation.Division => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.BuildSDiv(left, right),
                { IsUnsignedInteger: true } => _builder.BuildUDiv(left, right),
                _ => _builder.BuildFDiv(left, right),
            },
            BinaryOperation.Modulo => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.BuildSRem(left, right),
                { IsUnsignedInteger: true } => _builder.BuildURem(left, right),
                _ => _builder.BuildFRem(left, right),
            },
            BinaryOperation.BitwiseAnd or BinaryOperation.LogicalAnd => _builder.BuildAnd(
                left,
                right
            ),
            BinaryOperation.BitwiseOr or BinaryOperation.LogicalOr => _builder.BuildOr(left, right),
            BinaryOperation.BitwiseXor => _builder.BuildXor(left, right),
            BinaryOperation.Equality => type.SpecialType.IsFloatingPoint
                ? _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, left, right)
                : _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, left, right),
            BinaryOperation.NotEquals => type.SpecialType.IsFloatingPoint
                ? _builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE, left, right)
                : _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, left, right),
            BinaryOperation.LessThan => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntSLT,
                    left,
                    right
                ),
                { IsUnsignedInteger: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntULT,
                    left,
                    right
                ),
                _ => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLT, left, right),
            },
            BinaryOperation.LessThanOrEquals => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntSLE,
                    left,
                    right
                ),
                { IsUnsignedInteger: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntULE,
                    left,
                    right
                ),
                _ => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLE, left, right),
            },
            BinaryOperation.GreaterThan => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntSGT,
                    left,
                    right
                ),
                { IsUnsignedInteger: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntUGT,
                    left,
                    right
                ),
                _ => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGT, left, right),
            },
            BinaryOperation.GreaterThanOrEquals => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntSGE,
                    left,
                    right
                ),
                { IsUnsignedInteger: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntUGE,
                    left,
                    right
                ),
                _ => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGE, left, right),
            },
            BinaryOperation.ThreeWayComparison => throw new NotSupportedException(
                "Three way comparisons are not supported yet"
            ),
            BinaryOperation.ShiftLeft => _builder.BuildShl(left, right),
            BinaryOperation.ShiftRight => _builder.BuildAShr(left, right),
            BinaryOperation.UnsignedShiftRight => _builder.BuildLShr(left, right),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null),
        };
    }

    private LLVMValueRef EmitAssignment(
        BoundAssignmentOperation operation,
        FunctionEmissionContext context
    )
    {
        var assignee = EmitAddress(operation.Left, context);
        var value = EmitExpression(operation.Right, context);
        if (operation.Operation == AssignmentOperation.Simple)
        {
            _builder.BuildStore(value, assignee);
        }
        else
        {
            var binaryOp = operation.Operation.ToBinaryOperation();
            var type = GetOrCreateType(operation.Left.Type);
            var assigneeValue = _builder.BuildLoad2(type, assignee);
            var result = EmitBinaryOperation(operation.Left.Type, assigneeValue, value, binaryOp);
            _builder.BuildStore(result, assignee);
        }

        // Assignments do not return a value
        return null!;
    }

    private LLVMValueRef EmitConditional(
        BoundConditional conditional,
        FunctionEmissionContext context
    )
    {
        var function = context.Function;
        var thenBlock = function.AppendBasicBlock("cond.then");
        var elseBlock = function.AppendBasicBlock("cond.else");
        var mergeBlock = function.AppendBasicBlock("cond.merge");

        var condition = ConvertByteBoolToI1IfNeeded(EmitExpression(conditional.Condition, context));
        _builder.BuildCondBr(condition, thenBlock, elseBlock);

        _builder.PositionAtEnd(thenBlock);
        var thenValue = EmitExpression(conditional.WhenTrue, context);
        var actualThenBlock = _builder.InsertBlock;
        _builder.BuildBr(mergeBlock);

        _builder.PositionAtEnd(elseBlock);
        var elseValue = EmitExpression(conditional.WhenFalse, context);
        var actualElseBlock = _builder.InsertBlock;
        _builder.BuildBr(mergeBlock);

        _builder.PositionAtEnd(mergeBlock);
        var resultType = GetOrCreateType(conditional.Type);
        var phi = _builder.BuildPhi(resultType);

        Span<LLVMValueRef> values = [thenValue, elseValue];
        Span<LLVMBasicBlockRef> blocks = [actualThenBlock, actualElseBlock];
        phi.AddIncoming(values, blocks, 2);
        return phi;
    }

    private LLVMValueRef EmitCall(BoundInvocation call, FunctionEmissionContext context)
    {
        var target = call.Function;
        var callee = GetOrCreateFunction(target);

        using var arguments = call
            .Arguments.AsValueEnumerable()
            .Select(a => EmitExpression(a, context))
            .ToArrayPool();
        return _builder.BuildCall2(callee.FunctionType, callee, arguments.Span, "");
    }

    private LLVMValueRef EmitConversion(BoundConversion conversion, FunctionEmissionContext context)
    {
        var operand = EmitExpression(conversion.Operand, context);

        var sourceType = conversion.Operand.Type;
        var targetType = conversion.Type;

        return EmitScalarConversion(operand, conversion.Conversion, sourceType, targetType);
    }

    private LLVMValueRef EmitScalarConversion(
        LLVMValueRef operand,
        Conversion conversion,
        TypeSymbol sourceType,
        TypeSymbol targetType
    )
    {
        var source = GetOrCreateType(sourceType);
        var target = GetOrCreateType(targetType);

        if (source == target)
            return operand;

        if (conversion.IsNumeric)
        {
            if (sourceType.SpecialType.IsInteger && targetType.SpecialType.IsInteger)
            {
                if (source.IntWidth < target.IntWidth)
                {
                    return sourceType.SpecialType.IsSignedInteger
                        ? _builder.BuildSExt(operand, target)
                        : _builder.BuildZExt(operand, target);
                }

                Debug.Assert(source.IntWidth > target.IntWidth);
                return _builder.BuildTrunc(operand, target);
            }

            if (sourceType.SpecialType.IsFloatingPoint && targetType.SpecialType.IsFloatingPoint)
            {
                if (sourceType.SpecialType == SpecialType.F32)
                {
                    Debug.Assert(targetType.SpecialType == SpecialType.F64);
                    return _builder.BuildFPExt(operand, target);
                }

                Debug.Assert(sourceType.SpecialType == SpecialType.F64);
                Debug.Assert(targetType.SpecialType == SpecialType.F32);
                return _builder.BuildFPTrunc(operand, target);
            }

            if (sourceType.SpecialType.IsFloatingPoint && targetType.SpecialType.IsInteger)
            {
                return targetType.SpecialType.IsSignedInteger
                    ? _builder.BuildFPToSI(operand, target)
                    : _builder.BuildFPToUI(operand, target);
            }

            if (sourceType.SpecialType.IsInteger && targetType.SpecialType.IsFloatingPoint)
            {
                return targetType.SpecialType.IsSignedInteger
                    ? _builder.BuildSIToFP(operand, target)
                    : _builder.BuildUIToFP(operand, target);
            }
        }

        // ReSharper disable once InvertIf
        if (conversion.IsCharacter)
        {
            if (source.IntWidth < target.IntWidth)
            {
                return _builder.BuildZExt(operand, target);
            }

            Debug.Assert(source.IntWidth > target.IntWidth);
            return _builder.BuildTrunc(operand, target);
        }

        throw new InvalidOperationException("If we get here, the conversion is invalid");
    }

    private LLVMValueRef EmitAddress(BoundExpression expression, FunctionEmissionContext context)
    {
        return expression switch
        {
            BoundVariableAccess access => EmitAccessCore(access, context),
            BoundParameterAccess access => EmitAccessCore(access, context),
            _ => throw new ArgumentException("Invalid expression type for address emission"),
        };
    }

    private LLVMValueRef EmitAccessCore(BoundVariableAccess access, FunctionEmissionContext context)
    {
        return context.LookupStorage(access.Symbol) ?? GetOrCreateGlobal(access.Symbol);
    }

    private static LLVMValueRef EmitAccessCore(
        BoundParameterAccess access,
        FunctionEmissionContext context
    )
    {
        return context.LookupStorage(access.Symbol)
            ?? throw new ArgumentException("Invalid parameter access");
    }

    private void WriteIR()
    {
        var targetPath = Path.Combine(_options.OutputDirectory, $"{_compilation.AssemblyName}.ll");
        _module.PrintToFile(targetPath);
    }

    private EmitResult OutputBinary()
    {
        LLVM.InitializeNativeTarget();
        LLVM.InitializeNativeAsmPrinter();
        LLVM.InitializeNativeAsmParser();

        var tripleString = _compilation.Settings.GetLlvmTriple();
        if (!LLVMTargetRef.TryGetTargetFromTriple(tripleString, out var target, out _))
        {
            // TODO: Emit a diagnostic
            return new EmitResult(false, []);
        }

        const string cpu = "generic";
        const string features = "";
        var targetMachine = target.CreateTargetMachine(
            tripleString,
            cpu,
            features,
            LLVMCodeGenOptLevel.LLVMCodeGenLevelNone,
            LLVMRelocMode.LLVMRelocPIC,
            LLVMCodeModel.LLVMCodeModelDefault
        );

        _module.Target = tripleString;
        _module.DataLayoutObject = targetMachine.CreateTargetDataLayout();

        var outputFilename = Path.Combine(
            _options.OutputDirectory,
            $"{_compilation.AssemblyName}.obj"
        );

        if (
            !targetMachine.TryEmitToFile(
                _module,
                outputFilename,
                LLVMCodeGenFileType.LLVMObjectFile,
                out _
            )
        )
        {
            return new EmitResult(false, []);
        }

        return new EmitResult(true, []);
    }

    public void Dispose()
    {
        _builder.Dispose();
        _module.Dispose();
        _context.Dispose();
    }
}
