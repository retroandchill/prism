using System.Diagnostics;
using Prism.Core.Configuration;

namespace Prism.Core.Semantic.Layout;

public closed class ScalarLayout : TypeLayout
{
    internal const ulong BitsPerByte = 8;

    private protected ScalarLayout(TypeLayoutInfo info, ValueValidity? validity = null)
        : base(info)
    {
        Validity = validity ?? UnconstrainedValidity.Instance;
    }

    public ValueValidity Validity { get; }

    public ulong BitWidth => Info.Size * BitsPerByte;
}

public sealed class IntegerLayout : ScalarLayout
{
    internal IntegerLayout(ulong bitWidth, bool isSigned, ValueValidity? validity = null)
        : base(new TypeLayoutInfo(bitWidth / BitsPerByte), validity)
    {
        Debug.Assert(bitWidth % BitsPerByte == 0);
        IsSigned = isSigned;
    }

    public bool IsSigned { get; }
}

public enum FloatKind : byte
{
    Half,
    Single,
    Double,
    FP128,
    X86_FP80,
}

public sealed class FloatLayout : ScalarLayout
{
    internal FloatLayout(FloatKind kind)
        : base(GetFloatLayoutInfo(kind))
    {
        Kind = kind;
    }

    public FloatKind Kind { get; }

    private static TypeLayoutInfo GetFloatLayoutInfo(FloatKind kind)
    {
        return kind switch
        {
            FloatKind.Half => new TypeLayoutInfo(16 / BitsPerByte),
            FloatKind.Single => new TypeLayoutInfo(32 / BitsPerByte),
            FloatKind.Double => new TypeLayoutInfo(64 / BitsPerByte),
            FloatKind.FP128 or FloatKind.X86_FP80 => new TypeLayoutInfo(128 / BitsPerByte),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }
}

public sealed class PointerLayout : ScalarLayout
{
    internal PointerLayout(PointerWidth width, bool isNonNull, uint addressSpace = 0)
        : base(
            new TypeLayoutInfo(width.BitWidth / BitsPerByte),
            isNonNull ? NicheValidity.NonNull : UnconstrainedValidity.Instance
        )
    {
        Width = width;
        AddressSpace = addressSpace;
        IsNonNull = isNonNull;
    }

    public PointerWidth Width { get; }

    public bool IsNonNull { get; }

    public uint AddressSpace { get; }
}
