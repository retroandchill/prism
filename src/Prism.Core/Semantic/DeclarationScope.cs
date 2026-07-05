// @file DeclarationScope.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Frozen;
using System.Collections.Immutable;
using Prism.Core.Ast;
using Prism.Core.Strings;

namespace Prism.Core.Semantic;

public sealed class DeclarationScope
{
    public DeclarationScope? Parent { get; }
    private readonly FrozenDictionary<Name, ImmutableArray<SymbolDeclaration>> _declarations;

    internal DeclarationScope(
        Dictionary<Name, List<SymbolDeclarationBuilder>> declarations,
        DeclarationScope? parent = null
    )
    {
        Parent = parent;
        _declarations = declarations.ToFrozenDictionary(
            x => x.Key,
            x => x.Value.Select(b => b.Build(this)).ToImmutableArray()
        );
    }

    public ImmutableArray<SymbolDeclaration> FindDeclarations(Name name)
    {
        if (_declarations.TryGetValue(name, out var declarations))
        {
            return declarations;
        }

        return Parent?.FindDeclarations(name) ?? [];
    }

    public IEnumerable<SymbolDeclaration> FindAllDeclarations(Name name)
    {
        return _declarations
            .GetValueOrDefault(name, [])
            .Concat(Parent?.FindAllDeclarations(name) ?? []);
    }
}
