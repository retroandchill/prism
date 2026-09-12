// @file LlvmCodeEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Cysharp.Text;
using LLVMSharp.Interop;
using Prism.Core.Binding;
using Prism.Core.BoundTree;
using Prism.Core.Codegen.Mir;
using Prism.Core.Compiling;
using Prism.Core.Configuration;
using Prism.Core.Mir;
using Prism.Core.Mir.Analysis;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using ZLinq;

namespace Prism.Core.Codegen;

internal sealed class LlvmCodeEmitter : ICodeEmitter
{
    private readonly Compilation _compilation;
    private readonly CodeGenOptions _options;
    private readonly MirEmitter _mirEmitter;

    private LLVMContextRef _context;
    private LLVMModuleRef _module;
    private LLVMBuilderRef _builder;

    private readonly Dictionary<Symbol, LLVMValueRef> _symbolToValue = new(
        ReferenceEqualityComparer.Instance
    );

    private readonly Dictionary<TypeSymbol, LLVMTypeRef> _symbolToType = new(
        ReferenceEqualityComparer.Instance
    );

    private static readonly uint ByValAttr = LookupEnumAttributeKind("byval");
    private LLVMValueRef? _lifetimeStartFunction;
    private LLVMValueRef? _lifetimeEndFunction;

    public LlvmCodeEmitter(Compilation compilation, CodeGenOptions options)
    {
        _compilation = compilation;
        _options = options;
        _mirEmitter = new MirEmitter(compilation);
        _context = LLVMContextRef.Create();
        _module = _context.CreateModuleWithName(compilation.AssemblyName);
        _builder = _context.CreateBuilder();
    }

    public EmitResult Emit(BindingContext context)
    {
        WriteIR();
        return !context.HasErrors
            ? OutputBinary(context)
            : new EmitResult(false, context.CollectDiagnostics());
    }

    public void AddGlobalVariable(
        BoundVariableInitializer variable,
        BindingContext bindingContext,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var llvmVariable = GetOrCreateGlobal(variable.Variable);
        if (!variable.Variable.HasInitializer)
            return;

        if (!variable.HasInitializer)
            return;

        if (variable.ConstantValue is not { } constant)
            return;

        llvmVariable.Initializer = MakeConstant(constant);
    }

    public void AddFunction(
        BoundFunctionBody function,
        BindingContext bindingContext,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var llvmFunction = GetOrCreateFunction(function.Function);
        if (!function.HasBody)
        {
            llvmFunction.Linkage = LLVMLinkage.LLVMAvailableExternallyLinkage;
            return;
        }

        var mirFunction = _mirEmitter.EmitFunction(function.Function, cancellationToken);
        var cfg = MirFunctionAnalyzer.AnalyzeControlFlow(mirFunction, cancellationToken);
        var localFlow = MirFunctionAnalyzer.AnalyzeLocalFlow(
            _compilation,
            mirFunction,
            cfg,
            cancellationToken
        );
        var classifiedLocals = MirFunctionAnalyzer.ClassifyLocals(
            _compilation,
            mirFunction,
            localFlow,
            cancellationToken
        );

        var context = new FunctionEmissionContext(mirFunction, llvmFunction, cfg, classifiedLocals);
        foreach (var block in mirFunction.Blocks)
        {
            var blockRef = LLVMBasicBlockRef.CreateInContext(_context, block.Name);
            context.BindBlock(block.Id, blockRef);
        }

        var entryBlock = context.LookupBlock(mirFunction.EntryBlock);
        llvmFunction.AppendExistingBasicBlock(entryBlock);
        _builder.PositionAtEnd(entryBlock);

        InitializeLocalVariables(mirFunction, context);
        foreach (var block in mirFunction.Blocks)
        {
            EmitBlock(block, context);
        }
    }

    public void RegisterGlobalConstructor(FunctionSymbol function)
    {
        _module.AppendToGlobalCtors(GetOrCreateFunction(function), 65535);
    }

    public void RegisterEntryPoint(FunctionSymbol entryPoint)
    {
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
    }

