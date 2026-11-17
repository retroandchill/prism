// @file PlaceProjection.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Semantic.Symbols;

namespace Prism.Mir;

public closed record PlaceProjection
{
    public TypeSymbol Type => GetTypeSymbol();

    protected abstract TypeSymbol GetTypeSymbol();
}

public sealed record FieldProjection : PlaceProjection
{
    public FieldProjection(VariableSymbol field)
    {
        if (!field.IsInstance)
            throw new ArgumentException("Field must be an instance field.", nameof(field));

        Field = field;
    }

    public VariableSymbol Field { get; }

    public new TypeSymbol Type => Field.Type;

    public void Deconstruct(out VariableSymbol field)
    {
        field = Field;
    }

    protected override TypeSymbol GetTypeSymbol() => Field.Type;
}

public sealed record IndexProjection(Operand Index, TypeSymbol Type) : PlaceProjection
{
    protected override TypeSymbol GetTypeSymbol() => Type;
}

public sealed record DereferenceProjection(TypeSymbol Type) : PlaceProjection
{
    protected override TypeSymbol GetTypeSymbol() => Type;
}

public sealed record PayloadProjection(TypeSymbol Type) : PlaceProjection
{
    protected override TypeSymbol GetTypeSymbol() => Type;
}
