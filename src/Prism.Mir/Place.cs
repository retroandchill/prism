// @file Place.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Semantic;
using Prism.Semantic.Symbols;

namespace Prism.Mir;

public sealed record Place(PlaceBase Base, ImmutableArray<PlaceProjection> Projections)
{
    public bool Equals(Place? other)
    {
        if (other is null)
            return false;

        return Base.Equals(other.Base) && Projections.SequenceEqual(other.Projections);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(Base);
        foreach (var projection in Projections)
            hashCode.Add(projection);

        return hashCode.ToHashCode();
    }

    public override string ToString()
    {
        return Projections.Aggregate(Base.ToString(), (current, projection) => projection switch
            {
                FieldProjection(var field) => $"{current}.{field.Name}",
                IndexProjection(var index) => $"{current}[{index}]",
                DereferenceProjection => $"*({current})",
                DiscriminantProjection => $"{current}.<discriminant>",
                PayloadProjection => $"{current}.<payload>",
                SliceDataProjection => $"{current}.<data>",
                SliceLengthProjection => $"{current}.<length>",
            });
    }
}

public closed record PlaceBase;

public sealed record LocalPlace(Local Local) : PlaceBase
{
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

    public void Deconstruct(out VariableSymbol variable)
    {
        variable = Variable;
    }

    public override string ToString() => Variable.ToDisplayString();
}

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

public sealed record DiscriminantProjection : PlaceProjection
{
    private DiscriminantProjection() { }

    public static readonly DiscriminantProjection Instance = new();
}

public sealed record PayloadProjection : PlaceProjection
{
    private PayloadProjection() { }

    public static readonly PayloadProjection Instance = new();
}

public sealed record SliceDataProjection : PlaceProjection
{
    private SliceDataProjection() { }

    public static readonly SliceDataProjection Instance = new();
}

public sealed record SliceLengthProjection : PlaceProjection
{
    private SliceLengthProjection() { }

    public static readonly SliceLengthProjection Instance = new();
}
