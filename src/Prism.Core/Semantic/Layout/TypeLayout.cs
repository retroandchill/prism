namespace Prism.Core.Semantic.Layout;

public closed class TypeLayout
{
    private protected TypeLayout(int size, int alignment)
    {
        Size = size;
        Alignment = alignment;
    }

    private protected TypeLayout(int size)
        : this(size, size) { }

    public int Size { get; }

    public int Alignment { get; }
}
