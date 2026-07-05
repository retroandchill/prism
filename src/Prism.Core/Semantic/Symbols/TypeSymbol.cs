// @file TypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;
using Prism.Core.Strings;

namespace Prism.Core.Semantic.Symbols;

public closed class TypeSymbol : Symbol;

public sealed class NamedTypeSymbol : TypeSymbol
{
    public BuiltInType? BuiltInType { get; init; }
}

public sealed class ErrorTypeSymbol : TypeSymbol
{
    public static ErrorTypeSymbol Default { get; } = new()
    {
        Name = KnownName.Error
    };
}

public sealed class InferringTypeSymbol : TypeSymbol
{
    public static InferringTypeSymbol Instance { get; } = new()
    {
        Name = KnownName.Inferring
    };
}
