// @file Node.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Text;

namespace Prism.Metadata.BoundTree;

public readonly record struct SyntaxLocation(string File, TextSpan SourceSpan);

public closed class MetadataNode
{
    internal MetadataNode(SyntaxLocation location)
    {
        Location = location;
    }

    public SyntaxLocation Location { get; }
}
