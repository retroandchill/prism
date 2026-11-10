// @file ErrorLabelSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Diagnostics;
using Prism.Syntax;

namespace Prism.Core.Symbols.Error;

internal sealed class ErrorLabelSymbol(string name, FunctionSymbol containingFunction) : LabelSymbol
{
    public override string Name { get; } = name;
    public override FunctionSymbol ContainingFunction { get; } = containingFunction;
    public override ImmutableArray<Location> Locations => [];
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];
}
