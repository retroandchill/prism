namespace Prism.Core.Semantic.Layout;

public sealed class EmptyLayout : TypeLayout
{
    private EmptyLayout()
        : base(new TypeLayoutInfo(0, 1)) { }

    public static EmptyLayout Instance { get; } = new();
}
