// @file ICompiledSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Semantic;
using Prism.Syntax;

namespace Prism.Compiler.Symbols;

internal interface ICompiledSymbolData : ISymbolData
{
    Compilation DeclaringCompilation { get; }

    ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }
}
