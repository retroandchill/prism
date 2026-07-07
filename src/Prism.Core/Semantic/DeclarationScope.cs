// @file DeclarationScope.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using Prism.Core.Ast;
using Prism.Core.Semantic.Symbols;
using Prism.Core.Strings;

namespace Prism.Core.Semantic;

public sealed class DeclarationScope
{
    public DeclarationScope? Parent { get; }

    public SyntaxNode? Owner { get; }

    private readonly ConcurrentDictionary<Name, ImmutableArray<Symbol>> _declarations = new();

    internal DeclarationScope(SyntaxNode? owner = null, DeclarationScope? parent = null)
    {
        Parent = parent;
        Owner = owner;
    }

    internal void AddDeclaration(Symbol symbol)
    {
        _declarations.AddOrUpdate(symbol.Name, [symbol], (_, existing) => existing.Add(symbol));
    }

    public ImmutableArray<Symbol> GetDeclaredHere(Name name)
    {
        return _declarations.GetValueOrDefault(name, []);
    }

    [Obsolete("Use GetDeclaredHere instead")]
    public ImmutableArray<Symbol> FindDeclarations(Name name)
    {
        if (_declarations.TryGetValue(name, out var declarations))
        {
            return declarations;
        }

        return Parent?.FindDeclarations(name) ?? [];
    }

    [Obsolete("Use GetDeclaredHere instead")]
    public IEnumerable<Symbol> FindAllDeclarations(Name name)
    {
        return _declarations
            .GetValueOrDefault(name, [])
            .Concat(Parent?.FindAllDeclarations(name) ?? []);
    }
}
