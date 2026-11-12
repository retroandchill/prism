// @file Symbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Semantic;

public closed class Symbol
{
    private protected Symbol(ISymbolData data)
    {
        Data = data;
    }

    internal ISymbolData Data { get; }

    public string Name => Data.Name;

    public string MetadataName => Data.MetadataName;

    public DeclaredVisibility DeclaredVisibility =>
        Data is IDeclaredSymbolData declaredData
            ? declaredData.DeclaredVisibility
            : DeclaredVisibility.NotApplicable;

    public ImmutableArray<Location> Locations =>
        Data is ILocatableSymbolData locatableData ? locatableData.Locations : [];

    public Location FirstLocation()
    {
        return TryGetFirstLocation()
            ?? throw new InvalidOperationException("Symbol has no locations");
    }

    public Location? TryGetFirstLocation()
    {
        return Locations.Length > 0 ? Locations[0] : null;
    }

    public Location FirstLocationOrNone()
    {
        return TryGetFirstLocation() ?? Location.None;
    }

    public bool IsImplicitlyDeclared => Data is IDeclaredSymbolData { IsImplicitlyDeclared: true };

    public Symbol? ContainingSymbol => Data.ContainingSymbol;

    public ImmutableArray<AttributeData> GetAttributes()
    {
        return Data is IAttributeHolderSymbolData attributeHolderData
            ? attributeHolderData.GetAttributes()
            : [];
    }
}
