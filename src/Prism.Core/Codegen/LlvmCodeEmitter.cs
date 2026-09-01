// @file LlvmCodeEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using LLVMSharp;
using LLVMSharp.Interop;
using Prism.Core.Compiling;
using Prism.Core.Configuration;
using Prism.Core.Symbols;
using Prism.Core.Utils;
using Type = LLVMSharp.Type;

namespace Prism.Core.Codegen;

internal sealed class LlvmCodeEmitter : IDisposable
{
    private readonly Compilation _compilation;
    private readonly CodeGenOptions _options;

    private readonly LLVMContext _context;
    private readonly Module _module;
    private readonly IRBuilder _builder;

    private readonly ConcurrentDictionary<Symbol, Value> _symbolToValue = new(
        ReferenceEqualityComparer.Instance
    );
    private readonly ConcurrentDictionary<TypeSymbol, Type> _symbolToType = new(
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
        throw new NotImplementedException();
    }

    private GlobalVariable GetOrCreateGlobal(VariableSymbol symbol)
    {
        throw new NotImplementedException();
    }

    private Type GetOrCreateType(TypeSymbol symbol)
    {
        return _symbolToType.GetOrAdd(symbol, static (t, self) => self.CreateType(t), this);
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

    public void Dispose()
    {
        _builder.Handle.Dispose();
        _module.Handle.Dispose();
        _context.Handle.Dispose();
    }
}
