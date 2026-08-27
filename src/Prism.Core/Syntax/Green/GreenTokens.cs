// @file FixedTokens.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Syntax.Green;

internal static class GreenTokens
{
    public static readonly GreenToken EndOfFile = new(SyntaxKind.EndOfFileToken, 0);
    public static readonly GreenToken BadToken = new(SyntaxKind.BadToken, 0);
}
