// @file PlaceBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;

namespace Prism.Mir;

public struct PlaceBuilder
{
    public PlaceBase Base { get; set; }

    private List<PlaceProjection>? _projections;

    public void AddProjection(PlaceProjection projection)
    {
        _projections ??= [];
        _projections.Add(projection);
    }

    public readonly Place Build()
    {
        Debug.Assert(Base is not null, "Base must be set first");
        return new Place(Base, _projections?.ToImmutableArray() ?? []);
    }
}
