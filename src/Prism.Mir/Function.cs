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

    private ExecutableBody _executableBody = null!;

    public ImmutableArray<BasicBlock> Blocks => _executableBody.Blocks;

    public BasicBlock EntryBlock => _executableBody.EntryBlock;

    public ImmutableArray<Local> Locals => _executableBody.Locals;

    private Function(FunctionSymbol symbol)
    {
        Symbol = symbol;
    }

    public Function ReplaceBlock(int index, BasicBlock replacement)
    {
        var newBody = _executableBody.ReplaceBlock(index, replacement);
        return newBody == _executableBody
            ? this
            : new Function(Symbol) { _executableBody = newBody };
    }

    public Function PruneUnusedBlocksAndLocals()
    {
        var newBody = _executableBody.PruneUnusedBlocksAndLocals();
        return newBody == _executableBody
            ? this
            : new Function(Symbol) { _executableBody = newBody };
    }

    internal void Validate()
    {
        _executableBody.Validate();
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    [UseStaticMapper(typeof(ExecutableBody.Mapper))]
    internal static partial class Mapper
    {
        [MapProperty(nameof(FunctionBuilder.ExecutableBody), nameof(_executableBody))]
        public static partial Function Map(FunctionBuilder builder);
    }
}
