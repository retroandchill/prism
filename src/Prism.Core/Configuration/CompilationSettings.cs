using System.Runtime.InteropServices;

namespace Prism.Core.Configuration;

// TODO: Fill-Me Out
public sealed record CompilationSettings
{
    public TargetArchitecture Architecture { get; init; } = TargetArchitecture.CurrentPlatform;
    public OperatingSystem OperatingSystem { get; init; } = OperatingSystem.CurrentPlatform;
    public RuntimeEnvironment Environment { get; init; } = RuntimeEnvironment.CurrentPlatform;
    public PointerWidth PointerWidth { get; init; } = PointerWidth.CurrentPlatform;

    public OutputKind OutputKind { get; init; } = OutputKind.Executable;

    public bool IsApplication => OutputKind == OutputKind.Executable;

    public bool IsLibrary => OutputKind is OutputKind.SharedLibrary or OutputKind.StaticLibrary;

    public bool IsSharedLibrary => OutputKind == OutputKind.SharedLibrary;

    public static readonly CompilationSettings CurrentPlatform = new();
}
