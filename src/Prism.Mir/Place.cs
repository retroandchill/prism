// @file Place.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Semantic.Symbols;

namespace Prism.Mir;

public readonly record struct Place(PlaceBase Base, ImmutableArray<PlaceProjection> Projections)
{
    public Place(PlaceBase @base)
        : this(@base, []) { }

    public bool IsDefault => Base is null && Projections.IsDefault;

    public TypeSymbol Type => !Projections.IsEmpty ? Projections[^1].Type : Base.Type;

    public static implicit operator Place(PlaceBase @base) => new(@base);

    public Place AddProjection(PlaceProjection projection)
    {
        return this with { Projections = Projections.Add(projection) };
    }

    public Place AddProjections(ImmutableArray<PlaceProjection> projections)
    {
        return this with { Projections = Projections.AddRange(projections) };
    }

    public bool Equals(Place other)
    {
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
        return Projections.Aggregate(Base.ToString(), (current, projection) =>
            projection switch
            {
                FieldProjection(var field) => $"{current}.{field.Name}",
                IndexProjection(var index, _) => $"{current}[{index}]",
                DereferenceProjection => $"*({current})",
                PayloadProjection => $"{current}.<payload>",
            }
        );
    }
}
