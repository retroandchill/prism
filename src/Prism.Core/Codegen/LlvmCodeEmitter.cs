// @file LlvmCodeEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics;
using LLVMSharp;
using LLVMSharp.Interop;
using Prism.Core.BoundTree;
using Prism.Core.Compiling;
using Prism.Core.Configuration;
using Prism.Core.Mappers;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Core.Utils;
using ZLinq;
using Type = LLVMSharp.Type;

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

    private readonly LLVMContext _context;
    private readonly Module _module;
    private readonly IRBuilder _builder;

    private readonly Dictionary<Symbol, Value> _symbolToValue = new(
        ReferenceEqualityComparer.Instance
    );

    private readonly Dictionary<TypeSymbol, Type> _symbolToType = new(
        ReferenceEqualityComparer.Instance
    );

    public LlvmCodeEmitter(Compilation compilation, CodeGenOptions options)
    {
        _compilation = compilation;
        _options = options;
        _context = new LLVMContext();
        _module = Module.Create(_context, compilation.AssemblyName);
        _builder = new IRBuilder(_context);
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
            _builder.CreateRetVoid();
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

        var functionType = FunctionType.Get(Type.GetInt32Ty(_context), []);
        var mainFunc = _module.AddFunction("main", functionType);
        var entry = mainFunc.AppendBasicBlock("entry");
        _builder.SetInsertPoint(entry);

        var targetFunction = GetOrCreateFunction(entryPoint);
        var callEntryPoint = _builder.CreateCall(targetFunction.FunctionType, targetFunction);
        if (entryPoint.ReturnsVoid)
        {
            _builder.CreateRet(ConstantInt.Get(Type.GetInt32Ty(_context), 0));
        }
        else
        {
            _builder.CreateRet(callEntryPoint);
        }

        return true;
    }

    private Function GetOrCreateFunction(FunctionSymbol functionSymbol)
    {
        if (_symbolToValue.TryGetValue(functionSymbol, out var function))
        {
            return (Function)function;
        }

        var returnType = GetOrCreateType(functionSymbol.ReturnType);
        using var parameters = functionSymbol
            .Parameters.AsValueEnumerable()
            .Select(p => GetOrCreateType(p.Type))
            .ToArrayPool();

        var name = functionSymbol.Mangle();
        var functionType = FunctionType.Get(returnType, parameters.Span, false);
        var func = _module.AddFunction(name, functionType);
        _symbolToValue[functionSymbol] = func;
        return func;
    }

    private GlobalVariable GetOrCreateGlobal(VariableSymbol symbol)
    {
        if (_symbolToValue.TryGetValue(symbol, out var global))
        {
            return (GlobalVariable)global;
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
            var functionType = FunctionType.Get(Type.GetVoidTy(_context), [], false);
            var initializerName = $"{_compilation.AssemblyName}_<GlobalInitializer>";
            var assemblyInitializer = _module.AddFunction(initializerName, functionType);
            assemblyInitializer.Linkage = LLVMLinkage.LLVMInternalLinkage;
            var block = assemblyInitializer.AppendBasicBlock("entry");

            assemblyInitializerContext = new FunctionEmissionContext(assemblyInitializer, block);
            _builder.SetInsertPoint(block);
        }

        var initializedValue = EmitExpression(initializer, assemblyInitializerContext);
        _builder.CreateStore(initializedValue, variable);
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
        _builder.SetInsertPoint(entry);
        var context = new FunctionEmissionContext(function, entry);

        foreach (
            var (symbolParam, llvmParam) in symbol
                .Parameters.AsValueEnumerable()
                .Zip(function.GetParams())
        )
        {
            if (symbolParam.IsMutable)
            {
                var slot = CreateEntryAlloca(llvmParam.Type, symbolParam.Name, context);
                _builder.CreateStore(llvmParam, slot);
                context.BindStorage(symbolParam, slot);
            }
            else
            {
                context.BindStorage(symbolParam, llvmParam);
            }
        }
        EmitStatement(body, context);
    }

    private Type GetOrCreateType(TypeSymbol symbol)
    {
        if (_symbolToType.TryGetValue(symbol, out var type))
        {
            return type;
        }

        type = CreateType(symbol);
        _symbolToType[symbol] = type;
        return type;
    }

    private Type CreateType(TypeSymbol symbol)
    {
        return symbol.SpecialType switch
        {
            SpecialType.Void => Type.GetVoidTy(_context),
            SpecialType.Bool or SpecialType.I8 or SpecialType.U8 or SpecialType.Char =>
                Type.GetInt8Ty(_context),
            SpecialType.I16 or SpecialType.U16 or SpecialType.Char16 => Type.GetInt16Ty(_context),
            SpecialType.I32 or SpecialType.U32 or SpecialType.Rune => Type.GetInt32Ty(_context),
            SpecialType.I64 or SpecialType.U64 => Type.GetInt64Ty(_context),
            SpecialType.I128 or SpecialType.U128 => Type.GetInt128Ty(_context),
            SpecialType.ISize or SpecialType.USize => _compilation.Settings.PointerWidth switch
            {
                PointerWidth.X32 => Type.GetInt32Ty(_context),
                PointerWidth.X64 => Type.GetInt64Ty(_context),
                _ => throw new InvalidOperationException("Invalid pointer width"),
            },
            SpecialType.F32 => Type.GetFloatTy(_context),
            SpecialType.F64 => Type.GetDoubleTy(_context),
            _ => throw new NotImplementedException(),
        };
    }

    private Constant MakeConstant(in ConstantValue value)
    {
        return value.Kind switch
        {
            ConstantKind.Bool => ConstantInt.Get(
                Type.GetInt8Ty(_context),
                value.AsBoolean() ? 1UL : 0UL
            ),
            ConstantKind.Char => ConstantInt.Get(
                Type.GetInt8Ty(_context),
                (ulong)value.AsCharacter().Value
            ),
            ConstantKind.Char16 => ConstantInt.Get(
                Type.GetInt16Ty(_context),
                (ulong)value.AsCharacter().Value
            ),
            ConstantKind.Rune => ConstantInt.Get(
                Type.GetInt32Ty(_context),
                (ulong)value.AsCharacter().Value
            ),
            ConstantKind.I8 => ConstantInt.GetSigned(Type.GetInt8Ty(_context), value.AsInt64()),
            ConstantKind.I16 => ConstantInt.GetSigned(Type.GetInt16Ty(_context), value.AsInt64()),
            ConstantKind.I32 => ConstantInt.GetSigned(Type.GetInt32Ty(_context), value.AsInt64()),
            ConstantKind.I64 => ConstantInt.GetSigned(Type.GetInt64Ty(_context), value.AsInt64()),
            ConstantKind.I128 => ConstantInt.Get(Type.GetInt128Ty(_context), value.AsInt128()),
            ConstantKind.ISize => ConstantInt.GetSigned(
                _compilation.Settings.PointerWidth switch
                {
                    PointerWidth.X32 => Type.GetInt32Ty(_context),
                    PointerWidth.X64 => Type.GetInt64Ty(_context),
                    _ => throw new InvalidOperationException("Invalid pointer width"),
                },
                value.AsInt64()
            ),
            ConstantKind.U8 => ConstantInt.Get(Type.GetInt8Ty(_context), value.AsUInt64()),
            ConstantKind.U16 => ConstantInt.Get(Type.GetInt16Ty(_context), value.AsUInt64()),
            ConstantKind.U32 => ConstantInt.Get(Type.GetInt32Ty(_context), value.AsUInt64()),
            ConstantKind.U64 => ConstantInt.Get(Type.GetInt64Ty(_context), value.AsUInt64()),
            ConstantKind.U128 => ConstantInt.Get(Type.GetInt64Ty(_context), value.AsUInt128()),
            ConstantKind.USize => ConstantInt.Get(
                _compilation.Settings.PointerWidth switch
                {
                    PointerWidth.X32 => Type.GetInt32Ty(_context),
                    PointerWidth.X64 => Type.GetInt64Ty(_context),
                    _ => throw new InvalidOperationException("Invalid pointer width"),
                },
                value.AsUInt64()
            ),
            ConstantKind.F32 => ConstantFP.Get(Type.GetFloatTy(_context), value.AsFloat32()),
            ConstantKind.F64 => ConstantFP.Get(Type.GetDoubleTy(_context), value.AsFloat64()),
            ConstantKind.Str => _builder.CreateGlobalString(value.AsString()),
            _ => throw new ArgumentException("Invalid constant kind"),
        };
    }

    private Value ConvertByteBoolToI1IfNeeded(Value value)
    {
        return value.Type is IntegerType { BitWidth: 8 }
            ? _builder.CreateICmp(
                CmpInst.Predicate.ICMP_NE,
                value,
                ConstantInt.Get(Type.GetInt8Ty(_context), 0)
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

        _builder.CreateStore(EmitExpression(declaration.Initializer, context), slot);
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
            _builder.CreateRetVoid();
            return;
        }

        var expression = EmitExpression(returnStatement.Expression, context);
        _builder.CreateRet(expression);
    }

    private void EmitIfStatement(BoundIfStatement statement, FunctionEmissionContext context)
    {
        var function = context.Function;
        var thenBlock = function.AppendBasicBlock("cond.then");
        var elseBlock = statement.ElseStatement is not null
            ? function.AppendBasicBlock("cond.else")
            : null;
        var mergeBlock = function.AppendBasicBlock("cond.merge");

        var condition = ConvertByteBoolToI1IfNeeded(EmitExpression(statement.Condition, context));
        _builder.CreateCondBr(condition, thenBlock, elseBlock ?? mergeBlock);

        _builder.SetInsertPoint(thenBlock);
        EmitStatement(statement.ThenStatement, context);
        _builder.CreateBr(mergeBlock);

        if (elseBlock is not null)
        {
            _builder.SetInsertPoint(elseBlock);
            Debug.Assert(statement.ElseStatement is not null);
            EmitStatement(statement.ElseStatement, context);
            _builder.CreateBr(mergeBlock);
        }

        _builder.SetInsertPoint(mergeBlock);
    }

    void EmitWhileStatement(BoundWhileStatement statement, FunctionEmissionContext context)
    {
        throw new NotImplementedException();
    }

    void EmitLoopStatement(BoundLoopStatement loop, FunctionEmissionContext context)
    {
        throw new NotImplementedException();
    }

    void EmitForLoop(BoundForStatement loop, FunctionEmissionContext context)
    {
        throw new NotImplementedException();
    }

    void EmitBreakStatement(BoundBreakStatement statement, FunctionEmissionContext context)
    {
        throw new NotImplementedException();
    }

    void EmitContinueStatement(BoundContinueStatement statement, FunctionEmissionContext context)
    {
        throw new NotImplementedException();
    }

    private AllocaInst CreateEntryAlloca(Type type, string name, FunctionEmissionContext context)
    {
        var entry = context.Function.EntryBasicBlock;
        var entryBuilder = new IRBuilder(_context);
        try
        {
            entryBuilder.SetInsertPoint(entry);
            return entryBuilder.CreateAlloca(type, name: name);
        }
        finally
        {
            entryBuilder.Handle.Dispose();
        }
    }

    private Value EmitExpression(BoundExpression expression, FunctionEmissionContext context)
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

    private Value EmitAccess(BoundVariableAccess access, FunctionEmissionContext context)
    {
        var val = EmitAccessCore(access, context);
        if (access.Symbol is { IsMutable: false, IsGlobal: false, HasInitializer: true })
        {
            return val;
        }

        var type = GetOrCreateType(access.Symbol.Type);
        return _builder.CreateLoad(type, val);
    }

    private Value EmitAccess(BoundParameterAccess access, FunctionEmissionContext context)
    {
        var val = EmitAccessCore(access, context);
        if (!access.Symbol.IsMutable)
            return val;

        var type = GetOrCreateType(access.Symbol.Type);
        return _builder.CreateLoad(type, val);
    }

    private Value EmitOperation(BoundUnaryOperation operation, FunctionEmissionContext context)
    {
        switch (operation.Operation)
        {
            case UnaryOperation.Identity:
                return EmitExpression(operation.Operand, context);
            case UnaryOperation.Negation:
            {
                var operand = EmitExpression(operation.Operand, context);
                return operation.Operand.Type.SpecialType.IsInteger
                    ? _builder.CreateNeg(operand)
                    : _builder.CreateFNeg(operand);
            }
            case UnaryOperation.LogicalNot:
            case UnaryOperation.BitwiseNot:
                return _builder.CreateNot(EmitExpression(operation.Operand, context));
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

    private Value EmitUnaryIncrementDecrement(
        BoundUnaryOperation operation,
        UnaryReturnType returnType,
        UnaryArithmeticType direction,
        FunctionEmissionContext context
    )
    {
        var operand = EmitAddress(operation.Operand, context);
        var type = GetOrCreateType(operation.Operand.Type);
        var value = _builder.CreateLoad(type, operand);
        var updated = direction switch
        {
            UnaryArithmeticType.Increment => _builder.CreateAdd(
                value,
                ConstantInt.Get(value.Type, 1)
            ),
            UnaryArithmeticType.Decrement => _builder.CreateSub(
                value,
                ConstantInt.Get(value.Type, 1)
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };
        _builder.CreateStore(updated, operand);
        return returnType switch
        {
            UnaryReturnType.Prefix => updated,
            UnaryReturnType.Postfix => value,
            _ => throw new ArgumentOutOfRangeException(nameof(returnType), returnType, null),
        };
    }

    private Value EmitOperation(BoundBinaryOperation operation, FunctionEmissionContext context)
    {
        var left = EmitExpression(operation.Left, context);
        var right = EmitExpression(operation.Right, context);
        return EmitBinaryOperation(operation.Left.Type, left, right, operation.Operation);
    }

    private Value EmitBinaryOperation(
        TypeSymbol type,
        Value left,
        Value right,
        BinaryOperation operation
    )
    {
        return operation switch
        {
            BinaryOperation.Addition => type.SpecialType.IsInteger
                ? _builder.CreateAdd(left, right)
                : _builder.CreateFAdd(left, right),
            BinaryOperation.Subtraction => type.SpecialType.IsInteger
                ? _builder.CreateSub(left, right)
                : _builder.CreateFSub(left, right),
            BinaryOperation.Multiplication => type.SpecialType.IsInteger
                ? _builder.CreateMul(left, right)
                : _builder.CreateFMul(left, right),
            BinaryOperation.Division => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.CreateSDiv(left, right),
                { IsUnsignedInteger: true } => _builder.CreateUDiv(left, right),
                _ => _builder.CreateFDiv(left, right),
            },
            BinaryOperation.Modulo => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.CreateSRem(left, right),
                { IsUnsignedInteger: true } => _builder.CreateURem(left, right),
                _ => _builder.CreateFRem(left, right),
            },
            BinaryOperation.BitwiseAnd or BinaryOperation.LogicalAnd => _builder.CreateAnd(
                left,
                right
            ),
            BinaryOperation.BitwiseOr or BinaryOperation.LogicalOr => _builder.CreateOr(
                left,
                right
            ),
            BinaryOperation.BitwiseXor => _builder.CreateXor(left, right),
            BinaryOperation.Equality => type.SpecialType.IsFloatingPoint
                ? _builder.CreateFCmp(CmpInst.Predicate.FCMP_OEQ, left, right)
                : _builder.CreateICmp(CmpInst.Predicate.ICMP_EQ, left, right),
            BinaryOperation.NotEquals => type.SpecialType.IsFloatingPoint
                ? _builder.CreateFCmp(CmpInst.Predicate.FCMP_ONE, left, right)
                : _builder.CreateICmp(CmpInst.Predicate.ICMP_NE, left, right),
            BinaryOperation.LessThan => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.CreateICmp(
                    CmpInst.Predicate.ICMP_SLT,
                    left,
                    right
                ),
                { IsUnsignedInteger: true } => _builder.CreateICmp(
                    CmpInst.Predicate.ICMP_ULT,
                    left,
                    right
                ),
                _ => _builder.CreateFCmp(CmpInst.Predicate.FCMP_OLT, left, right),
            },
            BinaryOperation.LessThanOrEquals => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.CreateICmp(
                    CmpInst.Predicate.ICMP_SLE,
                    left,
                    right
                ),
                { IsUnsignedInteger: true } => _builder.CreateICmp(
                    CmpInst.Predicate.ICMP_ULE,
                    left,
                    right
                ),
                _ => _builder.CreateFCmp(CmpInst.Predicate.FCMP_OLE, left, right),
            },
            BinaryOperation.GreaterThan => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.CreateICmp(
                    CmpInst.Predicate.ICMP_SGT,
                    left,
                    right
                ),
                { IsUnsignedInteger: true } => _builder.CreateICmp(
                    CmpInst.Predicate.ICMP_UGT,
                    left,
                    right
                ),
                _ => _builder.CreateFCmp(CmpInst.Predicate.FCMP_OGT, left, right),
            },
            BinaryOperation.GreaterThanOrEquals => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.CreateICmp(
                    CmpInst.Predicate.ICMP_SGE,
                    left,
                    right
                ),
                { IsUnsignedInteger: true } => _builder.CreateICmp(
                    CmpInst.Predicate.ICMP_UGE,
                    left,
                    right
                ),
                _ => _builder.CreateFCmp(CmpInst.Predicate.FCMP_OGE, left, right),
            },
            BinaryOperation.ThreeWayComparison => throw new NotSupportedException(
                "Three way comparisons are not supported yet"
            ),
            BinaryOperation.ShiftLeft => _builder.CreateShl(left, right),
            BinaryOperation.ShiftRight => _builder.CreateAShr(left, right),
            BinaryOperation.UnsignedShiftRight => _builder.CreateLShr(left, right),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null),
        };
    }

    private Value EmitAssignment(
        BoundAssignmentOperation operation,
        FunctionEmissionContext context
    )
    {
        var assignee = EmitAddress(operation.Left, context);
        var value = EmitExpression(operation.Right, context);
        if (operation.Operation == AssignmentOperation.Simple)
        {
            _builder.CreateStore(value, assignee);
        }
        else
        {
            var binaryOp = operation.Operation.ToBinaryOperation();
            var type = GetOrCreateType(operation.Left.Type);
            var assigneeValue = _builder.CreateLoad(type, assignee);
            var result = EmitBinaryOperation(operation.Left.Type, assigneeValue, value, binaryOp);
            _builder.CreateStore(result, assignee);
        }

        // Assignments do not return a value
        return null!;
    }

    private Value EmitConditional(BoundConditional conditional, FunctionEmissionContext context)
    {
        var function = context.Function;
        var thenBlock = function.AppendBasicBlock("cond.then");
        var elseBlock = function.AppendBasicBlock("cond.else");
        var mergeBlock = function.AppendBasicBlock("cond.merge");

        var condition = ConvertByteBoolToI1IfNeeded(EmitExpression(conditional.Condition, context));
        _builder.CreateCondBr(condition, thenBlock, elseBlock);

        _builder.SetInsertPoint(thenBlock);
        var thenValue = EmitExpression(conditional.WhenTrue, context);
        var actualThenBlock = _builder.InsertBlock;
        _builder.CreateBr(mergeBlock);

        _builder.SetInsertPoint(elseBlock);
        var elseValue = EmitExpression(conditional.WhenFalse, context);
        var actualElseBlock = _builder.InsertBlock;
        _builder.CreateBr(mergeBlock);

        _builder.SetInsertPoint(mergeBlock);
        var resultType = GetOrCreateType(conditional.Type);
        var phi = _builder.CreatePHI(resultType);
        phi.AddIncoming(thenValue, actualThenBlock);
        phi.AddIncoming(elseValue, actualElseBlock);
        return phi;
    }

    private CallInst EmitCall(BoundInvocation call, FunctionEmissionContext context)
    {
        var target = call.Function;
        var callee = GetOrCreateFunction(target);

        using var arguments = call
            .Arguments.AsValueEnumerable()
            .Select(a => EmitExpression(a, context))
            .ToArrayPool();
        return _builder.CreateCall(callee.FunctionType, callee, arguments.Span, "");
    }

    private Value EmitConversion(BoundConversion conversion, FunctionEmissionContext context)
    {
        var operand = EmitExpression(conversion.Operand, context);

        var sourceType = conversion.Operand.Type;
        var targetType = conversion.Type;

        return EmitScalarConversion(operand, conversion.Conversion, sourceType, targetType);
    }

    private Value EmitScalarConversion(
        Value operand,
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
                var sourceInt = (IntegerType)source;
                var destInt = (IntegerType)target;

                if (sourceInt.BitWidth < destInt.BitWidth)
                {
                    return sourceType.SpecialType.IsSignedInteger
                        ? _builder.CreateSExt(operand, target)
                        : _builder.CreateZExt(operand, target);
                }

                Debug.Assert(sourceInt.BitWidth > destInt.BitWidth);
                return _builder.CreateTrunc(operand, target);
            }

            if (sourceType.SpecialType.IsFloatingPoint && targetType.SpecialType.IsFloatingPoint)
            {
                if (sourceType.SpecialType == SpecialType.F32)
                {
                    Debug.Assert(targetType.SpecialType == SpecialType.F64);
                    return _builder.CreateFPExt(operand, target);
                }

                Debug.Assert(sourceType.SpecialType == SpecialType.F64);
                Debug.Assert(targetType.SpecialType == SpecialType.F32);
                return _builder.CreateFPTrunc(operand, target);
            }

            if (sourceType.SpecialType.IsFloatingPoint && targetType.SpecialType.IsInteger)
            {
                return targetType.SpecialType.IsSignedInteger
                    ? _builder.CreateFPToSI(operand, target)
                    : _builder.CreateFPToUI(operand, target);
            }

            if (sourceType.SpecialType.IsInteger && targetType.SpecialType.IsFloatingPoint)
            {
                return targetType.SpecialType.IsSignedInteger
                    ? _builder.CreateSIToFP(operand, target)
                    : _builder.CreateUIToFP(operand, target);
            }
        }

        // ReSharper disable once InvertIf
        if (conversion.IsCharacter)
        {
            var sourceInt = (IntegerType)source;
            var destInt = (IntegerType)target;

            if (sourceInt.BitWidth < destInt.BitWidth)
            {
                return _builder.CreateZExt(operand, target);
            }

            Debug.Assert(sourceInt.BitWidth > destInt.BitWidth);
            return _builder.CreateTrunc(operand, target);
        }

        throw new InvalidOperationException("If we get here, the conversion is invalid");
    }

    private Value EmitAddress(BoundExpression expression, FunctionEmissionContext context)
    {
        return expression switch
        {
            BoundVariableAccess access => EmitAccessCore(access, context),
            BoundParameterAccess access => EmitAccessCore(access, context),
            _ => throw new ArgumentException("Invalid expression type for address emission"),
        };
    }

    private Value EmitAccessCore(BoundVariableAccess access, FunctionEmissionContext context)
    {
        return context.LookupStorage(access.Symbol) ?? GetOrCreateGlobal(access.Symbol);
    }

    private static Value EmitAccessCore(
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
        LLVM.InitializeAllTargetInfos();
        LLVM.InitializeAllTargets();
        LLVM.InitializeAllTargetMCs();
        LLVM.InitializeAllAsmParsers();
        LLVM.InitializeAllAsmPrinters();

        // TODO: Implement binary output

        return new EmitResult(true, []);
    }

    public void Dispose()
    {
        _builder.Handle.Dispose();
        _module.Handle.Dispose();
        _context.Handle.Dispose();
    }
}
