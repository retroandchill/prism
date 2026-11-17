// @file LlvmCodeEmitterFactory.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Codegen.Mir;
using Prism.Compiler;
using Prism.Compiler.Codegen;
using Prism.Mir;

namespace Prism.Codegen.Llvm;

internal sealed class LlvmCodeEmitterFactory : ICodeEmitterFactory
{
    public ICodeEmitter CreateEmitter(Compilation compilation, CodeGenOptions options)
    {
        var mirEmitter = new Emitter(compilation);
        return new LlvmCodeEmitter(compilation, mirEmitter, options);
    }
}
