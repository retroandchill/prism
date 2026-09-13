// @file ToolchainFactory.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Codegen.Platforms.Msvc;
using Prism.Core.Configuration;

namespace Prism.Core.Codegen;

internal readonly record struct ToolchainRequest(
    TargetArchitecture Architecture,
    TargetOperatingSystem OperatingSystem,
    ToolchainKind Toolchain
);

internal sealed class ToolchainNotFoundException(string message) : Exception(message);

internal static class ToolchainFactory
{
    public static IToolchain Create(ToolchainRequest request)
    {
        return request.Toolchain switch
        {
            ToolchainKind.Auto when request.OperatingSystem == TargetOperatingSystem.Windows =>
                CreateMsvc(request),
            ToolchainKind.Msvc => CreateMsvc(request),
            _ => throw new NotSupportedException("This platform is not yet supported"),
        };
    }

    private static MsvcToolchain CreateMsvc(ToolchainRequest request)
    {
        var installations = MsvcDiscovery.FindInstallations();
        var installation = SelectInstallation(installations, request.Architecture);
        var toolset = MsvcDiscovery.LocateToolset(installation, request.Architecture);
        var windowsSdk = WindowsSdkDiscovery.LocateSdk(request.Architecture);
        return new MsvcToolchain(
            installation,
            toolset,
            windowsSdk,
            Path.Combine(toolset.LinkerPath, "link.exe"),
            Path.Combine(toolset.LinkerPath, "lib.exe")
        );
    }

    private static MsvcInstallation SelectInstallation(
        ImmutableArray<MsvcInstallation> installations,
        TargetArchitecture architecture
    )
    {
        return installations
                .Where(x => Supports(x, architecture))
                .OrderByDescending(x => x.InstallationVersion)
                .FirstOrDefault()
            ?? throw new ToolchainNotFoundException("Could not find the required MSVC toolchain");
    }

    private static bool Supports(MsvcInstallation installation, TargetArchitecture architecture)
    {
        return architecture switch
        {
            TargetArchitecture.X86 => installation.SupportsX86,
            TargetArchitecture.X64 => installation.SupportsX64,
            TargetArchitecture.Arm => false,
            TargetArchitecture.Arm64 => installation.SupportsArm64,
            _ => throw new ArgumentOutOfRangeException(nameof(architecture), architecture, null),
        };
    }
}
