// @file SynthesizedFunctionBase.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Prism.Compiler.Symbols.Source;
using Prism.Semantic;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;

namespace Prism.Compiler.Symbols.Synthesized;

internal abstract class SynthesizedFunction : SynthesizedSymbol, IFunctionSymbolData
{
    protected SynthesizedFunction(Compilation declaringCompilation)
        : base(declaringCompilation)
    {
        PublicSymbol = new FunctionSymbol(this);
    }

    public new FunctionSymbol PublicSymbol
    {
        get => Unsafe.As<FunctionSymbol>(base.PublicSymbol);
        private init => base.PublicSymbol = new FunctionSymbol(this);
    }

    public abstract FunctionKind Kind { get; }
    public abstract AbiKind AbiKind { get; }
    public abstract bool IsExtern { get; }
    public abstract TypeSymbol ReturnType { get; }
    public abstract TypeSymbol? ReceiverType { get; }
    public abstract ImmutableArray<ParameterSymbol> Parameters { get; }
}
