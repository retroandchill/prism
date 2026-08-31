using System.Runtime.InteropServices;

namespace Prism.Core.Configuration;

// TODO: Fill-Me Out
public sealed record CompilationSettings
{
    public PointerWidth PointerWidth { get; init; } = PointerWidth.X64;

    public OutputKind OutputKind { get; init; } = OutputKind.Executable;

    public bool IsApplication => OutputKind == OutputKind.Executable;

    public bool IsLibrary => OutputKind is OutputKind.SharedLibrary or OutputKind.StaticLibrary;

    public bool IsSharedLibrary => OutputKind == OutputKind.SharedLibrary;

    public static readonly CompilationSettings CurrentPlatform = new();
}
