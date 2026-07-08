// @file ParameterSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;

namespace Prism.Core.Semantic.Symbols;

public sealed class ParameterSymbol : ValueSymbol
{
    public bool IsMutable { get; }
    
    public override bool IsAssignable => IsMutable;
    
    internal ParameterSymbol(ParameterDeclarationSyntax declaration,
        Compilation compilation,
        Symbol? containingSymbol = null) : base(declaration.Identifier.Name, compilation, declaration, containingSymbol)
    {
        IsMutable = declaration.IsMutable;
    }
}
