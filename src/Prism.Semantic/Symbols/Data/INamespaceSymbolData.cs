// @file INamespaceSymbolData.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Semantic.Symbols.Data;

public enum NamespaceKind
{
    Assembly,
    Compilation,
}

internal interface INamespaceSymbolData : IMemberContainerSymbolData
{
    NamespaceKind NamespaceKind { get; }
}
