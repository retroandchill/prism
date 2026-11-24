// @file LlvmCodeEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Cysharp.Text;
using LLVMSharp.Interop;
using Prism.Abi;
using Prism.Compiler;
using Prism.Compiler.Binding;
using Prism.Compiler.Codegen;
using Prism.Compiler.Mappers;
using Prism.Configuration;
using Prism.Constants;
using Prism.Diagnostics;
using Prism.Mir;
using Prism.Mir.Analysis;
using Prism.Semantic.Conversions;
using Prism.Semantic.Layout;
using Prism.Semantic.Symbols;
using Prism.Utils;
using Utf8StringInterpolation;
using ZLinq;
using FloatKind = Prism.Semantic.Layout.FloatKind;

namespace Prism.Codegen.Llvm;

internal sealed class LlvmCodeEmitter : ICodeEmitter
{
    private readonly Compilation _compilation;
    private readonly CodeGenOptions _options;

    private const uint NullTagIndex = 0;
    private const uint NullPayloadIndex = 1;
    private const uint SliceDataIndex = 0;
    private const uint SliceLengthIndex = 1;

    private LLVMContextRef _context;
    private LLVMModuleRef _module;
    private LLVMBuilderRef _builder;
    private LlvmDebugInfo? _debugInfo;

    private readonly Dictionary<Symbol, LLVMValueRef> _symbolToValue = [];

    private readonly Dictionary<TypeLayout, LLVMTypeRef> _layoutToType = [];

    private static readonly uint ByValAttr = LookupEnumAttributeKind("byval");
    private LLVMValueRef? _lifetimeStartFunction;
    private LLVMValueRef? _lifetimeEndFunction;

    private readonly LLVMValueRef?[] _threeWayComparisonIntrinsics = new LLVMValueRef?[
        IntegerKind.MaxValue - 1
    ];

    private LLVMValueRef? _nullOrderingValue;

    private string? _entryPoint;

    public LlvmCodeEmitter(Compilation compilation, CodeGenOptions options)
    {
        _compilation = compilation;
        _options = options;
        _context = LLVMContextRef.Create();
        _module = _context.CreateModuleWithName(compilation.AssemblyName);
        _builder = _context.CreateBuilder();
        _debugInfo = compilation.Settings.GenerateDebugInfo
            ? new LlvmDebugInfo(_compilation, _module)
            : null;
    }

    public async Task<EmitResult> EmitAsync(
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        return !context.HasErrors
            ? await OutputBinaryAsync(context, cancellationToken)
            : new EmitResult(false, context.CollectDiagnostics());
    }

    public void AddGlobalConstant(
        VariableSymbol variable,
        BindingContext bindingContext,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var llvmVariable = GetOrCreateGlobal(variable);
        llvmVariable.IsGlobalConstant = true;

        var constant = variable.ConstantValue;
        Debug.Assert(constant is not null);
        llvmVariable.Initializer = MakeConstant(constant.Value, variable.Type);
    }

    public void AddGlobalVariable(
        VariableInitializer variable,
        BindingContext bindingContext,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var llvmVariable = GetOrCreateGlobal(variable.Symbol);
        if (variable.Value is not ConstantValue constantValue)
            return;

        llvmVariable.Initializer = MakeConstant(constantValue, variable.Symbol.Type);
    }

