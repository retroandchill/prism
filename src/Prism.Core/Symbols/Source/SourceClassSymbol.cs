// @file SourceAttributeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Binding;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using Prism.Core.Utils;
using ZLinq;

namespace Prism.Core.Symbols.Source;

internal sealed class SourceClassSymbol : SourceNamedTypeSymbol
{
    internal SourceClassSymbol(MergedTypeDeclaration declaration, Symbol containingSymbol)
        : base(declaration, containingSymbol)
    {
        Debug.Assert(declaration.Kind == DeclarationKind.Class);

        var declarations = declaration.Declarations;
        Debug.Assert(declarations.Length == 1);

        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        foreach (var decl in declaration.Declarations)
        {
            compilation.CacheSymbol(decl.SyntaxReference.Syntax, this);
        }
    }
}
