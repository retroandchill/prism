using JetBrains.Annotations;
using Prism.Metadata.BoundTree;
using Prism.Metadata.BoundTree.Serialized;

namespace Prism.Metadata.Serialization;

[MustDisposeResource]
internal sealed class VariableInitializerHeapBuilder()
    : SerializedHeapBuilder<VariableInitializerHandle, MetadataExpression>(
        VariableInitializerHeapMetadata.HandleShift
    );

internal static class VariableInitializerHeapMetadata
{
    public const int HandleShift = 1;
}