    public void AddFunction(
        Function function,
        BindingContext bindingContext,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var llvmFunction = GetOrCreateFunction(function.Symbol);

        var classifiedLocals = LocalClassification.GetClassifications(
            _compilation,
            function,
            cancellationToken
        );

        var context = new FunctionEmissionContext(function, llvmFunction, classifiedLocals);
        foreach (var block in function.Blocks)
        {
            var blockRef = LLVMBasicBlockRef.CreateInContext(_context, block.Name);
            context.BindBlock(block, blockRef);
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

    public void RegisterGlobalConstructor(FunctionSymbol function)
    {
        _module.AppendToGlobalCtors(GetOrCreateFunction(function), 65535);
    }

    public void RegisterEntryPoint(FunctionSymbol entryPoint)
    {
        _entryPoint = "main";
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

        var extraParams = 0;
        if (indirectReturn)
            extraParams++;
        if (abi.Receiver is not null)
            extraParams++;

        var totalParameters = functionSymbol.Parameters.Length + extraParams;
        Span<LLVMTypeRef> parameters = stackalloc LLVMTypeRef[totalParameters];

        LLVMTypeRef returnType;
        int firstParamIndex;
        if (indirectReturn)
        {
            returnType = _context.VoidType;
            parameters[0] = _context.CreatePointerType(0);
            firstParamIndex = 1;
        }
        else
        {
            returnType = GetOrCreateType(functionSymbol.ReturnType);
            firstParamIndex = 0;
        }

        if (abi.Receiver is not null)
        {
            if (abi.Receiver is AbiValueClassification.Indirect)
            {
                parameters[firstParamIndex++] = _context.CreatePointerType(0);
            }
            else
            {
                parameters[firstParamIndex++] = GetOrCreateType(
                    functionSymbol.ReceiverType.RequireNonNull()
                );
            }
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

        var name = functionSymbol.Mangle(_compilation);
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

        if (abi.Receiver is AbiValueClassification.Indirect)
        {
            var byValAttr = _context.CreateTypeAttribute(
                ByValAttr,
                GetOrCreateType(functionSymbol.ReceiverType.RequireNonNull())
            );
            func.AddAttributeAtIndex((LLVMAttributeIndex)(1 + (indirectReturn ? 1 : 0)), byValAttr);
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

        var id = LookupIntrinsicId("llvm.lifetime.start.p0"u8);
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

        var id = LookupIntrinsicId("llvm.lifetime.end.p0"u8);
        _lifetimeEndFunction = _module.GetIntrinsicDeclaration(id, [_context.CreatePointerType(0)]);
        return _lifetimeEndFunction.Value;
    }

    private static uint LookupIntrinsicId(ref Utf8StringWriter<ArrayBufferWriter<byte>> writer)
    {
        writer.Flush();
        return LookupIntrinsicId(writer.GetBufferWriter().WrittenSpan);
    }

    private static unsafe uint LookupIntrinsicId(ReadOnlySpan<byte> name)
    {
        fixed (byte* p = name)
        {
            return LLVM.LookupIntrinsicID((sbyte*)p, (nuint)name.Length);
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
        if (symbol.IsVoid)
            return _context.VoidType;

        var layout = _compilation.GetTypeLayout(symbol);
        return GetOrCreateType(layout);
    }

    private LLVMTypeRef GetOrCreateType(TypeLayout layout)
    {
        if (_layoutToType.TryGetValue(layout, out var type))
        {
            return type;
        }

        type = CreateType(layout);
        _layoutToType[layout] = type;
        return type;
    }

    private LLVMTypeRef CreateType(TypeLayout layout)
    {
        return layout switch
        {
            EmptyLayout => _context.VoidType,
            IntegerLayout integer => _context.GetIntType((uint)integer.BitWidth),
            FloatLayout floating => floating.Kind switch
            {
                FloatKind.Half => _context.HalfType,
                FloatKind.Single => _context.FloatType,
                FloatKind.Double => _context.DoubleType,
                FloatKind.FP128 => _context.FP128Type,
                FloatKind.X86_FP80 => _context.X86FP80Type,
                _ => throw new ArgumentException("Invalid floating point kind"),
            },
            PointerLayout pointer => _context.CreatePointerType(pointer.AddressSpace),
            ArrayLayout arrayLayout => LLVMTypeRef.CreateArray2(
                GetOrCreateType(arrayLayout.ElementType),
                arrayLayout.ElementCount
            ),
            StructLayout structLayout => _context.GetStructType(
                structLayout.Fields.Select(f => GetOrCreateType(f.Type)).ToArray(),
                structLayout.IsPacked
            ),
            TaggedNullableLayout nullableLayout => GetOrCreateType(
                nullableLayout.StructRepresentation
            ),
            OptimizedNullableLayout nullableLayout => GetOrCreateType(
                nullableLayout.UnderlyingLayout
            ),
        };
    }

    private LLVMValueRef MakeConstant(in ConstantValue value, TypeSymbol type)
    {
        var innerValue = value switch
        {
            NullConstant => GetNullValue(type),
            bool v => LLVMValueRef.CreateConstInt(_context.Int8Type, v ? 1UL : 0UL),
            CharacterConstant ch => ch switch
            {
                byte v => LLVMValueRef.CreateConstInt(_context.Int8Type, v),
                char v => LLVMValueRef.CreateConstInt(_context.Int16Type, v),
                Rune v => LLVMValueRef.CreateConstInt(_context.Int32Type, (ulong)v.Value),
            },
            IntegerConstant i => i switch
            {
                sbyte v => LLVMValueRef.CreateConstInt(
                    _context.Int8Type,
                    unchecked((ulong)v),
                    true
                ),
                byte v => LLVMValueRef.CreateConstInt(_context.Int8Type, v),
                short v => LLVMValueRef.CreateConstInt(
                    _context.Int16Type,
                    unchecked((ulong)v),
                    true
                ),
                ushort v => LLVMValueRef.CreateConstInt(_context.Int16Type, v),
                int v => LLVMValueRef.CreateConstInt(_context.Int32Type, unchecked((ulong)v), true),
                uint v => LLVMValueRef.CreateConstInt(_context.Int32Type, v),
                long v => LLVMValueRef.CreateConstInt(
                    _context.Int64Type,
                    unchecked((ulong)v),
                    true
                ),
                ulong v => LLVMValueRef.CreateConstInt(_context.Int64Type, v),
                Int128 v => LLVMValueRef.CreateConstInt(_context.Int128Type, v),
                UInt128 v => LLVMValueRef.CreateConstInt(_context.Int128Type, v),
            },
            FloatConstant f => f switch
            {
                float v => LLVMValueRef.CreateConstReal(_context.FloatType, v),
                double v => LLVMValueRef.CreateConstReal(_context.DoubleType, v),
            },
            string v => CreateStringConstant(v, type),
        };

        var structType = GetOrCreateType(type);
        if (type is not NullableTypeSymbol || innerValue.TypeOf == structType)
            return innerValue;

        var hasValue = LLVMValueRef.CreateConstInt(
            GetOrCreateType(_compilation.GetSpecialType(SpecialType.Bool)),
            1
        );
        return LLVMValueRef.CreateConstNamedStruct(structType, [hasValue, innerValue]);
    }

    private LLVMValueRef CreateStringConstant(string value, TypeSymbol type)
    {
        var globalString = _builder.BuildGlobalString(value);
        var length = (ulong)Encoding.UTF8.GetByteCount(value);
        var lengthValue = LLVMValueRef.CreateConstInt(
            GetOrCreateType(_compilation.GetSpecialType(SpecialType.USize)),
            length
        );
        var structType = GetOrCreateType(type);
        return LLVMValueRef.CreateConstNamedStruct(structType, [globalString, lengthValue]);
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

    private void InitializeLocalVariables(Function function, FunctionEmissionContext context)
    {
        uint paramIndex = 0;
        var abi = _compilation.GetFunctionAbi(function.Symbol);

        foreach (var local in function.Locals)
        {
            var classification = context.LocalClassification[local];
            if (
                local.Id == LocalId.ReturnSlot
                || (local.Id == LocalId.ThisSlot && function.Symbol.ReceiverType is not null)
                || local.AssociatedSymbol is ParameterSymbol
            )
            {
                var rawParam = context.LlvmFunction.GetParam(paramIndex);
                if (classification.StorageKind == LocalStorageKind.StackSlot)
                {
                    var layout = _compilation.GetTypeLayout(local.Type);
                    var alloca = _builder.BuildAlloca(GetOrCreateType(layout));
                    alloca.SetAlignment((uint)layout.Alignment);
                    context.BindLocal(local, alloca);
                    _builder.BuildStore(rawParam, alloca);
                }
                else
                {
                    context.BindLocal(local, rawParam);
                }

                if (local.Id == LocalId.ReturnSlot && !abi.Return.IsIndirect)
                    continue;

                paramIndex++;
            }
            else if (classification.StorageKind == LocalStorageKind.StackSlot)
            {
                var layout = _compilation.GetTypeLayout(local.Type);
                var alloca = _builder.BuildAlloca(GetOrCreateType(layout));
                alloca.SetAlignment((uint)layout.Alignment);
                context.BindLocal(local, alloca);
            }
        }
    }

    private void EmitBlock(BasicBlock block, FunctionEmissionContext context)
    {
        var llvmBlock = context.LookupBlock(block);
        if (block != context.MirFunction.EntryBlock)
        {
            context.LlvmFunction.AppendExistingBasicBlock(llvmBlock);
        }

        _builder.PositionAtEnd(llvmBlock);

        foreach (var instruction in block.Instructions)
        {
            EmitInstruction(instruction, context);
        }

        EmitTerminator(block.Terminator, context);
    }

    private void EmitInstruction(Instruction instruction, FunctionEmissionContext context)
    {
        switch (instruction)
        {
            case AssignmentInstruction(var destination, var source):
                EmitAssignment(destination, source, context);
                break;
            case StorageDeadInstruction(var local):
                EmitStorageDead(local, context);
                break;
            case StorageLiveInstruction(var local):
                EmitStorageLive(local, context);
                break;
            default:
                throw new InvalidOperationException(
                    "We probably added an instruction without updating the switch case"
                );
        }
    }

    private bool CanTypeBeLoadedAsSsa(TypeSymbol type)
    {
        if (type is ArrayTypeSymbol)
            return false;

        return _compilation.GetTypeLayout(type).Size <= 16;
    }

    private void EmitAssignment(Place destination, RValue rvalue, FunctionEmissionContext context)
    {
        if (DestinationIsSsa(destination, context, out var local))
        {
            var value = EmitRValueAsValue(rvalue, destination.Type, context);
            context.BindLocal(local, value);
        }
        else
        {
            EmitRValueTo(destination, rvalue, context);
        }
    }

    private static bool DestinationIsSsa(
        Place destination,
        FunctionEmissionContext context,
        [NotNullWhen(true)] out Local? local
    )
    {
        if (destination is not { Base: LocalPlace(var foundLocal), Projections: [] })
        {
            local = null;
            return false;
        }

        local = foundLocal;
        var classification = context.LocalClassification[local];
        return !classification.IsStoredInMemory;
    }

    private LLVMValueRef EmitRValueAsValue(
        RValue rvalue,
        TypeSymbol targetType,
        FunctionEmissionContext context
    )
    {
        return rvalue switch
        {
            UseRValue(var operand) => EmitOperandAsValue(operand, context),
            UnaryRValue(var @operator, var operand) => EmitUnary(@operator, operand, context),
            BinaryRValue(var @operator, var left, var right) => EmitBinary(
                @operator,
                left,
                right,
                context
            ),
            IsNullRValue(var operand) => EmitIsNull(operand, context),
            IsNotNullRValue(var operand) => EmitIsNotNull(operand, context),
            SelectRValue(var condition, var left, var right) => EmitSelect(
                condition,
                left,
                right,
                context
            ),
            ConversionRValue(var operand, var conversion, _) => EmitConversion(
                operand,
                conversion,
                targetType,
                context
            ),
            ReferenceRValue(var place, _) => EmitTakeAddress(place, context).Pointer,
            DataRValue(var data) => EmitData(data, context),
            LengthRValue(var operand) => EmitLength(operand, context),
            ArrayRValue => throw new InvalidOperationException(
                "Arrays should always occupy stack memory"
            ),
        };
    }

    private void EmitRValueTo(Place destination, RValue rvalue, FunctionEmissionContext context)
    {
        switch (rvalue)
        {
            case UseRValue(var operand):
                EmitOperandTo(destination, operand, context);
                break;
            case UnaryRValue(var @operator, var operand):
                _builder.BuildStore(
                    EmitUnary(@operator, operand, context),
                    EmitTakeAddress(destination, context).Pointer
                );
                break;
            case BinaryRValue(var @operator, var left, var right):
                _builder.BuildStore(
                    EmitBinary(@operator, left, right, context),
                    EmitTakeAddress(destination, context).Pointer
                );
                break;
            case IsNullRValue(var operand):
                _builder.BuildStore(
                    EmitIsNull(operand, context),
                    EmitTakeAddress(destination, context).Pointer
                );
                break;
            case IsNotNullRValue(var operand):
                _builder.BuildStore(
                    EmitIsNotNull(operand, context),
                    EmitTakeAddress(destination, context).Pointer
                );
                break;
            case SelectRValue(var condition, var left, var right):
                _builder.BuildStore(
                    EmitSelect(condition, left, right, context),
                    EmitTakeAddress(destination, context).Pointer
                );
                break;
            case ConversionRValue(var operand, var conversion, _):
                _builder.BuildStore(
                    EmitConversion(operand, conversion, destination.Type, context),
                    EmitTakeAddress(destination, context).Pointer
                );
                break;
            case ArrayRValue(var elements):
                EmitArrayTo(destination, elements, context);
                break;
            case ReferenceRValue(var place, _):
                _builder.BuildStore(
                    EmitTakeAddress(place, context).Pointer,
                    EmitTakeAddress(destination, context).Pointer
                );
                break;
            case DataRValue(var data):
                _builder.BuildStore(
                    EmitData(data, context),
                    EmitTakeAddress(destination, context).Pointer
                );
                break;
            case LengthRValue(var operand):
                _builder.BuildStore(
                    EmitLength(operand, context),
                    EmitTakeAddress(destination, context).Pointer
                );
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(rvalue));
        }
    }

    private LLVMValueRef EmitOperandAsValue(Operand operand, FunctionEmissionContext context)
    {
        return operand switch
        {
            ConstantOperand(var constant, var type) => MakeConstant(in constant, type),
            PlaceOperand(var place) => EmitPlaceAsValue(place, context),
        };
    }

    private void EmitOperandTo(Place destination, Operand operand, FunctionEmissionContext context)
    {
        var address = EmitTakeAddress(destination, context);
        EmitOperandTo(in address, operand, context);
    }

    private void EmitOperandTo(
        in LoweredAddress address,
        Operand operand,
        FunctionEmissionContext context
    )
    {
        switch (operand)
        {
            case ConstantOperand(var constant, var type):
                _builder.BuildStore(MakeConstant(in constant, type), address.Pointer);
                break;
            case PlaceOperand(var source):
            {
                if (DestinationIsSsa(source, context, out var local))
                {
                    _builder.BuildStore(context.LookupLocal(local), address.Pointer);
                }
                else if (CanTypeBeLoadedAsSsa(source.Type))
                {
                    var sourceAddress = EmitTakeAddress(source, context).Pointer;
                    var type = GetOrCreateType(source.Type);
                    var value = _builder.BuildLoad2(type, sourceAddress);
                    _builder.BuildStore(value, address.Pointer);
                }
                else
                {
                    var sourceAddress = EmitTakeAddress(source, context);
                    var layout = _compilation.GetTypeLayout(source.Type);
                    var type = GetOrCreateType(layout);
                    var alignment = (uint)layout.Alignment;
                    if (CanUseMemCopy(in sourceAddress, in address))
                    {
                        _builder.BuildMemCpy(
                            address.Pointer,
                            alignment,
                            sourceAddress.Pointer,
                            alignment,
                            type.SizeOf
                        );
                    }
                    else
                    {
                        _builder.BuildMemMove(
                            address.Pointer,
                            alignment,
                            sourceAddress.Pointer,
                            alignment,
                            type.SizeOf
                        );
                    }
                }

                break;
            }
        }
    }

    private LLVMValueRef EmitPlaceAsValue(Place place, FunctionEmissionContext context)
    {
        if (DestinationIsSsa(place, context, out var local))
        {
            return context.LookupLocal(local);
        }

        var address = EmitTakeAddress(place, context);
        var type = GetOrCreateType(place.Type);
        return _builder.BuildLoad2(type, address.Pointer);
    }

    private LLVMValueRef EmitUnary(
        UnaryOperator operation,
        Operand operand,
        FunctionEmissionContext context
    )
    {
        var source = EmitOperandAsValue(operand, context);
        return operation switch
        {
            UnaryOperator.Negation => operand.Type.SpecialType.IsInteger
                ? _builder.BuildNeg(source)
                : _builder.BuildFNeg(source),
            UnaryOperator.LogicalNot => _builder.BuildICmp(
                LLVMIntPredicate.LLVMIntEQ,
                source,
                LLVMValueRef.CreateConstInt(source.TypeOf, 0)
            ),
            UnaryOperator.BitwiseNot => _builder.BuildNot(source),
            _ => throw new InvalidOperationException("Unknown unary operation"),
        };
    }

    private LLVMValueRef EmitBinary(
        BinaryOperator operation,
        Operand left,
        Operand right,
        FunctionEmissionContext context
    )
    {
        var leftValue = EmitOperandAsValue(left, context);
        var rightValue = EmitOperandAsValue(right, context);
        var type = left.Type;
        Debug.Assert(left.Type == right.Type);
        return operation switch
        {
            BinaryOperator.Addition => type.SpecialType.IsInteger
                ? _builder.BuildAdd(leftValue, rightValue)
                : _builder.BuildFAdd(leftValue, rightValue),
            BinaryOperator.Subtraction => type.SpecialType.IsInteger
                ? _builder.BuildSub(leftValue, rightValue)
                : _builder.BuildFSub(leftValue, rightValue),
            BinaryOperator.Multiplication => type.SpecialType.IsInteger
                ? _builder.BuildMul(leftValue, rightValue)
                : _builder.BuildFMul(leftValue, rightValue),
            BinaryOperator.Division => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.BuildSDiv(leftValue, rightValue),
                { IsUnsignedInteger: true } => _builder.BuildUDiv(leftValue, rightValue),
                _ => _builder.BuildFDiv(leftValue, rightValue),
            },
            BinaryOperator.Modulo => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.BuildSRem(leftValue, rightValue),
                { IsUnsignedInteger: true } => _builder.BuildURem(leftValue, rightValue),
                _ => _builder.BuildFRem(leftValue, rightValue),
            },
            BinaryOperator.BitwiseAnd => _builder.BuildAnd(leftValue, rightValue),
            BinaryOperator.BitwiseOr => _builder.BuildOr(leftValue, rightValue),
            BinaryOperator.BitwiseXor => _builder.BuildXor(leftValue, rightValue),
            BinaryOperator.Equality => type.SpecialType.IsFloatingPoint
                ? _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, leftValue, rightValue)
                : _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, leftValue, rightValue),
            BinaryOperator.NotEquals => type.SpecialType.IsFloatingPoint
                ? _builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE, leftValue, rightValue)
                : _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, leftValue, rightValue),
            BinaryOperator.LessThan => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntSLT,
                    leftValue,
                    rightValue
                ),
                { IsUnsignedInteger: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntULT,
                    leftValue,
                    rightValue
                ),
                _ => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLT, leftValue, rightValue),
            },
            BinaryOperator.LessThanOrEquals => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntSLE,
                    leftValue,
                    rightValue
                ),
                { IsUnsignedInteger: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntULE,
                    leftValue,
                    rightValue
                ),
                _ => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLE, leftValue, rightValue),
            },
            BinaryOperator.GreaterThan => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntSGT,
                    leftValue,
                    rightValue
                ),
                { IsUnsignedInteger: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntUGT,
                    leftValue,
                    rightValue
                ),
                _ => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGT, leftValue, rightValue),
            },
            BinaryOperator.GreaterThanOrEquals => type.SpecialType switch
            {
                { IsSignedInteger: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntSGE,
                    leftValue,
                    rightValue
                ),
                { IsUnsignedInteger: true } => _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntUGE,
                    leftValue,
                    rightValue
                ),
                _ => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGE, leftValue, rightValue),
            },
            BinaryOperator.ThreeWayComparison => type.SpecialType switch
            {
                { IsInteger: true } => EmitIntegerThreeWayComparison(),
                _ => EmitFloatingPointThreeWayComparison(),
            },
            BinaryOperator.ShiftLeft => _builder.BuildShl(leftValue, rightValue),
            BinaryOperator.ShiftRight => _builder.BuildAShr(leftValue, rightValue),
            BinaryOperator.UnsignedShiftRight => _builder.BuildLShr(leftValue, rightValue),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null),
        };

        LLVMValueRef EmitIntegerThreeWayComparison()
        {
            var intrinsic = GetThreeWayComparisonIntrinsic(type.SpecialType);
            var functionType = intrinsic.FunctionType;
            return _builder.BuildCall2(functionType, intrinsic, [leftValue, rightValue], []);
        }

        LLVMValueRef EmitFloatingPointThreeWayComparison()
        {
            var lt = _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLT, leftValue, rightValue);
            var gt = _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGT, leftValue, rightValue);
            var eq = _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, leftValue, rightValue);

            var s1 = _builder.BuildSelect(
                eq,
                LLVMValueRef.CreateConstInt(_context.Int8Type, 0),
                GetNullOrderingValue()
            );
            var s2 = _builder.BuildSelect(
                gt,
                LLVMValueRef.CreateConstInt(_context.Int8Type, 1),
                s1
            );
            return _builder.BuildSelect(lt, LLVMValueRef.CreateConstInt(_context.Int8Type, -1), s2);
        }
    }

    private LLVMValueRef GetThreeWayComparisonIntrinsic(SpecialType specialType)
    {
        var integerKind = specialType.ToIntegerKind(_compilation.Settings.PointerWidth);

        var index = integerKind.ToRaw() - 1;
        ref var intrinsic = ref _threeWayComparisonIntrinsics[index];
        if (intrinsic is not null)
            return intrinsic.Value;

        var signChar = integerKind.Sign switch
        {
            IntegerSign.Unsigned => 'u',
            IntegerSign.Signed => 's',
            _ => throw new InvalidOperationException("Invalid integer sign"),
        };
        var id = LookupIntrinsicId($"llvm.{signChar}cmp.i8.i{integerKind.BitCount}");

        intrinsic = _module.GetIntrinsicDeclaration(
            id,
            [_context.Int8Type, _context.GetIntType((uint)integerKind.BitCount)]
        );
        return intrinsic.Value;
    }

    private LLVMValueRef GetNullOrderingValue()
    {
        if (_nullOrderingValue.HasValue)
            return _nullOrderingValue.Value;

        var ordering = _compilation.GetSpecialType(SpecialType.Ordering);
        Debug.Assert(ordering.EnumUnderlyingType?.SpecialType == SpecialType.I8);

        var nullableOrdering = _compilation.CreateNullableTypeSymbol(ordering);
        var layout = (OptimizedNullableLayout)_compilation.GetTypeLayout(nullableOrdering);

        var nullValue = (ScalarNullValue)layout.NullValue;

        var orderingValue = LLVMValueRef.CreateConstInt(_context.Int8Type, nullValue.BitPattern);
        _nullOrderingValue = orderingValue;
        return orderingValue;
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

    private LLVMValueRef EmitIsNull(Operand operand, FunctionEmissionContext context)
    {
        return EmitNullCheck(operand, false, context);
    }

    private LLVMValueRef EmitIsNotNull(Operand operand, FunctionEmissionContext context)
    {
        return EmitNullCheck(operand, true, context);
    }

    private LLVMValueRef EmitNullCheck(
        Operand operand,
        bool isNotNull,
        FunctionEmissionContext context
    )
    {
        var layout = (NullableLayout)_compilation.GetTypeLayout(operand.Type);
        return layout switch
        {
            OptimizedNullableLayout optimized => CompareToOptimizedNullValue(
                operand,
                isNotNull,
                optimized,
                context
            ),
            TaggedNullableLayout => CheckTaggedNullValue(operand, isNotNull, context),
        };
    }

    private LLVMValueRef CheckTaggedNullValue(
        Operand operand,
        bool isNotNull,
        FunctionEmissionContext context
    )
    {
        var value = EmitExtractSimpleValue(operand, NullTagIndex, context);
        return _builder.BuildICmp(
            isNotNull ? LLVMIntPredicate.LLVMIntNE : LLVMIntPredicate.LLVMIntEQ,
            value,
            LLVMValueRef.CreateConstInt(_context.Int8Type, 0)
        );
    }

    private LLVMValueRef CreateTaggedNullValue(TaggedNullableLayout layout)
    {
        return LLVMValueRef.CreateConstNull(GetOrCreateType(layout));
    }

    private LLVMValueRef CompareToOptimizedNullValue(
        Operand operand,
        bool isNotNull,
        OptimizedNullableLayout layout,
        FunctionEmissionContext context
    )
    {
        var i64Type = _context.Int64Type;
        var indices = new List<LLVMValueRef> { LLVMValueRef.CreateConstInt(i64Type, 0) };
        var nullValue = layout.NullValue;
        var underlyingType = layout.UnderlyingLayout;
        LLVMValueRef comparand;
        loop:
        while (true)
        {
            switch (nullValue)
            {
                case ScalarNullValue scalar:
                    comparand = GetComparand(scalar);
                    break loop;
                case CompositeNullValue composite:
                    indices.Add(LLVMValueRef.CreateConstInt(i64Type, (uint)composite.FieldIndex));
                    nullValue = composite.Value;
                    underlyingType = ((StructLayout)underlyingType)
                        .Fields[composite.FieldIndex]
                        .Type;
                    break;
            }
        }

        var result = operand switch
        {
            ConstantOperand(var constant, var type) => ExtractFromSsa(
                MakeConstant(in constant, type)
            ),
            PlaceOperand(var place) => ExtractFromPlace(place),
        };

        return _builder.BuildICmp(
            isNotNull ? LLVMIntPredicate.LLVMIntNE : LLVMIntPredicate.LLVMIntEQ,
            result,
            comparand
        );

        LLVMValueRef GetComparand(ScalarNullValue scalar)
        {
            var llvmType = GetOrCreateType(underlyingType);
            return underlyingType switch
            {
                IntegerLayout => LLVMValueRef.CreateConstInt(llvmType, scalar.BitPattern),
                PointerLayout pointer => scalar.BitPattern == 0
                    ? LLVMValueRef.CreateConstPointerNull(llvmType)
                    : LLVMValueRef.CreateConstPointerCast(
                        LLVMValueRef.CreateConstInt(
                            _context.GetIntType((uint)pointer.BitWidth),
                            scalar.BitPattern
                        ),
                        llvmType
                    ),
                _ => throw new InvalidOperationException(
                    "Cannot have an optimized scalar null value"
                ),
            };
        }

        LLVMValueRef ExtractFromSsa(LLVMValueRef ssa)
        {
            foreach (
                var index in indices.AsValueEnumerable().Skip(1).Select(i => (uint)i.ConstIntZExt)
            )
            {
                ssa = _builder.BuildExtractValue(ssa, index);
            }

            return ssa;
        }

        LLVMValueRef ExtractFromPlace(Place place)
        {
            if (DestinationIsSsa(place, context, out var local))
            {
                return ExtractFromSsa(context.LookupLocal(local));
            }

            var llvmType = GetOrCreateType(place.Type);
            var address = EmitTakeAddress(place, context).Pointer;
            if (indices.Any(i => i.ConstIntZExt > 0))
            {
                address = _builder.BuildInBoundsGEP2(
                    llvmType,
                    address,
                    CollectionsMarshal.AsSpan(indices),
                    ""
                );
            }

            return _builder.BuildLoad2(comparand.TypeOf, address);
        }
    }

    private LLVMValueRef CreateOptimizedNullValue(NullValue value, TypeLayout underlyingType)
    {
        return value switch
        {
            ScalarNullValue scalar => CreateOptimizedNullValue(scalar, underlyingType),
            CompositeNullValue composite => CreateOptimizedNullValue(composite, underlyingType),
        };
    }

    private LLVMValueRef CreateOptimizedNullValue(ScalarNullValue scalar, TypeLayout underlyingType)
    {
        var llvmType = GetOrCreateType(underlyingType);
        return underlyingType switch
        {
            IntegerLayout => LLVMValueRef.CreateConstInt(llvmType, scalar.BitPattern),
            PointerLayout pointer => scalar.BitPattern == 0
                ? LLVMValueRef.CreateConstPointerNull(llvmType)
                : LLVMValueRef.CreateConstPointerCast(
                    LLVMValueRef.CreateConstInt(
                        _context.GetIntType((uint)pointer.BitWidth),
                        scalar.BitPattern
                    ),
                    llvmType
                ),
            _ => throw new InvalidOperationException("Cannot have an optimized scalar null value"),
        };
    }

    private LLVMValueRef CreateOptimizedNullValue(
        CompositeNullValue composite,
        TypeLayout underlyingType
    )
    {
        var llvmType = GetOrCreateType(underlyingType);
        var structType = (StructLayout)underlyingType;
        var fieldType = structType.Fields[composite.FieldIndex].Type;
        var keyValue = CreateOptimizedNullValue(composite.Value, fieldType);
        if (keyValue.IsNull)
            return LLVMValueRef.CreateConstNull(llvmType);

        var fieldCount = llvmType.StructElementTypesCount;
        Span<LLVMValueRef> elements = new LLVMValueRef[fieldCount];
        for (var i = 0; i < fieldCount; i++)
        {
            var elementType = structType.Fields[i].Type;
            var llvmElementType = GetOrCreateType(elementType);
            elements[i] =
                i == composite.FieldIndex
                    ? keyValue
                    : LLVMValueRef.CreateConstNull(llvmElementType);
        }

        return LLVMValueRef.CreateConstNamedStruct(llvmType, elements);
    }

    private LLVMValueRef EmitSelect(
        Operand condition,
        Operand left,
        Operand right,
        FunctionEmissionContext context
    )
    {
        var value = EmitOperandAsValue(condition, context);
        var comparisonValue = ConvertByteBoolToI1IfNeeded(value);
        var leftValue = EmitOperandAsValue(left, context);
        var rightValue = EmitOperandAsValue(right, context);
        return _builder.BuildSelect(comparisonValue, leftValue, rightValue);
    }

    private LLVMValueRef EmitConversion(
        Operand operand,
        Conversion conversion,
        TypeSymbol targetType,
        FunctionEmissionContext context
    )
    {
        var baseValue = EmitOperandAsValue(operand, context);
        return EmitScalarConversion(baseValue, conversion, operand.Type, targetType);
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

        if (conversion.IsCharacter)
        {
            if (source.IntWidth < target.IntWidth)
            {
                return _builder.BuildZExt(operand, target);
            }

            Debug.Assert(source.IntWidth > target.IntWidth);
            return _builder.BuildTrunc(operand, target);
        }

        if (conversion.IsReference)
        {
            // Pointers are pointers, so it shouldn't matter here
            return operand;
        }

        // ReSharper disable once InvertIf
        if (conversion.IsSpan)
        {
            var sourceReference = (ReferenceTypeSymbol)sourceType;

            // ReSharper disable once InvertIf
            if (sourceReference.ReferencedType is ArrayTypeSymbol { Size: { } size })
            {
                var structType = GetOrCreateType(targetType);
                var structValue = structType.Undef;
                structValue = _builder.BuildInsertValue(structValue, operand, 0);

                var lengthValue = LLVMValueRef.CreateConstInt(
                    GetOrCreateType(_compilation.GetSpecialType(SpecialType.USize)),
                    size
                );
                structValue = _builder.BuildInsertValue(structValue, lengthValue, 1);

                return structValue;
            }
        }

        throw new InvalidOperationException("If we get here, the conversion is invalid");
    }

    private void EmitArrayTo(
        Place destination,
        ImmutableArray<Operand> elements,
        FunctionEmissionContext context
    )
    {
        var baseAddress = EmitTakeAddress(destination, context);
        var arrayType = GetOrCreateType(destination.Type);
        var elementType = ((ArrayTypeSymbol)destination.Type).ElementType;
        var layout = _compilation.GetTypeLayout(elementType);
        var elementSize = layout.Size;
        foreach (var (i, operand) in elements.AsValueEnumerable().Index())
        {
            var elementPointer = _builder.BuildGEP2(
                arrayType,
                baseAddress.Pointer,
                [LLVMValueRef.CreateConstInt(_context.Int64Type, (ulong)i)],
                []
            );

            var elementAddress = baseAddress with
            {
                Pointer = elementPointer,
                ByteRange = baseAddress.ByteRange.RequireNonNull() with
                {
                    Offset = (ulong)i * elementSize,
                },
            };

            EmitOperandTo(in elementAddress, operand, context);
        }
    }

    private LLVMValueRef EmitData(Operand operand, FunctionEmissionContext context)
    {
        return EmitExtractSimpleValue(operand, SliceDataIndex, context);
    }

    private LLVMValueRef EmitLength(Operand operand, FunctionEmissionContext context)
    {
        return EmitExtractSimpleValue(operand, SliceLengthIndex, context);
    }

    private LLVMValueRef EmitExtractSimpleValue(
        Operand operand,
        uint index,
        FunctionEmissionContext context
    )
    {
        return operand switch
        {
            ConstantOperand(var constant, var type) => ExtractFromSsa(
                MakeConstant(in constant, type)
            ),
            PlaceOperand(var place) => ExtractFromPlace(place),
        };

        LLVMValueRef ExtractFromSsa(LLVMValueRef ssa)
        {
            return _builder.BuildExtractValue(ssa, index);
        }

        LLVMValueRef ExtractFromPlace(Place place)
        {
            if (DestinationIsSsa(place, context, out var local))
            {
                return ExtractFromSsa(context.LookupLocal(local));
            }

            var llvmType = GetOrCreateType(place.Type);
            var baseAddress = EmitTakeAddress(place, context).Pointer;
            var downstreamAddress =
                index == 0 ? baseAddress : _builder.BuildStructGEP2(llvmType, baseAddress, index);
            return _builder.BuildLoad2(
                llvmType.GetStructElementTypes()[NullTagIndex],
                downstreamAddress
            );
        }
    }

    private void EmitStorageLive(Local local, FunctionEmissionContext context)
    {
        var classification = context.LocalClassification[local];
        if (!classification.IsStoredInMemory || CanTypeBeLoadedAsSsa(local.Type))
            return;

        var lifetimeStartFunction = GetLifetimeStartFunction();
        _builder.BuildCall2(
            lifetimeStartFunction.FunctionType,
            lifetimeStartFunction,
            [context.LookupLocal(local)],
            "start".AsSpan()
        );
    }

    private void EmitStorageDead(Local local, FunctionEmissionContext context)
    {
        var classification = context.LocalClassification[local];
        if (!classification.IsStoredInMemory || CanTypeBeLoadedAsSsa(local.Type))
            return;

        var lifetimeEndFunction = GetLifetimeEndFunction();
        _builder.BuildCall2(
            lifetimeEndFunction.FunctionType,
            lifetimeEndFunction,
            [context.LookupLocal(local)],
            "end".AsSpan()
        );
    }

    private void EmitTerminator(Terminator instruction, FunctionEmissionContext context)
    {
        switch (instruction)
        {
            case BranchTerminator(var condition, var targetWhenTrue, var targetWhenFalse):
            {
                var conditionValue = ConvertByteBoolToI1IfNeeded(
                    EmitOperandAsValue(condition, context)
                );
                _builder.BuildCondBr(
                    conditionValue,
                    context.LookupBlock(targetWhenTrue),
                    context.LookupBlock(targetWhenFalse)
                );
                break;
            }
            case GotoTerminator(var target):
                _builder.BuildBr(context.LookupBlock(target));
                break;
            case CallTerminator call:
                EmitCall(call, context);
                break;
            case ReturnTerminator:
                if (!context.MirFunction.Symbol.ReturnsVoid)
                {
                    var returnLocal = context.MirFunction.Locals[0];
                    Debug.Assert(returnLocal.Id == LocalId.ReturnSlot);
                    var localClassification = context.LocalClassification[returnLocal];
                    switch (localClassification.StorageKind)
                    {
                        case LocalStorageKind.SsaRegister:
                            _builder.BuildRet(context.LookupLocal(returnLocal));
                            break;
                        case LocalStorageKind.StackSlot:
                        {
                            var returnType = GetOrCreateType(returnLocal.Type);
                            var loadedValue = _builder.BuildLoad2(
                                returnType,
                                EmitTakeAddress(new LocalPlace(returnLocal), context).Pointer
                            );
                            _builder.BuildRet(loadedValue);
                            break;
                        }
                        case LocalStorageKind.IndirectParameter:
                            _builder.BuildRetVoid();
                            break;
                        default:
                            throw new InvalidOperationException(
                                "Unexpected return local storage kind"
                            );
                    }
                }
                else
                {
                    _builder.BuildRetVoid();
                }

                break;
            case UnreachableTerminator:
                _builder.BuildUnreachable();
                break;
        }
    }

    private void EmitCall(CallTerminator call, FunctionEmissionContext context)
    {
        var abi = _compilation.GetFunctionAbi(call.Function);
        var callee = GetOrCreateFunction(call.Function);
        var isIndirectReturn = abi.Return.IsIndirect;
        var receiverType = call.Function.ReceiverType;

        var parameterCount = call.Arguments.Length;
        if (isIndirectReturn)
            parameterCount++;
        if (receiverType is not null)
            parameterCount++;

        Span<LLVMValueRef> parameters = stackalloc LLVMValueRef[parameterCount];

        int offsetIndex;
        if (isIndirectReturn)
        {
            Debug.Assert(call.Destination is not null);
            var address = EmitTakeAddress(call.Destination.Value, context);
            parameters[0] = address.Pointer;
            offsetIndex = 1;
        }
        else
        {
            offsetIndex = 0;
        }

        if (receiverType is not null)
        {
            parameters[offsetIndex] = SetParameterValue(
                call.Receiver.RequireNonNull(),
                "this",
                receiverType,
                abi.Receiver.RequireNonNull(),
                context
            );

            offsetIndex++;
        }

        foreach (var (i, argument) in abi.Parameters.AsValueEnumerable().Index())
        {
            parameters[i + offsetIndex] = SetParameterValue(
                call.Arguments[i],
                argument.Parameter.Name,
                argument.Parameter.Type,
                argument.Classification,
                context
            );
        }

        LLVMValueRef result;
        if (call.UnwindBlock is null)
        {
            result = _builder.BuildCall2(callee.FunctionType, callee, parameters, "");
            _builder.BuildBr(context.LookupBlock(call.NextBlock));
        }
        else
        {
            result = _builder.BuildInvoke2(
                callee.FunctionType,
                callee,
                parameters,
                context.LookupBlock(call.NextBlock),
                context.LookupBlock(call.UnwindBlock),
                ""
            );
        }

        if (!isIndirectReturn && call.Destination is { Base: LocalPlace(var local) })
        {
            context.BindLocal(local, result);
        }
    }

    private LLVMValueRef SetParameterValue(
        Operand value,
        string name,
        TypeSymbol receiverType,
        AbiValueClassification classification,
        FunctionEmissionContext context
    )
    {
        if (classification != AbiValueClassification.Indirect)
            return EmitOperandAsValue(value, context);
        var layout = _compilation.GetTypeLayout(receiverType);
        var llvmType = GetOrCreateType(layout);
        var destination = CreateEntryAlloca(llvmType, name, context);
        destination.SetAlignment((uint)layout.Alignment);

        var destinationAddress = new LoweredAddress(destination);
        EmitOperandTo(in destinationAddress, value, context);

        return destination;
    }

    private LLVMValueRef GetNullValue(TypeSymbol type)
    {
        var layout = (NullableLayout)_compilation.GetTypeLayout(type);
        return GetNullValue(layout);
    }

    private LLVMValueRef GetNullValue(NullableLayout layout)
    {
        return layout switch
        {
            TaggedNullableLayout tagged => CreateTaggedNullValue(tagged),
            OptimizedNullableLayout optimizedNullableLayout => CreateOptimizedNullValue(
                optimizedNullableLayout.NullValue,
                optimizedNullableLayout.UnderlyingLayout
            ),
        };
    }

    private LoweredAddress EmitTakeAddress(Place place, FunctionEmissionContext context)
    {
        var (address, root) = place.Base switch
        {
            GlobalPlace(var variable) => (GetOrCreateGlobal(variable), (AliasRoot?)variable),
            LocalPlace(var local) => (context.LookupLocal(local), local),
        };

        var currentType = place.Base.Type;

        ConstantByteRange? byteRange = place.Type.IsDynamicallySized
            ? null
            : new(0, _compilation.GetTypeLayout(place.Type).Size);

        foreach (var projection in place.Projections)
        {
            address = projection switch
            {
                FieldProjection(var field) => GetFieldAddress(field),
                DereferenceProjection => TakeReferencedAddress(),
                IndexProjection(var index, _) => GetReferencedIndex(index),
                PayloadProjection => GetPayload(),
            };

            currentType = projection.Type;
        }

        return new LoweredAddress(address, root, byteRange);

        LLVMValueRef TakeReferencedAddress()
        {
            root = null;
            byteRange = null;
            return address;
        }

        LLVMValueRef GetFieldAddress(VariableSymbol field)
        {
            var fieldIndex = GetFieldIndex(field);

            // ReSharper disable once InvertIf
            if (byteRange is { Offset: var offset } range)
            {
                var layout = (StructLayout)_compilation.GetTypeLayout(currentType);
                offset += layout.Fields[(int)fieldIndex].ByteOffset;
                byteRange = range with { Offset = offset };
            }

            return fieldIndex == 0
                ? address
                : _builder.BuildStructGEP2(GetOrCreateType(currentType), address, fieldIndex);
        }

        LLVMValueRef GetReferencedIndex(Operand index)
        {
            if (!UpdateOffset(index))
            {
                byteRange = null;
            }

            if (currentType.IsDynamicallySized)
            {
                var elementType = GetUnderlyingType();
                return _builder.BuildGEP2(
                    GetOrCreateType(elementType),
                    address,
                    [EmitOperandAsValue(index, context)],
                    []
                );
            }

            return _builder.BuildGEP2(
                GetOrCreateType(currentType),
                address,
                [EmitOperandAsValue(index, context)],
                []
            );
        }

        TypeSymbol GetUnderlyingType()
        {
            if (currentType is ArrayTypeSymbol { ElementType: var elementType })
                return elementType;
            if (currentType.SpecialType == SpecialType.Str)
                return _compilation.GetSpecialType(SpecialType.Char);

            throw new InvalidOperationException($"Unsupported type: {currentType}");
        }

        LLVMValueRef GetPayload()
        {
            var layout = (NullableLayout)_compilation.GetTypeLayout(currentType);
            return layout switch
            {
                TaggedNullableLayout tagged => GetTaggedPayload(tagged),
                OptimizedNullableLayout => address,
            };
        }

        LLVMValueRef GetTaggedPayload(TaggedNullableLayout layout)
        {
            // ReSharper disable once InvertIf
            if (byteRange is { Offset: var offset } range)
            {
                offset += layout.StructRepresentation.Fields[(int)NullPayloadIndex].ByteOffset;
                byteRange = range with { Offset = offset };
            }

            return _builder.BuildStructGEP2(
                GetOrCreateType(currentType),
                address,
                NullPayloadIndex
            );
        }

        bool UpdateOffset(Operand index)
        {
            if (byteRange is not { Offset: var offset } range)
                return false;

            if (index is not ConstantOperand(var constant, _))
                return false;

            if (constant.Value is not IntegerConstant integer)
                return false;

            var elementIndex = integer switch
            {
                sbyte v => v,
                byte v => v,
                short v => v,
                ushort v => v,
                int v => v,
                uint v => v,
                long v => v,
                ulong v and <= long.MaxValue => (long)v,
                _ => throw new InvalidOperationException("Unsupported constant index"),
            };

            if (elementIndex < 0)
                return false;

            if (_compilation.GetTypeLayout(currentType) is not ArrayLayout arrayLayout)
                return false;

            offset += (ulong)elementIndex * arrayLayout.ElementType.Size;
            byteRange = range with { Offset = offset };
            return true;
        }
    }

    private uint GetFieldIndex(VariableSymbol field)
    {
        var baseType = field.ContainingType.RequireNonNull();
        var layout = (StructLayout)_compilation.GetTypeLayout(baseType);

        uint i = 0;
        foreach (var (name, _, _) in layout.Fields)
        {
            if (name == field.Name)
            {
                return i;
            }

            i++;
        }

        throw new InvalidOperationException($"Unknown field: {field.Name}");
    }

    private void WriteIR()
    {
        var targetPath = Path.Combine(_options.OutputDirectory, $"{_compilation.AssemblyName}.ll");
        _module.PrintToFile(targetPath);
    }

    private void WriteOptimizedIR()
    {
        var targetPath = Path.Combine(
            _options.OutputDirectory,
            $"{_compilation.AssemblyName}.opt.ll"
        );
        _module.PrintToFile(targetPath);
    }

    private async Task<EmitResult> OutputBinaryAsync(
        BindingContext context,
        CancellationToken cancellationToken
    )
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
            LLVMCodeGenOptLevel.LLVMCodeGenLevelDefault,
            LLVMRelocMode.LLVMRelocPIC,
            LLVMCodeModel.LLVMCodeModelDefault
        );

        _module.Target = tripleString;
        _module.DataLayoutObject = targetMachine.CreateTargetDataLayout();

        WriteIR();

        OptimizeModule(targetMachine);

        WriteOptimizedIR();

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

        return _compilation.Settings.OutputKind switch
        {
            OutputKind.Executable => await LinkAsync(
                outputFilename,
                LinkOutput.Executable,
                context,
                cancellationToken
            ),
            OutputKind.StaticLibrary => await CreateStaticLibraryAsync(
                outputFilename,
                context,
                cancellationToken
            ),
            OutputKind.SharedLibrary => await LinkAsync(
                outputFilename,
                LinkOutput.DynamicLibrary,
                context,
                cancellationToken
            ),
            _ => throw new InvalidOperationException("Unsupported output kind"),
        };
    }

    private void OptimizeModule(LLVMTargetMachineRef targetMachine)
    {
        using var passBuilderOptions = LLVMPassBuilderOptionsRef.Create();
        _module.RunPasses(GetPassesName(), targetMachine, passBuilderOptions);
    }

    private string GetPassesName()
    {
        return _compilation.Settings.OptimizationLevel switch
        {
            OptimizationLevel.None => "default<O0>",
            OptimizationLevel.Less => "default<O1>",
            OptimizationLevel.Default => "default<O2>",
            OptimizationLevel.Aggressive => "default<O3>",
            _ => throw new InvalidOperationException("Unsupported optimization level"),
        };
    }

    private async Task<EmitResult> LinkAsync(
        string objFileName,
        LinkOutput kind,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var toolchain = ToolchainFactory.Create(
                new ToolchainRequest(
                    _compilation.Settings.Architecture,
                    _compilation.Settings.OperatingSystem,
                    _compilation.Settings.Toolchain
                )
            );

            var targetPath = kind switch
            {
                LinkOutput.Executable => Path.Join(
                    _options.OutputDirectory,
                    $"{_compilation.AssemblyName}.exe"
                ),
                LinkOutput.DynamicLibrary => Path.Join(
                    _options.OutputDirectory,
                    $"{_compilation.AssemblyName}.dll"
                ),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
            };

            var request = new LinkRequest
            {
                OutputPath = targetPath,
                OutputKind = kind,
                ObjectFiles = [objFileName],
                EntryPoint = kind == LinkOutput.Executable ? _entryPoint : null,
                Subsystem = kind == LinkOutput.Executable ? Subsystem.Console : null,
                ImportLibraryPath =
                    kind == LinkOutput.DynamicLibrary
                        ? Path.Join(_options.OutputDirectory, $"{_compilation.AssemblyName}.lib")
                        : null,
            };

            var result = await toolchain.LinkAsync(request, cancellationToken);
            if (result.Success)
                return new EmitResult(true, context.CollectDiagnostics());

            context.ReportDiagnostic(
                Diagnostic.ExternalDiagnostic(
                    DiagnosticSeverity.Error,
                    Location.None,
                    result.StdOut
                )
            );
            return new EmitResult(false, context.CollectDiagnostics());
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.ExternalDiagnostic(Location.None, ex.Message));
            return new EmitResult(false, context.CollectDiagnostics());
        }
    }

    private async Task<EmitResult> CreateStaticLibraryAsync(
        string objFileName,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var toolchain = ToolchainFactory.Create(
                new ToolchainRequest(
                    _compilation.Settings.Architecture,
                    _compilation.Settings.OperatingSystem,
                    _compilation.Settings.Toolchain
                )
            );

            var targetPath = Path.Join(
                _options.OutputDirectory,
                $"{_compilation.AssemblyName}.lib"
            );

            var request = new StaticLibraryRequest
            {
                OutputPath = targetPath,
                ObjectFiles = [objFileName],
            };

            var result = await toolchain.CreateStaticLibraryAsync(request, cancellationToken);
            if (result.Success)
                return new EmitResult(true, context.CollectDiagnostics());

            context.ReportDiagnostic(Diagnostic.ExternalDiagnostic(Location.None, result.StdErr));
            return new EmitResult(false, context.CollectDiagnostics());
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.ExternalDiagnostic(Location.None, ex.Message));
            return new EmitResult(false, context.CollectDiagnostics());
        }
    }

    private static bool CanUseMemCopy(in LoweredAddress destination, in LoweredAddress source)
    {
        if (!AliasRootsDefinitelyEqual(destination.Root, source.Root))
            return true;

        if (
            destination.ByteRange is var (destOffset, destSize)
            && source.ByteRange is var (sourceOffset, sourceSize)
        )
        {
            return RangesDoNotOverlap(destOffset, destSize, sourceOffset, sourceSize);
        }

        return false;
    }

    private readonly record struct ConstantByteRange(ulong Offset, ulong Size);

    private readonly record struct LoweredAddress(
        LLVMValueRef Pointer,
        AliasRoot? Root = null,
        ConstantByteRange? ByteRange = null
    );

    // ReSharper disable UnusedUnionCase.Local
    private readonly union AliasRoot(Local, VariableSymbol);

    // ReSharper restore UnusedUnionCase.Local

    private static bool AliasRootsDefinitelyEqual(AliasRoot? left, AliasRoot? right)
    {
        if (left is null || right is null)
            return false;

        return left switch
        {
            Local local => right is Local rightLocal && local.Equals(rightLocal),
            VariableSymbol variable => right is VariableSymbol rightVariable
                && variable.Equals(rightVariable),
        };
    }

    private static bool RangesDoNotOverlap(
        ulong leftOffset,
        ulong leftSize,
        ulong rightOffset,
        ulong rightSize
    )
    {
        var leftEnd = leftOffset + leftSize;
        var rightEnd = rightOffset + rightSize;
        return leftEnd <= rightOffset || rightEnd <= leftOffset;
    }

    public void Dispose()
    {
        _debugInfo?.Dispose();
        _builder.Dispose();
        _module.Dispose();
        _context.Dispose();
    }
}
