// @file Emitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Mir;
using Prism.Semantic.Symbols;

namespace Prism.Codegen.Mir;

internal sealed class Emitter : IEmitter
{
    public Function Emit(FunctionSymbol function, CancellationToken cancellationToken = default)
    {
        var builder = new FunctionBuilder(function);
        // TODO: Actually emit the code
        return builder.Build();
    }
}
