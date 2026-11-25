// @file IEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.BoundTree;
using Prism.Mir;
using Prism.Semantic.Symbols;

namespace Prism.Compiler.Codegen;

internal interface IMirEmitter
{
    void RegisterGlobalVariable(
        BoundVariableInitializer boundVariable,
        CancellationToken cancellationToken = default
    );

    void EmitFunctionBody(
        BoundFunctionBody function,
        CancellationToken cancellationToken = default
    );

    void RegisterEntryPoint(FunctionSymbol functionSymbol);

    Module BuildModule(CancellationToken cancellationToken = default);
}
