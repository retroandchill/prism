using System.Collections.Immutable;

namespace Prism.Core.Semantic.Layout;

public closed class CompositeLayout : TypeLayout
{
    private protected CompositeLayout(TypeLayoutInfo info)
        : base(info) { }
}

public sealed class ArrayLayout : CompositeLayout
{
    internal ArrayLayout(TypeLayout elementType, ulong elementCount)
        : base(new TypeLayoutInfo(elementType.Size * elementCount, elementType.Alignment))
    {
        ElementType = elementType;
        ElementCount = elementCount;
    }

    public TypeLayout ElementType { get; }

    public ulong ElementCount { get; }
}

public readonly record struct StructField(string Name, TypeLayout Type, ulong ByteOffset);

public sealed class StructLayout : CompositeLayout
{
    internal StructLayout(
        TypeLayoutInfo info,
        ImmutableArray<StructField> fields,
        bool isPacked = false
    )
        : base(info)
    {
        Fields = fields;
        IsPacked = isPacked;
    }

    public ImmutableArray<StructField> Fields { get; }

    public bool IsPacked { get; }
}
