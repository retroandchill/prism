// @file BoundDeclaration.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Semantic.Symbols;

namespace Prism.Core.Semantic.Binding;

public closed class BoundDeclaration : BoundNode;

public sealed class BoundVariable : BoundDeclaration
{
    public required VariableSymbol Symbol { get; init; }

    public required BoundExpression? Initializer { get; init; }
}

public sealed class BoundFunction : BoundDeclaration
{
    public required FunctionSymbol Symbol { get; init; }

    public required BoundBlock? Body { get; init; }
}
