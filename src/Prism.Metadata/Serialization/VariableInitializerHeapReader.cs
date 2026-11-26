using MagicArchive;
using Prism.Metadata.BoundTree;
using Prism.Metadata.BoundTree.Serialized;
using Prism.Utils;

namespace Prism.Metadata.Serialization;

internal sealed class VariableInitializerHeapReader(
    ReadOnlyMemory<byte> buffer,
    int totalItems,
    MetadataReader metadataReader
)
    : HeapReader<MetadataExpression>(
        buffer,
        totalItems,
        VariableInitializerHeapMetadata.HandleShift
    ),
        IHeapReader<VariableInitializerHeapReader>
{
    private readonly MetadataBoundTreeReader _mapper = new(metadataReader);

    protected override MetadataExpression Read(ref ArchiveReader reader)
    {
        var expressionEntry = reader.ReadValue<MetadataExpressionEntry>().RequireNonNull();
        return _mapper.Map(expressionEntry);
    }

    public static VariableInitializerHeapReader Create(
        ReadOnlyMemory<byte> buffer,
        int totalItems,
        MetadataReader reader
    )
    {
        return new VariableInitializerHeapReader(buffer, totalItems, reader);
    }
}
