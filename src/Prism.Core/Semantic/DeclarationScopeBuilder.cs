// @file DeclarationScopeBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;
using Prism.Core.Strings;
using Prism.Core.Utils;

namespace Prism.Core.Semantic;

public sealed class DeclarationScopeBuilder
{
    private readonly Dictionary<Name, List<SymbolDeclarationBuilder>> _declarations;

    internal DeclarationScopeBuilder(int initialCapacity)
    {
        _declarations = new Dictionary<Name, List<SymbolDeclarationBuilder>>(initialCapacity);
    }

    public void AddDeclaration(
        Name name,
        DeclarationSyntax syntax,
        DeclarationScopeBuilder? childScope = null
    )
    {
        var builder = _declarations.GetOrAdd(name, () => []);
        builder.Add(
            new SymbolDeclarationBuilder
            {
                Name = name,
                Syntax = syntax,
                ChildScope = childScope,
            }
        );
    }

    public DeclarationScope Build(DeclarationScope? parent = null)
    {
        return new DeclarationScope(_declarations, parent);
    }
}
