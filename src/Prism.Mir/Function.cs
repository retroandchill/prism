// @file MirFunction.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Semantic.Symbols;
using Riok.Mapperly.Abstractions;

namespace Prism.Mir;

public sealed partial class Function
{
    public FunctionSymbol Symbol { get; }

    public ImmutableArray<BasicBlock> Blocks { get; private set; }

    public ImmutableArray<Local> Locals { get; private set; }

    private Function(FunctionSymbol symbol)
    {
        Symbol = symbol;
    }

    [Mapper(UseReferenceHandling = true, RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    [UseStaticMapper(typeof(BasicBlock.Mapper))]
    internal static partial class Mapper
    {
        public static partial Function Map(FunctionBuilder builder);
    }
}
