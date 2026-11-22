// @file EmitterFactory.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Compiler;
using Prism.Mir;

namespace Prism.Codegen.Mir;

public sealed class MirEmitterFactory : IMirEmitterFactory<Compilation>
{
    public IMirEmitter CreateEmitter(Compilation context) => new MirEmitter(context);
}
