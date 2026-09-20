using System.Diagnostics;

namespace Prism.Core.Semantic.Layout;

public class IntegerTypeLayout : TypeLayout
{
    internal IntegerTypeLayout(int bitWidth, bool isSigned)
        : base(bitWidth / 8)
    {
        Debug.Assert(bitWidth % 8 == 0);

        BitWidth = bitWidth;
        IsSigned = isSigned;
    }

    public int BitWidth { get; }
    public bool IsSigned { get; }
}
