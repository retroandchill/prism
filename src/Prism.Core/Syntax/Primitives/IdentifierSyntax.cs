// @file IdentifierSyntax.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Source;
using Prism.Core.Strings;

namespace Prism.Core.Syntax.Primitives;

public sealed record IdentifierSyntax(Name Name, TextSpan Span) : SyntaxNode(Span);
