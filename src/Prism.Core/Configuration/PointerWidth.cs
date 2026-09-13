using NetEscapades.EnumGenerators;

namespace Prism.Core.Configuration;

[EnumExtensions]
public enum PointerWidth : ushort
{
    X32 = 32,
    X64 = 64,
}

public static partial class PointWidthExtensions
{
    extension(PointerWidth width)
    {
        public int BitWidth => width.AsUnderlyingType();
    }
}
