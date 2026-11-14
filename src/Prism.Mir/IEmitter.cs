// @file IEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Semantic.Symbols;

namespace Prism.Mir;

public interface IEmitter
{
    Function Emit(FunctionSymbol function, CancellationToken cancellationToken = default);
}
