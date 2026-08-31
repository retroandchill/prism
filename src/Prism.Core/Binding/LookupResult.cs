// @file LookupResult.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Symbols;

namespace Prism.Core.Binding;

internal enum LookupResultKind : byte
{
    NotFound,
    Viable,
    Ambiguous,
    WrongKind,
    Inaccessible,
    Error,
}

internal readonly struct LookupResult
{
    private LookupResult(LookupResultKind kind, ImmutableArray<Symbol> symbols)
    {
        Kind = kind;
        Symbols = symbols;
    }

    public LookupResultKind Kind { get; }

    public bool Found => Kind != LookupResultKind.NotFound;

    public bool IsViable => Kind == LookupResultKind.Viable;

    public bool IsAmbiguous => Kind == LookupResultKind.Ambiguous;

    public bool IsError => Kind == LookupResultKind.Error;

    public ImmutableArray<Symbol> Symbols { get; }

    public Symbol Symbol =>
        Symbols.Length == 1
            ? Symbols[0]
            : throw new InvalidOperationException(
                "LookupResult.Symbol: LookupResult is not a single symbol."
            );

    public static LookupResult NotFound() => new(LookupResultKind.NotFound, []);

    public static LookupResult Viable(ImmutableArray<Symbol> symbols) =>
        new(LookupResultKind.Viable, symbols);

    public static LookupResult Ambiguous(ImmutableArray<Symbol> symbols) =>
        new(LookupResultKind.Ambiguous, symbols);

    public static LookupResult WrongKind(ImmutableArray<Symbol> symbols) =>
        new(LookupResultKind.WrongKind, symbols);

    public static LookupResult Inaccessible(ImmutableArray<Symbol> symbols) =>
        new(LookupResultKind.Inaccessible, symbols);

    public static LookupResult Error() => new(LookupResultKind.Error, []);
}
