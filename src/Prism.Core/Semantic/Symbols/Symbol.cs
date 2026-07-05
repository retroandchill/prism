// @file Symbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Ast;
using Prism.Core.Strings;

#pragma warning disable CA1067
#pragma warning disable CA1050

namespace Prism.Core.Semantic.Symbols;

public closed class Symbol
{
    public required Name Name { get; init; }
    public DeclarationSyntax? Declaration { get; init; }
}

public sealed class VariableSymbol : Symbol
{
    public bool IsMutable { get; init; }

    public required TypeSymbol Type { get; init; }
}

public sealed class FunctionSymbol : Symbol
{
    public required TypeSymbol ReturnType { get; init; }

    public required ImmutableArray<ParameterSymbol> Parameters { get; init; }
}

public sealed class ParameterSymbol : Symbol
{
    public bool IsMutable { get; init; }
    public required TypeSymbol Type { get; init; }
}
