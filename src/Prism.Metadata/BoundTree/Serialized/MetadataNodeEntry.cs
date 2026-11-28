// @file MetadataNodeEntry.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;

namespace Prism.Metadata.BoundTree.Serialized;

internal closed class MetadataNodeEntry
{
    internal const ushort ExpressionOffset = 128;

    internal MetadataNodeEntry(SyntaxLocationData location)
    {
        Location = location;
    }

    public SyntaxLocationData Location { get; }
}
