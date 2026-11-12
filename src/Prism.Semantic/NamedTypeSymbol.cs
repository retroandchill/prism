// @file NamedTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Prism.Semantic;

public sealed class NamedTypeSymbol : MemberContainerSymbol, IAttributeHolderSymbolData
{
    internal NamedTypeSymbol(INamedTypeSymbolData data)
        : base(data) { }

    internal new INamedTypeSymbolData Data => Unsafe.As<INamedTypeSymbolData>(base.Data);

    public NamedTypeKind Kind => Data.Kind;
}
