// @file SyntaxFlags.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Syntax;

[Flags]
internal enum SyntaxFlags
{
    None = 0,
    NotMissing = 1 << 0,
    ContainsDiagnostics = 1 << 1,
    ContainsSkippedText = 1 << 2,
    ContainsStructuredTrivia = 1 << 3,

    InheritMask = NotMissing | ContainsDiagnostics | ContainsSkippedText | ContainsStructuredTrivia,
}
