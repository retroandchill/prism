// @file Identifier.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Parse;
using Prism.Core.Strings;

namespace Prism.Core.Ast;

public readonly record struct IdentifierSyntax(
    Name Name,
    SourceRange Range,
    SyntaxFlags Flags = SyntaxFlags.None
);

public enum Modifier
{
    Extern,
}

public readonly record struct ModifierSyntax(
    Modifier Modifier,
    SyntaxFlags Flags,
    SourceRange Range
);
