// @file OptimizationPipeline.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using ZLinq;

namespace Prism.Mir.Optimization;

public sealed class OptimizationPipeline(ImmutableArray<OptimizationPass> passes)
{
    public Module Run(Module module, CancellationToken cancellationToken = default)
    {
        var original = module;

        foreach (var pass in passes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (pass)
            {
                case FunctionOptimizationPass functionPass:
                {
                    Function[]? functions = null;
                    foreach (var (i, function) in module.Functions.AsValueEnumerable().Index())
                    {
                        var optimizedFunction = functionPass.Run(function, cancellationToken);
                        if (function == optimizedFunction)
                            continue;

                        // If the function didn't change there is no point to doing a BFS over the graph to prune
                        // used blocks and locals because we already did that.
                        optimizedFunction = optimizedFunction.PruneUnusedBlocksAndLocals();

                        functions ??= [.. module.Functions];
                        functions[i] = optimizedFunction;
                    }

                    if (functions is not null)
                    {
                        module = module.WithFunctions(
                            ImmutableCollectionsMarshal.AsImmutableArray(functions)
                        );
                    }

                    break;
                }
                case ModuleOptimizationPass modulePass:
                    module = modulePass.Run(module, cancellationToken);
                    break;
            }
        }

        if (module == original)
            return module;

        // We need to ensure all functions are valid after optimization
        foreach (var function in module.Functions)
        {
            function.Validate();
        }

        return module;
    }
}
