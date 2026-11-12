// @file WrapperTypeData.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Semantic;

internal closed class WrapperTypeData : ITypeSymbolData
{
    public abstract string Name { get; }

    public Symbol? ContainingSymbol => null;

    public abstract bool IsDynamicallySized { get; }

    public abstract bool IsDefaultConstructible { get; }
}
