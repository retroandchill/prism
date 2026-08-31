using System.Runtime.InteropServices;

namespace Prism.Core.Configuration;

// TODO: Fill-Me Out
public sealed record CompilationSettings
{
    public PointerWidth PointerWidth { get; init; } = PointerWidth.X64;

    public static readonly CompilationSettings CurrentPlatform = new();
}
