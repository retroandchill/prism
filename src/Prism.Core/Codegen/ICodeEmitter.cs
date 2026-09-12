// @file ICodeModuleBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Binding;
using Prism.Core.BoundTree;
using Prism.Core.Symbols;

namespace Prism.Core.Codegen;

internal interface ICodeEmitter : IDisposable
{
    void AddGlobalVariable(
        BoundVariableInitializer variable,
        BindingContext bindingContext,
        CancellationToken cancellationToken
    );

    void AddFunction(
        BoundFunctionBody function,
        BindingContext bindingContext,
        CancellationToken cancellationToken
    );

    void RegisterGlobalConstructor(FunctionSymbol function);

    void RegisterEntryPoint(FunctionSymbol function);

    EmitResult Emit(BindingContext context);
}
