using NetEscapades.EnumGenerators;

namespace Prism.Configuration;

[EnumExtensions]
public enum PointerWidth : ushort
{
    X32 = 32,
    X64 = 64,
}

public static partial class PointerWidthExtensions
{
    extension(PointerWidth width)
    {
        public ushort BitWidth => width.AsUnderlyingType();
    }
}
