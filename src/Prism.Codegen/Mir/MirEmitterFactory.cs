// @file EmitterFactory.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Compiler;
using Prism.Compiler.Codegen;

namespace Prism.Codegen.Mir;

internal sealed class MirEmitterFactory : IMirEmitterFactory
{
    public IMirEmitter CreateEmitter(Compilation context) => new MirEmitter(context);
}
