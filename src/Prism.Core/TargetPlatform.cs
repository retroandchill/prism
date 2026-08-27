using System.Runtime.CompilerServices;

namespace Prism.Core;

public sealed record TargetPlatform
{
    public required int PointerSize { get; init; }

    public static TargetPlatform Default { get; } = new() { PointerSize = Unsafe.SizeOf<IntPtr>() };
}