    private LLVMValueRef GetOrCreateFunction(FunctionSymbol functionSymbol)
    {
        if (_symbolToValue.TryGetValue(functionSymbol, out var function))
        {
            return function;
        }

        var abi = _compilation.GetFunctionAbi(functionSymbol);
        var indirectReturn = abi.Return.IsIndirect;
        var totalParameters = indirectReturn
            ? functionSymbol.Parameters.Length + 1
            : functionSymbol.Parameters.Length;
        Span<LLVMTypeRef> parameters = stackalloc LLVMTypeRef[totalParameters];

        LLVMTypeRef returnType;
        int firstParamIndex;
        if (indirectReturn)
        {
            returnType = _context.VoidType;
            parameters[0] = _context.CreatePointerType(0);
            ;
            firstParamIndex = 1;
        }
        else
        {
            returnType = GetOrCreateType(functionSymbol.ReturnType);
            firstParamIndex = 0;
        }

        Debug.Assert(abi.Parameters.Length + firstParamIndex == parameters.Length);
        foreach (var (i, parameterAbi) in abi.Parameters.AsValueEnumerable().Index())
        {
            if (parameterAbi.IsIndirect)
            {
                parameters[i + firstParamIndex] = _context.CreatePointerType(0);
            }
            else
            {
                parameters[i + firstParamIndex] = GetOrCreateType(parameterAbi.Parameter.Type);
            }
        }

        var name = functionSymbol.Mangle();
        var functionType = LLVMTypeRef.CreateFunction(returnType, parameters, false);
        var func = _module.AddFunction(name, functionType);

        if (indirectReturn)
        {
            var byValAttr = _context.CreateTypeAttribute(
                ByValAttr,
                GetOrCreateType(functionSymbol.ReturnType)
            );
            func.AddAttributeAtIndex((LLVMAttributeIndex)1, byValAttr);
        }

        foreach (var (i, parameterAbi) in abi.Parameters.AsValueEnumerable().Index())
        {
            if (!parameterAbi.IsIndirect)
                continue;
            var byValAttr = _context.CreateTypeAttribute(
                ByValAttr,
                GetOrCreateType(parameterAbi.Parameter.Type)
            );
            func.AddAttributeAtIndex(
                (LLVMAttributeIndex)(i + unchecked((uint)firstParamIndex) + 1),
                byValAttr
            );
        }

        _symbolToValue[functionSymbol] = func;
        return func;
    }

    private LLVMValueRef GetLifetimeStartFunction()
    {
        if (_lifetimeStartFunction is not null)
            return _lifetimeStartFunction.Value;

        var id = LookupIntrinsicId("llvm.lifetime.start.p0");
        _lifetimeStartFunction = _module.GetIntrinsicDeclaration(
            id,
            [_context.CreatePointerType(0)]
        );
        return _lifetimeStartFunction.Value;
    }

    private LLVMValueRef GetLifetimeEndFunction()
    {
        if (_lifetimeEndFunction is not null)
            return _lifetimeEndFunction.Value;

        var id = LookupIntrinsicId("llvm.lifetime.end.p0");
        _lifetimeEndFunction = _module.GetIntrinsicDeclaration(id, [_context.CreatePointerType(0)]);
        return _lifetimeEndFunction.Value;
    }

    private static uint LookupIntrinsicId(ReadOnlySpan<char> name)
    {
        using var builder = ZString.CreateUtf8StringBuilder();
        builder.Append(name);
        unsafe
        {
            fixed (byte* p = builder.AsSpan())
            {
                return LLVM.LookupIntrinsicID((sbyte*)p, (nuint)builder.Length);
            }
        }
    }

