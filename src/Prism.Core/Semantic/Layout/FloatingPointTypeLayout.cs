using System.Diagnostics;

namespace Prism.Core.Semantic.Layout;

public class FloatingPointTypeLayout : TypeLayout
{
    internal FloatingPointTypeLayout(int bitWidth)
        : base(bitWidth / 8)
    {
        Debug.Assert(bitWidth % 8 == 0);

        BitWidth = bitWidth;
    }

    public int BitWidth { get; }
}
