// @file SynthesizedFunctionSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using Prism.Core.Utils;

namespace Prism.Core.Symbols.Synthesized;

internal sealed class SynthesizedFunctionSymbol : FunctionSymbol
{
    internal SynthesizedFunctionSymbol(
        string name,
        Symbol? containingSymbol,
        TypeSymbol returnType,
        ImmutableArray<ParameterSymbol> parameters = default
    )
        : base(name, containingSymbol)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(returnType);

        ReturnType = returnType;
        Parameters = parameters.NullToEmpty();
    }

    public override TypeSymbol ReturnType { get; }

    public override ImmutableArray<ParameterSymbol> Parameters { get; }

    public override ImmutableArray<Location> Locations => [];

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override bool IsImplicitlyDeclared => true;
}
