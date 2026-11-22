// @file MirGenerator.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using Prism.Diagnostics;
using Prism.Mir;
using Prism.Semantic.Symbols;

namespace Prism.Compiler.Codegen;

internal sealed class MirCodeManager(IMirEmitter mirEmitter)
{
    private readonly ConcurrentDictionary<
        VariableSymbol,
        Result<VariableInitializer>
    > _variableInitializers = [];
    private readonly ConcurrentDictionary<FunctionSymbol, Result<Function>> _functions = [];

    public Result<VariableInitializer> GetInitializer(
        VariableSymbol variable,
        CancellationToken cancellationToken = default
    )
    {
        return _variableInitializers.GetOrAdd(
            variable,
            static (variable, t) =>
            {
                var (emitter, cancellationToken) = t;
                return emitter.Emit(variable, cancellationToken);
            },
            (_emitter: mirEmitter, cancellationToken)
        );
    }

    public Result<Function> GetFunction(
        FunctionSymbol function,
        CancellationToken cancellationToken = default
    )
    {
        return _functions.GetOrAdd(
            function,
            static (variable, t) =>
            {
                var (emitter, cancellationToken) = t;
                return emitter.Emit(variable, cancellationToken);
            },
            (_emitter: mirEmitter, cancellationToken)
        );
    }
}
