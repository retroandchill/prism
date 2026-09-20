using Prism.Core.Configuration;

namespace Prism.Core.Semantic.Layout;

public sealed class PointerTypeLayout : TypeLayout
{
    private PointerTypeLayout(PointerWidth pointerWidth)
        : base(pointerWidth.BitWidth / 8) { }

    internal static readonly PointerTypeLayout X32 = new(PointerWidth.X32);
    internal static readonly PointerTypeLayout X64 = new(PointerWidth.X64);
}
