// @file PlaceBase.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Semantic.Symbols;

namespace Prism.Mir;

public closed record PlaceBase
{
    public abstract TypeSymbol Type { get; }
}

public sealed record LocalPlace(Local Local) : PlaceBase
{
    public override TypeSymbol Type => Local.Type;

    public override string ToString() => Local.ToString();
}

public sealed record GlobalPlace : PlaceBase
{
    public GlobalPlace(VariableSymbol variable)
    {
        if (!variable.IsStaticStorage)
            throw new ArgumentException("Variable must have static storage.", nameof(variable));

        Variable = variable;
    }

    public VariableSymbol Variable { get; }

    public override TypeSymbol Type => Variable.Type;

    public void Deconstruct(out VariableSymbol variable)
    {
        variable = Variable;
    }

    public override string ToString() => Variable.ToDisplayString();
}
