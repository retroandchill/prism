// @file ICodeModuleBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Compiler.Binding;
using Prism.Mir;
using Prism.Semantic.Symbols;

namespace Prism.Compiler.Codegen;

internal interface ICodeEmitter : IDisposable
{
    Task<EmitResult> EmitAsync(
        Module mirModule,
        BindingContext context,
        CancellationToken cancellationToken = default
    );
}
