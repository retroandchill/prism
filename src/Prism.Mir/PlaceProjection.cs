// @file PlaceProjection.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Semantic.Symbols;

namespace Prism.Mir;

public closed record PlaceProjection;

public sealed record FieldProjection : PlaceProjection
{
    public FieldProjection(VariableSymbol field)
    {
        if (!field.IsInstance)
            throw new ArgumentException("Field must be an instance field.", nameof(field));

        Field = field;
    }

    public VariableSymbol Field { get; }

    public void Deconstruct(out VariableSymbol field)
    {
        field = Field;
    }
}

public sealed record IndexProjection(Operand Index) : PlaceProjection;

public sealed record DereferenceProjection : PlaceProjection
{
    private DereferenceProjection() { }

    public static readonly DereferenceProjection Instance = new();
}

public sealed record PayloadProjection : PlaceProjection
{
    private PayloadProjection() { }

    public static readonly PayloadProjection Instance = new();
}
