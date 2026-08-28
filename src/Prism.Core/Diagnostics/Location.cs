// @file Location.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Syntax;
using Prism.Core.Text;

namespace Prism.Core.Diagnostics;

public abstract record Location;

public sealed record SourceLocation(SyntaxTree SyntaxTree, TextSpan SourceSpan) : Location
{
    public SourceLocation(SyntaxReference reference)
        : this(reference.Syntax.SyntaxTree, reference.SourceSpan) { }

    public SourceLocation(SyntaxNode node)
        : this(node.SyntaxTree, node.FullSpan) { }

    public SourceLocation(in SyntaxToken token)
        : this(token.SyntaxTree, token.FullSpan) { }

    public FileSourcePositionSpan PositionSpan => SyntaxTree.GetPositionSpan(SourceSpan);
}
