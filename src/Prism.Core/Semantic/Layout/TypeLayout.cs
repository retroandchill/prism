namespace Prism.Core.Semantic.Layout;

internal readonly record struct TypeLayoutInfo(ulong Size, ulong Alignment)
{
    public TypeLayoutInfo(ulong sizeAndAlignment)
        : this(sizeAndAlignment, sizeAndAlignment) { }

    public static TypeLayoutInfo Empty => new(0, 0);
}

public closed class TypeLayout
{
    private protected TypeLayout(TypeLayoutInfo info)
    {
        Info = info;
    }

    internal TypeLayoutInfo Info { get; }

    public ulong Size => Info.Size;

    public ulong Alignment => Info.Alignment;
}
