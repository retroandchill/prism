// @file SyntaxNode.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Source;

namespace Prism.Core.Syntax;

public abstract record SyntaxNode(TextSpan Span);
