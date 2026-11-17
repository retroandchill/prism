// @file Module.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Mir;

public sealed class Module
{
    public ImmutableArray<Function> Functions { get; }

    public Module(ImmutableArray<Function> functions)
    {
        Functions = functions;
    }

    internal Module WithFunctions(ImmutableArray<Function> functions)
    {
        return functions != Functions ? new Module(functions) : this;
    }
}
