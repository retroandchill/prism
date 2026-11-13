// @file ICodeModuleBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Compiler.Binding;
using Prism.Compiler.BoundTree;
using Prism.Compiler.Symbols;
using Prism.Semantic;

namespace Prism.Compiler.Codegen;

internal interface ICodeEmitter : IDisposable
{
    void AddGlobalConstant(
        VariableSymbol variable,
        BindingContext bindingContext,
        CancellationToken cancellationToken
    );

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

    Task<EmitResult> EmitAsync(BindingContext context, CancellationToken cancellationToken);
}
