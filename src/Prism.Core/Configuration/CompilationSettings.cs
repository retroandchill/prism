using System.Runtime.InteropServices;

namespace Prism.Core.Configuration;

// TODO: Fill-Me Out
public sealed record CompilationSettings
{
    public TargetArchitecture Architecture { get; init; } = TargetArchitecture.CurrentPlatform;

    public TargetOperatingSystem OperatingSystem
    {
        get;
        init
        {
            field = value;
            Toolchain = ToolchainKind.Detect(field);
        }
    } = TargetOperatingSystem.CurrentPlatform;

    public ToolchainKind Toolchain { get; init; } =
        ToolchainKind.Detect(TargetOperatingSystem.CurrentPlatform);
    public PointerWidth PointerWidth => Architecture.PointerWidth;

    public OutputKind OutputKind { get; init; } = OutputKind.Executable;

    public bool IsApplication => OutputKind == OutputKind.Executable;

    public bool IsLibrary => OutputKind is OutputKind.SharedLibrary or OutputKind.StaticLibrary;

    public bool IsSharedLibrary => OutputKind == OutputKind.SharedLibrary;

    public bool GenerateDebugInfo { get; init; } = false;

    public OptimizationLevel OptimizationLevel { get; init; } = OptimizationLevel.Default;

    public static readonly CompilationSettings CurrentPlatform = new();

    public bool IsOptimized => OptimizationLevel != OptimizationLevel.None;

    public bool BuildingCoreLibrary { get; init; }
}
