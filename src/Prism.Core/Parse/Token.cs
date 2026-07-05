// @file Token.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Prism.Core.Parse;

[Flags]
public enum TokenFlags : ushort
{
    None = 0,
    Synthetic = 1 << 0,
    Unterminated = 1 << 1,
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct Token(
    TokenKind Kind,
    SourceRange Range,
    TokenFlags Flags = TokenFlags.None
)
{
    public bool IsSynthetic => Flags.HasFlag(TokenFlags.Synthetic);

    public bool IsUnterminated => Flags.HasFlag(TokenFlags.Unterminated);
}
