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
    private static readonly PointerWidth CurrentPlatformPointerWidth = Environment.Is64BitProcess
        ? PointerWidth.X64
        : PointerWidth.X32;

    extension(PointerWidth width)
    {
        public int BitWidth => width.AsUnderlyingType();

        public static PointerWidth CurrentPlatform => CurrentPlatformPointerWidth;
    }
}
