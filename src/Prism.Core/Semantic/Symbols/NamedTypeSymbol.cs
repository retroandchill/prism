// @file NamedTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Strings;
using Prism.Core.Syntax;

namespace Prism.Core.Semantic.Symbols;

public sealed class NamedTypeSymbol : TypeSymbol
{
    public BuiltInType? BuiltInType { get; }

    internal NamedTypeSymbol(BuiltInType type, Compilation compilation)
        : base(KnownName.From(type), compilation)
    {
        BuiltInType = type;
    }
}
