using System.Collections.Immutable;

namespace Prism.Core.Semantic.Layout;

public closed class NullableLayout : TypeLayout
{
    internal NullableLayout(TypeLayoutInfo info)
        : base(info) { }
}

public class TaggedNullableLayout : NullableLayout
{
    internal TaggedNullableLayout(TypeLayout payload)
        : base(CalculateLayoutInfo(payload))
    {
        Payload = payload;
        StructRepresentation = CreateStructRepresentation(payload);
    }

    public TypeLayout Payload { get; }
    public StructLayout StructRepresentation { get; }

    private static StructLayout CreateStructRepresentation(TypeLayout valueLayout)
    {
        var boolLayout = LayoutCalculator.BoolLayout;
        var tagField = new StructField("is_some", LayoutCalculator.BoolLayout, 0);

        var maxAlignment = Math.Max(boolLayout.Alignment, valueLayout.Alignment);
        var payloadOffset = AlignTo(boolLayout.Size, valueLayout.Alignment);

        var valueField = new StructField("value", valueLayout, payloadOffset);

        var totalSize = AlignTo(payloadOffset + valueLayout.Size, maxAlignment);

        return new StructLayout(
            new TypeLayoutInfo(totalSize, maxAlignment),
            [tagField, valueField]
        );
    }

    private static TypeLayoutInfo CalculateLayoutInfo(TypeLayout valueLayout)
    {
        var maxAlignment = Math.Max(1u, valueLayout.Alignment);
        var payloadOffset = AlignTo(1, valueLayout.Alignment);
        var totalSize = AlignTo(payloadOffset + valueLayout.Size, maxAlignment);
        return new TypeLayoutInfo(totalSize, maxAlignment);
    }

    private static ulong AlignTo(ulong offset, ulong alignment)
    {
        return (offset + alignment - 1) & ~(alignment - 1);
    }
}

public closed record NullValue;

public sealed record ScalarNullValue(ulong BitPattern) : NullValue;

public sealed record CompositeNullValue(int FieldIndex, NullValue Value) : NullValue;

public sealed class OptimizedNullableLayout : NullableLayout
{
    internal OptimizedNullableLayout(TypeLayout underlyingLayout, NullValue nullValue)
        : base(underlyingLayout.Info)
    {
        UnderlyingLayout = underlyingLayout;
        NullValue = nullValue;
    }

    public TypeLayout UnderlyingLayout { get; }

    public NullValue NullValue { get; }
}
