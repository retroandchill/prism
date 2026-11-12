// @file IFunctionSymbolData.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Semantic;

internal interface IFunctionSymbolData : ISymbolData
{
    FunctionKind Kind { get; }

    AbiKind AbiKind { get; }

    bool IsExtern { get; }

    TypeSymbol ReturnType { get; }

    TypeSymbol? ReceiverType { get; }

    ImmutableArray<ParameterSymbol> Parameters { get; }
}
