// @file MetadataNodeEntry.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;

namespace Prism.Metadata.BoundTree.Serialized;

[Archivable]
[ArchivableUnion(ExpressionOffset + 1, typeof(MetadataConstantExpressionEntry))]
[ArchivableUnion(ExpressionOffset + 2, typeof(ThisMetadataExpressionEntry))]
[ArchivableUnion(ExpressionOffset + 3, typeof(MetadataVariableAccessEntry))]
[ArchivableUnion(ExpressionOffset + 4, typeof(MetadataParameterAccessEntry))]
[ArchivableUnion(ExpressionOffset + 5, typeof(MetadataUnaryOperationEntry))]
[ArchivableUnion(ExpressionOffset + 6, typeof(MetadataBinaryOperationEntry))]
[ArchivableUnion(ExpressionOffset + 7, typeof(MetadataAssignmentOperationEntry))]
[ArchivableUnion(ExpressionOffset + 8, typeof(MetadataConditionalEntry))]
[ArchivableUnion(ExpressionOffset + 9, typeof(MetadataInvocationEntry))]
[ArchivableUnion(ExpressionOffset + 10, typeof(MetadataConversionEntry))]
[ArchivableUnion(ExpressionOffset + 11, typeof(MetadataAddressOfEntry))]
[ArchivableUnion(ExpressionOffset + 12, typeof(MetadataDereferenceEntry))]
[ArchivableUnion(ExpressionOffset + 13, typeof(MetadataNewExpressionEntry))]
[ArchivableUnion(ExpressionOffset + 14, typeof(MetadataDefaultExpressionEntry))]
internal closed partial class MetadataNodeEntry
{
    internal const ushort ExpressionOffset = 128;

    internal MetadataNodeEntry(SourceLocationData location)
    {
        Location = location;
    }

    public SourceLocationData Location { get; }
}
