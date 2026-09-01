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
        throw new NotImplementedException();
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

    void EmitReturn(BoundReturnStatement returnStatement, FunctionEmissionContext context)
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
        throw new NotImplementedException();
    }

    private Value EmitLiteral(BoundLiteral literal, FunctionEmissionContext context)
    {
        throw new NotImplementedException();
    }

    private Value EmitAccess(BoundVariableAccess access, FunctionEmissionContext context)
    {
        throw new NotImplementedException();
    }

    private Value EmitAccess(BoundParameterAccess access, FunctionEmissionContext context)
    {
        throw new NotImplementedException();
    }

    private Value EmitOperation(BoundUnaryOperation operation, FunctionEmissionContext context)
    {
        throw new NotImplementedException();
    }

    private Value EmitUnaryIncrementDecrement(
        BoundUnaryOperation operation,
        UnaryReturnType returnType,
        UnaryArithmeticType arithmeticType,
        FunctionEmissionContext context
    )
    {
        throw new NotImplementedException();
    }

    private Value EmitOperation(BoundBinaryOperation operation, FunctionEmissionContext context)
    {
        throw new NotImplementedException();
    }

    private Value EmitBinaryOperation(
        TypeSymbol type,
        Value left,
        Value right,
        BinaryOperation operation
    )
    {
        throw new NotImplementedException();
    }

    private Value EmitAssignment(
        BoundAssignmentOperation operation,
        FunctionEmissionContext context
    )
    {
        throw new NotImplementedException();
    }

    private void EmitAssignment(
        Value assignee,
        TypeSymbol assigneeType,
        Value value,
        Func<Value, Value, Value> functor
    )
    {
        throw new NotImplementedException();
    }

    private Value EmitConditional(BoundConditional operation, FunctionEmissionContext context)
    {
        throw new NotImplementedException();
    }

    private Value EmitCall(BoundInvocation call, FunctionEmissionContext context)
    {
        throw new NotImplementedException();
    }

    private Value EmitConversion(BoundConversion conversion, FunctionEmissionContext context)
    {
        throw new NotImplementedException();
    }

    private Value EmitScalarConversion(
        Value operand,
        Conversion conversion,
        TypeSymbol sourceType,
        TypeSymbol targetType
    )
    {
        throw new NotImplementedException();
    }

    private Value EmitAddress(BoundExpression expression, FunctionEmissionContext context)
    {
        throw new NotImplementedException();
    }

    private Value EmitAccessCore(BoundVariableAccess access, FunctionEmissionContext context)
    {
        throw new NotImplementedException();
    }

    private Value EmitAccessCore(BoundParameterAccess access, FunctionEmissionContext context)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _builder.Handle.Dispose();
        _module.Handle.Dispose();
        _context.Handle.Dispose();
    }
}
