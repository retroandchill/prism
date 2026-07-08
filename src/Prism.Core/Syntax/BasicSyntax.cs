// @file Identifier.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Prism.Core.Parse;
using Prism.Core.Strings;

namespace Prism.Core.Syntax;

[StructLayout(LayoutKind.Auto)]
public readonly record struct IdentifierSyntax(
    Name Name,
    TextSpan Range,
    SyntaxFlags Flags = SyntaxFlags.None
);

public enum Modifier : ushort
{
    Extern,
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct ModifierSyntax(
    Modifier Modifier,
    TextSpan Range,
    SyntaxFlags Flags = SyntaxFlags.None
);
