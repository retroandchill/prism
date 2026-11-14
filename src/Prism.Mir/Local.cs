// @file Local.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Semantic;
using Prism.Semantic.Symbols;

namespace Prism.Mir;

public readonly record struct LocalId(uint Value)
{
    public static LocalId ReturnSlot => new(0);
}

public sealed class Local(LocalId id, TypeSymbol type)
{
    public LocalId Id { get; } = id;

    public TypeSymbol Type { get; } = type;

    public override string ToString() => $"_{Id.Value}";
}
