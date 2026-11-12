// @file Location.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Text;

namespace Prism;

public closed record Location
{
    public static readonly NoLocation None = new();
}

public sealed record NoLocation : Location
{
    internal NoLocation() { }
}

public sealed record SourceLocation(ISourceDocument SyntaxTree, TextSpan SourceSpan) : Location
{
    public FileSourcePositionSpan PositionSpan => SyntaxTree.GetPositionSpan(SourceSpan);

    public override string ToString()
    {
        return $"{PositionSpan}";
    }
}

public sealed record MetadataLocation(IMetadataReferenceProvider Reference) : Location
{
    public override string ToString() => Reference.Identity.FullName;
}
