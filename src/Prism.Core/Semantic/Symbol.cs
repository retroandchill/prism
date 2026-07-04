// @file Symbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Ast;
using Prism.Core.Strings;
#pragma warning disable CA1067
#pragma warning disable CA1050

namespace Prism.Core.Semantic;

public closed record Symbol
{
    public required Name Name { get; init; }
    public SymbolDeclaration? Declaration { get; init; }
}

public sealed record VariableSymbol : Symbol
{
    public bool IsMutable { get; init; }
    public required TypeSymbol Type { get; init; }
}

public sealed record FunctionSymbol : Symbol
{
    public required TypeSymbol ReturnType { get; init; }
    public required ImmutableArray<ParameterSymbol> Parameters { get; init; }
}

public sealed record ParameterSymbol : Symbol
{
    public bool IsMutable { get; init; }
    public required TypeSymbol Type { get; init; }
}

public closed record TypeSymbol : Symbol;

public sealed record NamedTypeSymbol : TypeSymbol
{
    public BuiltInType? BuiltInType { get; init; }
}

public sealed record ErrorTypeSymbol : TypeSymbol
{
    public static ErrorTypeSymbol Instance { get; } = new()
    {
        Name = KnownName.Error
    };
}

public sealed record InferringTypeSymbol : TypeSymbol
{
    public static InferringTypeSymbol Instance { get; } = new()
    {
        Name = KnownName.Inferring
    };
}
