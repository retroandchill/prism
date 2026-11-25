// @file Node.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Metadata.BoundTree.Builders;

public closed class MetadataNodeBuilder
{
    internal MetadataNodeBuilder(SourceLocation location)
    {
        Location = location;
    }

    public SourceLocation Location { get; }
}
