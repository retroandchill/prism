// @file ICompletableSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Semantic;

internal interface ICompletableSymbolData : ISymbolData
{
    void ForceComplete(
        SourceLocation? location,
        Predicate<Symbol>? filter,
        CancellationToken cancellationToken
    );

    bool IsComplete(CompletionPart part);
}
