// @file LlvmCodeEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using LLVMSharp.Interop;
using Prism.Core.Configuration;
using Prism.Core.Mir;
using Prism.Core.Mir.Analysis;
using Prism.Core.Semantic;
using ZLinq;

namespace Prism.Core.Codegen;

internal sealed class LlvmCodeEmitter : IDisposable
{
    private readonly MirModule _mirModule;
    private readonly CompilationSettings _settings;
    private readonly CodeGenOptions _options;

    private LLVMContextRef _context;
    private LLVMModuleRef _module;
    private LLVMBuilderRef _builder;

    private readonly Dictionary<MirGlobalId, LLVMValueRef> _globals = new();
    private readonly Dictionary<MirFunctionId, LLVMValueRef> _functions = new();
    private readonly Dictionary<MirType, LLVMTypeRef> _typeMap = new();

    public LlvmCodeEmitter(MirModule module, CompilationSettings settings, CodeGenOptions options)
    {
        _mirModule = module;
        _settings = settings;
        _options = options;
        _context = LLVMContextRef.Create();
        _module = _context.CreateModuleWithName(module.Name);
        _builder = _context.CreateBuilder();
    }

    public EmitResult Emit()
    {
        foreach (var global in _mirModule.Globals)
        {
            EmitGlobal(global);
        }

        foreach (var function in _mirModule.Functions)
        {
            EmitFunction(function);
        }

        WriteIR();
        return OutputBinary();
    }

    private LLVMTypeRef GetOrCreateType(MirType symbol)
    {
        if (_typeMap.TryGetValue(symbol, out var type))
        {
            return type;
        }

        type = CreateType(symbol);
        _typeMap[symbol] = type;
        return type;
    }

