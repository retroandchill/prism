// @file IOptimizationPass.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Mir.Optimization;

public closed class OptimizationPass
{
    public abstract string Name { get; }
}

public abstract class FunctionOptimizationPass : OptimizationPass
{
    public abstract Function Run(Function function, CancellationToken cancellationToken);
}

public abstract class ModuleOptimizationPass : OptimizationPass
{
    public abstract Module Run(Module module, CancellationToken cancellationToken);
}