    private static uint LookupEnumAttributeKind(ReadOnlySpan<char> name)
    {
        using var builder = ZString.CreateUtf8StringBuilder();
        builder.Append(name);
        unsafe
        {
            fixed (byte* p = builder.AsSpan())
            {
                return LLVM.GetEnumAttributeKindForName((sbyte*)p, (nuint)builder.Length);
            }
        }
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
        switch (symbol)
        {
            case ReferenceTypeSymbol { ReferencedType: var referencedType }:
            {
                var pointer = _context.CreatePointerType(0);
                if (referencedType.IsDynamicallySized)
                {
                    return LLVMTypeRef.CreateStruct(
                        [pointer, GetOrCreateType(_compilation.GetSpecialType(SpecialType.USize))],
                        false
                    );
                }

                return pointer;
            }
            case ArrayTypeSymbol { ElementType: var elementType, Size: { } size }:
                return LLVMTypeRef.CreateArray2(GetOrCreateType(elementType), size);
            default:
                return symbol.SpecialType switch
                {
                    SpecialType.Void => _context.VoidType,
                    SpecialType.Bool or SpecialType.I8 or SpecialType.U8 or SpecialType.Char =>
                        _context.Int8Type,
                    SpecialType.I16 or SpecialType.U16 or SpecialType.Char16 => _context.Int16Type,
                    SpecialType.I32 or SpecialType.U32 or SpecialType.Rune => _context.Int32Type,
                    SpecialType.I64 or SpecialType.U64 => _context.Int64Type,
                    SpecialType.I128 or SpecialType.U128 => _context.Int128Type,
                    SpecialType.ISize or SpecialType.USize => _compilation
                        .Settings
                        .PointerWidth switch
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

    private LLVMValueRef ConvertI1ToByteBoolIfNeeded(LLVMValueRef value)
    {
        return value.TypeOf.IntWidth == 1 ? _builder.BuildZExt(value, _context.Int8Type) : value;
    }

    private void InitializeLocalVariables(MirFunction function, FunctionEmissionContext context)
    {
        uint paramIndex = 0;
        foreach (var local in function.Locals)
        {
            var classification = context.LocalClassification.Locals[local.Id];
            if (local is MirParameter)
            {
                var rawParam = context.LlvmFunction.GetParam(paramIndex);
                if (classification.StorageKind == MirLocalStorageKind.Memory)
                {
                    var alloca = _builder.BuildAlloca(GetOrCreateType(local.Type), local.Name);
                    context.BindLocal(local.Id, alloca);
                    _builder.BuildStore(rawParam, alloca);
                }
                else
                {
                    context.BindLocal(local.Id, rawParam);
                }

                paramIndex++;
            }
            else if (classification.StorageKind == MirLocalStorageKind.Memory)
            {
                var alloca = _builder.BuildAlloca(GetOrCreateType(local.Type), local.Name);
                context.BindLocal(local.Id, alloca);
            }
        }
    }

    private void EmitBlock(MirBasicBlock block, FunctionEmissionContext context)
    {
        var llvmBlock = context.LookupBlock(block.Id);
        if (block.Id != context.MirFunction.EntryBlock)
        {
            context.LlvmFunction.AppendExistingBasicBlock(llvmBlock);
        }

        context.CurrentBlock = block.Id;
        _builder.PositionAtEnd(llvmBlock);

        var phiLocals = context.LookupPhiLocals(block.Id);
        foreach (var local in phiLocals)
        {
            var (values, blocks) = context.LookupPhiValues(local);
            Debug.Assert(values.Length > 1);
            var localType = context.MirFunction.GetLocal(local).Type;
            var phi = _builder.BuildPhi(GetOrCreateType(localType));
            phi.AddIncoming(values, blocks, (uint)values.Length);
            context.BindLocal(local, phi);
        }

        foreach (var instruction in block.Instructions)
        {
            EmitInstruction(instruction, context);
        }

        EmitTerminator(block.Terminator, context);
    }

    private void EmitInstruction(MirInstruction instruction, FunctionEmissionContext context)
    {
        switch (instruction)
        {
            case MirAssignInstruction mirAssignInstruction:
                EmitAssignment(mirAssignInstruction, context);
                break;
            case MirUnaryInstruction mirUnaryInstruction:
                EmitUnaryOperation(mirUnaryInstruction, context);
                break;
            case MirBinaryInstruction mirBinaryInstruction:
                EmitBinaryOperation(mirBinaryInstruction, context);
                break;
            case MirCallInstruction mirCallInstruction:
                EmitCall(mirCallInstruction, context);
                break;
            case MirConvertInstruction mirConvertInstruction:
                EmitConvert(mirConvertInstruction, context);
                break;
            case MirStorageLiveInstruction mirStorageLiveInstruction:
                EmitStorageLive(mirStorageLiveInstruction, context);
                break;
            case MirStorageDeadInstruction mirStorageDeadInstruction:
                EmitStorageDead(mirStorageDeadInstruction, context);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(instruction));
        }
    }

    private void EmitAssignment(MirAssignInstruction assignment, FunctionEmissionContext context)
    {
        var source = GetValue(assignment.Source, context);
        EmitWriteToDest(assignment.Destination, source, context);
    }

    private void EmitUnaryOperation(MirUnaryInstruction operation, FunctionEmissionContext context)
    {
        var source = GetValue(operation.Value, context);
        var result = operation.Op switch
        {
            MirUnaryOp.Negation => operation.Value.Type.SpecialType.IsInteger
                ? _builder.BuildNeg(source)
                : _builder.BuildFNeg(source),
            MirUnaryOp.LogicalNot or MirUnaryOp.BitwiseNot => _builder.BuildNot(source),
            _ => throw new InvalidOperationException("Unknown unary operation"),
        };
        EmitWriteToDest(operation.Destination, result, context);
    }

    private void EmitBinaryOperation(
        MirBinaryInstruction operation,
        FunctionEmissionContext context
    )
    {
        var left = GetValue(operation.Left, context);
        var right = GetValue(operation.Right, context);
        var type = operation.Left.Type;
        var result = operation.Op switch
        {
            MirBinaryOp.Addition => type.SpecialType.IsInteger
                ? _builder.BuildAdd(left, right)
                : _builder.BuildFAdd(left, right),
            MirBinaryOp.Subtraction => type.SpecialType.IsInteger
                ? _builder.BuildSub(left, right)
                : _builder.BuildFSub(left, right),
            MirBinaryOp.Multiplication => type.SpecialType.IsInteger
                ? _builder.BuildMul(left, right)
                : _builder.BuildFMul(left, right),
            MirBinaryOp.Division => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.BuildSDiv(left, right),
                { IsUnsignedInteger: true } => _builder.BuildUDiv(left, right),
                _ => _builder.BuildFDiv(left, right),
            },
            MirBinaryOp.Modulo => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.BuildSRem(left, right),
                { IsUnsignedInteger: true } => _builder.BuildURem(left, right),
                _ => _builder.BuildFRem(left, right),
            },
            MirBinaryOp.BitwiseAnd => _builder.BuildAnd(left, right),
            MirBinaryOp.BitwiseOr => _builder.BuildOr(left, right),
            MirBinaryOp.BitwiseXor => _builder.BuildXor(left, right),
            MirBinaryOp.Equality => type.SpecialType.IsFloatingPoint
                ? _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, left, right)
                : _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, left, right),
            MirBinaryOp.NotEquals => type.SpecialType.IsFloatingPoint
                ? _builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE, left, right)
                : _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, left, right),
            MirBinaryOp.LessThan => type.SpecialType switch
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
            MirBinaryOp.LessThanOrEquals => type.SpecialType switch
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
            MirBinaryOp.GreaterThan => type.SpecialType switch
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
            MirBinaryOp.GreaterThanOrEquals => type.SpecialType switch
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
            MirBinaryOp.ShiftLeft => _builder.BuildShl(left, right),
            MirBinaryOp.ShiftRight => _builder.BuildAShr(left, right),
            MirBinaryOp.UnsignedShiftRight => _builder.BuildLShr(left, right),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null),
        };
        EmitWriteToDest(operation.Destination, result, context);
    }

    private void EmitCall(MirCallInstruction call, FunctionEmissionContext context)
    {
        var abi = _compilation.GetFunctionAbi(call.Callee);
        var function = call.Callee;
        var callee = GetOrCreateFunction(function);
        var isIndirectReturn = abi.Return.IsIndirect;
        var parameterCount = isIndirectReturn ? call.Arguments.Length + 1 : call.Arguments.Length;
        Span<LLVMValueRef> parameters = stackalloc LLVMValueRef[parameterCount];

        int offsetIndex;
        if (isIndirectReturn)
        {
            Debug.Assert(call.Destination is not null);
            parameters[0] = context.LookupLocal(call.Destination.LocalId);
            offsetIndex = 1;
        }
        else
        {
            offsetIndex = 0;
        }

        foreach (var (i, argument) in abi.Parameters.AsValueEnumerable().Index())
        {
            var value = call.Arguments[i];
            var llvmValue = GetValue(value, context);
            if (argument.IsIndirect)
            {
                var destination = CreateEntryAlloca(
                    llvmValue.TypeOf,
                    argument.Parameter.Name,
                    context
                );
                _builder.BuildStore(llvmValue, destination);
                parameters[i + offsetIndex] = destination;
            }
            else
            {
                parameters[i + offsetIndex] = llvmValue;
            }
        }

        var result = _builder.BuildCall2(callee.FunctionType, callee, parameters, "");
        if (call.Destination is not null)
        {
            EmitWriteToDest(call.Destination, result, context);
        }
    }

    private LLVMValueRef CreateEntryAlloca(
        LLVMTypeRef type,
        string name,
        FunctionEmissionContext context
    )
    {
        var entry = context.LlvmFunction.EntryBasicBlock;
        using var entryBuilder = _context.CreateBuilder();

        var instruction = entry.FirstInstruction;
        while (instruction is { IsNull: false, InstructionOpcode: LLVMOpcode.LLVMAlloca })
        {
            instruction = instruction.NextInstruction;
        }

        if (instruction.IsNull || instruction.InstructionOpcode == 0)
        {
            entryBuilder.PositionAtEnd(entry);
        }
        else
        {
            entryBuilder.PositionBefore(instruction);
        }

        return entryBuilder.BuildAlloca(type, name);
    }

    private void EmitConvert(MirConvertInstruction conversion, FunctionEmissionContext context)
    {
        var baseValue = GetValue(conversion.Value, context);
        var convertedValue = EmitScalarConversion(
            baseValue,
            conversion.Conversion,
            conversion.Value.Type,
            conversion.Destination.Type
        );
        EmitWriteToDest(conversion.Destination, convertedValue, context);
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

    private void EmitStorageLive(
        MirStorageLiveInstruction storageLive,
        FunctionEmissionContext context
    )
    {
        var classification = context.LocalClassification.Locals[storageLive.LocalId];
        if (!classification.IsIndirectStorage)
            return;

        var lifetimeStartFunction = GetLifetimeStartFunction();
        _builder.BuildCall2(
            lifetimeStartFunction.FunctionType,
            lifetimeStartFunction,
            [context.LookupLocal(storageLive.LocalId)],
            "start".AsSpan()
        );
    }

    private void EmitStorageDead(
        MirStorageDeadInstruction storageDead,
        FunctionEmissionContext context
    )
    {
        var classification = context.LocalClassification.Locals[storageDead.LocalId];
        if (!classification.IsIndirectStorage)
            return;

        var lifetimeEndFunction = GetLifetimeEndFunction();
        _builder.BuildCall2(
            lifetimeEndFunction.FunctionType,
            lifetimeEndFunction,
            [context.LookupLocal(storageDead.LocalId)],
            "end".AsSpan()
        );
    }

    private void EmitTerminator(MirTerminator instruction, FunctionEmissionContext context)
    {
        switch (instruction)
        {
            case MirBranchTerminator mirBranchTerminator:
            {
                var condition = ConvertByteBoolToI1IfNeeded(
                    GetValue(mirBranchTerminator.Condition, context)
                );
                _builder.BuildCondBr(
                    condition,
                    context.LookupBlock(mirBranchTerminator.WhenTrue),
                    context.LookupBlock(mirBranchTerminator.WhenFalse)
                );
                break;
            }
            case MirGotoTerminator mirGotoTerminator:
                _builder.BuildBr(context.LookupBlock(mirGotoTerminator.Target));
                break;
            case MirReturnTerminator mirReturnTerminator:
                if (mirReturnTerminator.Value is not null)
                {
                    _builder.BuildRet(GetValue(mirReturnTerminator.Value, context));
                }
                else
                {
                    _builder.BuildRetVoid();
                }

                break;
            case MirUnreachableTerminator:
                _builder.BuildUnreachable();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(instruction));
        }
    }

    private LLVMValueRef GetValue(MirValue value, FunctionEmissionContext context)
    {
        return value switch
        {
            MirAddressOfValue mirAddressOfValue => EmitTakeAddress(mirAddressOfValue, context),
            MirConstantValue mirConstantValue => MakeConstant(mirConstantValue.Constant),
            MirNullValue => throw new InvalidOperationException("Cannot get a null value"),
            MirReadValue mirReadValue => EmitReadValue(mirReadValue, context),
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
    }

    private LLVMValueRef EmitTakeAddress(
        MirAddressOfValue addressOf,
        FunctionEmissionContext context
    )
    {
        return EmitTakeAddress(addressOf.Place, context);
    }

    private LLVMValueRef EmitTakeAddress(MirPlace place, FunctionEmissionContext context)
    {
        return place switch
        {
            MirDerefPlace mirDerefPlace => GetValue(mirDerefPlace.Pointer, context),
            MirGlobalPlace mirGlobalPlace => GetOrCreateGlobal(mirGlobalPlace.Variable),
            MirIndexPlace mirIndexPlace => throw new NotImplementedException(),
            MirLocalPlace mirLocalPlace => context.LookupLocal(mirLocalPlace.LocalId),
            _ => throw new InvalidOperationException("Invalid place"),
        };
    }

    private LLVMValueRef EmitReadValue(MirReadValue read, FunctionEmissionContext context)
    {
        return read.Place switch
        {
            MirGlobalPlace mirGlobalPlace => EmitReadGlobal(mirGlobalPlace),
            MirIndexPlace mirIndexPlace => EmitReadIndex(mirIndexPlace, context),
            MirLocalPlace mirLocalPlace => EmitReadLocal(mirLocalPlace, context),
            MirDerefPlace mirDerefPlace => EmitDerefLocal(mirDerefPlace, context),
            _ => throw new InvalidOperationException("Invalid place"),
        };
    }

    private LLVMValueRef EmitReadGlobal(MirGlobalPlace place)
    {
        var global = GetOrCreateGlobal(place.Variable);
        var type = GetOrCreateType(place.Type);
        return _builder.BuildLoad2(type, global);
    }

    private LLVMValueRef EmitReadLocal(MirLocalPlace place, FunctionEmissionContext context)
    {
        var local = context.LookupLocal(place.LocalId);
        var classification = context.LocalClassification.Locals[place.LocalId];
        if (!classification.IsIndirectStorage)
            return local;

        var type = GetOrCreateType(place.Type);
        return _builder.BuildLoad2(type, local);
    }

    private LLVMValueRef EmitDerefLocal(MirDerefPlace place, FunctionEmissionContext context)
    {
        var location = GetValue(place.Pointer, context);
        return _builder.BuildLoad2(GetOrCreateType(place.Type), location);
    }

    private LLVMValueRef EmitReadIndex(MirIndexPlace indexer, FunctionEmissionContext context)
    {
        var index = GetValue(indexer.Index, context);
        var itemType = GetOrCreateType(indexer.Type);

        LLVMValueRef pointer;
        if (indexer.Base.Type.IsDynamicallySized)
        {
            var widePointer = EmitTakeAddress(indexer.Base, context);
            pointer = _builder.BuildExtractValue(widePointer, 0, "pointer");
        }
        else
        {
            pointer = EmitTakeAddress(indexer.Base, context);
        }

        var element = _builder.BuildGEP2(itemType, pointer, [index], "element".AsSpan());
        return _builder.BuildLoad2(itemType, element);
    }

    private void EmitWriteToDest(
        MirPlace destination,
        LLVMValueRef value,
        FunctionEmissionContext context
    )
    {
        switch (destination)
        {
            case MirGlobalPlace mirGlobalPlace:
                EmitAssignGlobal(mirGlobalPlace, value);
                break;
            case MirIndexPlace mirIndexPlace:
                throw new NotImplementedException();
            case MirLocalPlace mirLocalPlace:
                EmitAssignLocal(mirLocalPlace, value, context);
                break;
            case MirDerefPlace mirDerefPlace:
                EmitAssignDeref(mirDerefPlace, value, context);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(destination));
        }
    }

    private void EmitAssignGlobal(MirGlobalPlace place, LLVMValueRef value)
    {
        _builder.BuildStore(value, GetOrCreateGlobal(place.Variable));
    }

    private void EmitAssignLocal(
        MirLocalPlace destination,
        LLVMValueRef value,
        FunctionEmissionContext context
    )
    {
        var classification = context.LocalClassification.Locals[destination.LocalId];
        switch (classification.StorageKind)
        {
            case MirLocalStorageKind.Ssa:
                context.BindLocal(destination.LocalId, value);
                break;
            case MirLocalStorageKind.SsaWithPhi:
                context.AddPhiValue(destination.LocalId, value);
                break;
            case MirLocalStorageKind.Memory or MirLocalStorageKind.IndirectParam:
                {
                    var local = context.LookupLocal(destination.LocalId);
                    _builder.BuildStore(value, local);
                }
                break;
            default:
                throw new InvalidOperationException("Unknown enum type");
        }
    }

    private void EmitAssignDeref(
        MirDerefPlace destination,
        LLVMValueRef value,
        FunctionEmissionContext context
    )
    {
        var location = GetValue(destination.Pointer, context);
        _builder.BuildStore(value, location);
    }

    private void WriteIR()
    {
        var targetPath = Path.Combine(_options.OutputDirectory, $"{_compilation.AssemblyName}.ll");
        _module.PrintToFile(targetPath);
    }

    private EmitResult OutputBinary(BindingContext context)
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

        _module.Verify(LLVMVerifierFailureAction.LLVMPrintMessageAction);
        if (
            !targetMachine.TryEmitToFile(
                _module,
                outputFilename,
                LLVMCodeGenFileType.LLVMObjectFile,
                out _
            )
        )
        {
            return new EmitResult(false, context.CollectDiagnostics());
        }

        return new EmitResult(true, context.CollectDiagnostics());
    }

    public void Dispose()
    {
        _builder.Dispose();
        _module.Dispose();
        _context.Dispose();
    }
}