    private LLVMTypeRef CreateType(MirType symbol)
    {
        switch (symbol)
        {
            case MirArrayType mirArrayType:
                return LLVMTypeRef.CreateArray2(
                    CreateType(mirArrayType.ElementType),
                    mirArrayType.Length
                );
            case MirBoolType:
                return _context.Int8Type;
            case MirFloatType mirFloatType:
                return mirFloatType.Bits switch
                {
                    32 => _context.FloatType,
                    64 => _context.DoubleType,
                    _ => throw new InvalidOperationException("Invalid float type"),
                };
            case MirIntType mirIntType:
                return _context.GetIntType((uint)mirIntType.Bits);
            case MirPointerType:
                return _context.CreatePointerType(0);
            case MirSliceType:
            {
                var sizeType = _context.GetIntType((uint)_settings.PointerWidth.BitWidth);
                return LLVMTypeRef.CreateStruct([_context.CreatePointerType(0), sizeType], false);
            }
            case MirStructType mirStructType:
            {
                using var types = mirStructType
                    .Fields.AsValueEnumerable()
                    .Select(CreateType)
                    .ToArrayPool();
                var namedStruct = _context.CreateNamedStruct(mirStructType.Name);
                namedStruct.GetStructElementTypes(types.Span);
                return namedStruct;
            }
            case MirVoidType:
                return _context.VoidType;
            default:
                throw new ArgumentOutOfRangeException(nameof(symbol));
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
                _settings.PointerWidth switch
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
                _settings.PointerWidth switch
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

    private void EmitGlobal(MirGlobal global)
    {
        var type = GetOrCreateType(global.Type);
        var created = _module.AddGlobal(type, global.Name);
        created.Initializer = global.Initializer switch
        {
            MirConstantGlobalInitializer mirConstantGlobalInitializer => MakeConstant(
                mirConstantGlobalInitializer.Value.Constant
            ),
            MirComputedGlobalInitializer or MirNoGlobalInitializer => LLVMValueRef.CreateConstNull(
                type
            ),
            _ => throw new InvalidOperationException("Unknown global initializer type"),
        };
        _globals[global.Id] = created;
    }

    private LLVMValueRef GetOrCreateFunction(MirFunction function)
    {
        if (_functions.TryGetValue(function.Id, out var llvmValue))
        {
            return llvmValue;
        }

        var returnType = GetOrCreateType(function.ReturnType);
        using var parameters = function
            .Locals.AsValueEnumerable()
            .Where(l => l.Kind == MirLocalKind.Parameter)
            .Select(p => GetOrCreateType(p.Type))
            .ToArrayPool();
        var functionType = LLVMTypeRef.CreateFunction(returnType, parameters.Span, false);
        llvmValue = _module.AddFunction(function.Name, functionType);
        _functions[function.Id] = llvmValue;
        return llvmValue;
    }

    private void EmitFunction(MirFunction function)
    {
        var llvmFunction = GetOrCreateFunction(function);

        var cfg = MirFunctionAnalyzer.AnalyzeControlFlow(function);
        var localInfos = MirFunctionAnalyzer.AnalyzeLocalFlow(function, cfg);
        var localClassification = MirFunctionAnalyzer.ClassifyLocals(localInfos);

        var context = new FunctionEmissionContext(function, llvmFunction, cfg, localClassification);
        foreach (var block in function.Blocks)
        {
            var blockRef = LLVMBasicBlockRef.CreateInContext(_context, block.Name);
            context.BindBlock(block.Id, blockRef);
        }

        var entryBlock = context.LookupBlock(function.EntryBlock);
        llvmFunction.AppendExistingBasicBlock(entryBlock);
        _builder.PositionAtEnd(entryBlock);

        InitializeLocalVariables(function, context);
        foreach (var block in function.Blocks)
        {
            EmitBlock(block, context);
        }
    }

    private void InitializeLocalVariables(MirFunction function, FunctionEmissionContext context)
    {
        uint paramIndex = 0;
        foreach (var local in function.Locals)
        {
            var classification = context.LocalClassification.Locals[local.Id];
            if (local.Kind == MirLocalKind.Parameter)
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
            MirUnaryOp.Negation => operation.Value.Type switch
            {
                MirIntType => _builder.BuildNeg(source),
                MirFloatType => _builder.BuildFNeg(source),
                _ => throw new InvalidOperationException("Unknown type for negation"),
            },
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
        var result = operation.Op switch
        {
            MirBinaryOp.Add => operation.Left.Type switch
            {
                MirIntType => _builder.BuildAdd(left, right),
                MirFloatType => _builder.BuildFAdd(left, right),
                _ => throw new InvalidOperationException("Invalid operand type"),
            },
            MirBinaryOp.Subtract => operation.Left.Type switch
            {
                MirIntType => _builder.BuildSub(left, right),
                MirFloatType => _builder.BuildFSub(left, right),
                _ => throw new InvalidOperationException("Invalid operand type"),
            },
            MirBinaryOp.Multiply => operation.Left.Type switch
            {
                MirIntType => _builder.BuildMul(left, right),
                MirFloatType => _builder.BuildFMul(left, right),
                _ => throw new InvalidOperationException("Invalid operand type"),
            },
            MirBinaryOp.Divide => operation.Left.Type switch
            {
                MirIntType { Signed: true } => _builder.BuildSDiv(left, right),
                MirIntType { Signed: false } => _builder.BuildUDiv(left, right),
                MirFloatType => _builder.BuildFDiv(left, right),
                _ => throw new InvalidOperationException("Invalid operand type"),
            },
            MirBinaryOp.Modulo => operation.Left.Type switch
            {
                MirIntType { Signed: true } => _builder.BuildSRem(left, right),
                MirIntType { Signed: false } => _builder.BuildURem(left, right),
                MirFloatType => _builder.BuildFRem(left, right),
                _ => throw new InvalidOperationException("Invalid operand type"),
            },
            MirBinaryOp.BitwiseAnd => _builder.BuildAnd(left, right),
            MirBinaryOp.BitwiseOr => _builder.BuildOr(left, right),
            MirBinaryOp.BitwiseXor => _builder.BuildXor(left, right),
            MirBinaryOp.ShiftLeft => _builder.BuildShl(left, right),
            MirBinaryOp.ShiftRight => _builder.BuildAShr(left, right),
            MirBinaryOp.UnsignedShiftRight => _builder.BuildLShr(left, right),
            MirBinaryOp.Equal => operation.Left.Type switch
            {
                MirIntType or MirPointerType or MirBoolType => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntEQ,
                    left,
                    right
                ),
                MirFloatType => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, left, right),
                _ => throw new InvalidOperationException("Invalid operand type"),
            },
            MirBinaryOp.NotEqual => operation.Left.Type switch
            {
                MirIntType or MirPointerType or MirBoolType => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntNE,
                    left,
                    right
                ),
                MirFloatType => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE, left, right),
                _ => throw new InvalidOperationException("Invalid operand type"),
            },
            MirBinaryOp.LessThan => operation.Left.Type switch
            {
                MirIntType { Signed: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntSLT,
                    left,
                    right
                ),
                MirIntType { Signed: false } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntULT,
                    left,
                    right
                ),
                MirFloatType => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLT, left, right),
                _ => throw new InvalidOperationException("Invalid operand type"),
            },
            MirBinaryOp.LessThanOrEqual => operation.Left.Type switch
            {
                MirIntType { Signed: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntSLE,
                    left,
                    right
                ),
                MirIntType { Signed: false } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntULE,
                    left,
                    right
                ),
                MirFloatType => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLE, left, right),
                _ => throw new InvalidOperationException("Invalid operand type"),
            },
            MirBinaryOp.GreaterThan => operation.Left.Type switch
            {
                MirIntType { Signed: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntSGT,
                    left,
                    right
                ),
                MirIntType { Signed: false } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntUGT,
                    left,
                    right
                ),
                MirFloatType => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGT, left, right),
                _ => throw new InvalidOperationException("Invalid operand type"),
            },
            MirBinaryOp.GreaterThanOrEqual => operation.Left.Type switch
            {
                MirIntType { Signed: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntSGE,
                    left,
                    right
                ),
                MirIntType { Signed: false } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntUGE,
                    left,
                    right
                ),
                MirFloatType => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGE, left, right),
                _ => throw new InvalidOperationException("Invalid operand type"),
            },
            _ => throw new InvalidOperationException("Invalid binary operation"),
        };
        EmitWriteToDest(operation.Destination, result, context);
    }

    private void EmitCall(MirCallInstruction call, FunctionEmissionContext context)
    {
        var function = _mirModule.GetFunction(call.Callee);
        var callee = GetOrCreateFunction(function);
        using var parameters = call
            .Arguments.AsValueEnumerable()
            .Select(p => GetValue(p, context))
            .ToArrayPool();
        var result = _builder.BuildCall2(callee.FunctionType, callee, parameters.Span, "");
        if (call.Destination is not null)
        {
            EmitWriteToDest(call.Destination, result, context);
        }
    }

    private void EmitConvert(MirConvertInstruction conversion, FunctionEmissionContext context)
    {
        var baseValue = GetValue(conversion.Value, context);
        var type = GetOrCreateType(conversion.Destination.Type);
        var convertedValue = conversion.Kind switch
        {
            MirConversionKind.Identity => baseValue,
            MirConversionKind.ZeroExtend => _builder.BuildZExt(baseValue, type),
            MirConversionKind.SignExtend => _builder.BuildSExt(baseValue, type),
            MirConversionKind.Truncate => _builder.BuildTrunc(baseValue, type),
            MirConversionKind.FloatExtend => _builder.BuildFPExt(baseValue, type),
            MirConversionKind.FloatTruncate => _builder.BuildFPTrunc(baseValue, type),
            MirConversionKind.SignedIntToFloat => _builder.BuildSIToFP(baseValue, type),
            MirConversionKind.UnsignedIntToFloat => _builder.BuildUIToFP(baseValue, type),
            MirConversionKind.FloatToSignedInt => _builder.BuildFPToSI(baseValue, type),
            MirConversionKind.FloatToUnsignedInt => _builder.BuildFPToUI(baseValue, type),
            MirConversionKind.BitCast => _builder.BuildBitCast(baseValue, type),
            _ => throw new InvalidOperationException("Unknown conversion kind"),
        };
        EmitWriteToDest(conversion.Destination, convertedValue, context);
    }

    private void EmitStorageLive(
        MirStorageLiveInstruction storageLive,
        FunctionEmissionContext context
    ) { }

    private void EmitStorageDead(
        MirStorageDeadInstruction storageDead,
        FunctionEmissionContext context
    ) { }

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
        return addressOf.Place switch
        {
            MirDerefPlace mirDerefPlace => throw new NotImplementedException(),
            MirFieldPlace mirFieldPlace => throw new NotImplementedException(),
            MirGlobalPlace mirGlobalPlace => _globals[mirGlobalPlace.GlobalId],
            MirIndexPlace mirIndexPlace => throw new NotImplementedException(),
            MirLocalPlace mirLocalPlace => context.LookupLocal(mirLocalPlace.LocalId),
            _ => throw new InvalidOperationException("Invalid place"),
        };
    }

    private LLVMValueRef EmitReadValue(MirReadValue read, FunctionEmissionContext context)
    {
        return read.Place switch
        {
            MirFieldPlace mirFieldPlace => throw new NotImplementedException(),
            MirGlobalPlace mirGlobalPlace => EmitReadGlobal(mirGlobalPlace),
            MirIndexPlace mirIndexPlace => throw new NotImplementedException(),
            MirLocalPlace mirLocalPlace => EmitReadLocal(mirLocalPlace, context),
            MirDerefPlace mirDerefPlace => EmitDerefLocal(mirDerefPlace, context),
            _ => throw new InvalidOperationException("Invalid place"),
        };
    }

    private LLVMValueRef EmitReadGlobal(MirGlobalPlace place)
    {
        var global = _globals[place.GlobalId];
        var type = GetOrCreateType(place.Type);
        return _builder.BuildLoad2(type, global);
    }

    private LLVMValueRef EmitReadLocal(MirLocalPlace place, FunctionEmissionContext context)
    {
        var local = context.LookupLocal(place.LocalId);
        var classification = context.LocalClassification.Locals[place.LocalId];
        if (classification.StorageKind != MirLocalStorageKind.Memory)
            return local;

        var type = GetOrCreateType(place.Type);
        return _builder.BuildLoad2(type, local);
    }

    private LLVMValueRef EmitDerefLocal(MirDerefPlace place, FunctionEmissionContext context)
    {
        var location = GetValue(place.Pointer, context);
        return _builder.BuildLoad2(GetOrCreateType(place.Type), location);
    }

    private void EmitWriteToDest(
        MirPlace destination,
        LLVMValueRef value,
        FunctionEmissionContext context
    )
    {
        switch (destination)
        {
            case MirFieldPlace mirFieldPlace:
                throw new NotImplementedException();
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
        _builder.BuildStore(value, _globals[place.GlobalId]);
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
            case MirLocalStorageKind.Memory:
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
        var targetPath = Path.Combine(_options.OutputDirectory, $"{_mirModule.Name}.ll");
        _module.PrintToFile(targetPath);
    }

    private EmitResult OutputBinary()
    {
        LLVM.InitializeNativeTarget();
        LLVM.InitializeNativeAsmPrinter();
        LLVM.InitializeNativeAsmParser();

        var tripleString = _settings.GetLlvmTriple();
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

        var outputFilename = Path.Combine(_options.OutputDirectory, $"{_mirModule.Name}.obj");

        _module.Verify(LLVMVerifierFailureAction.LLVMReturnStatusAction);
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
