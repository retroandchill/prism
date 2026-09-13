// @file WindowsSdkDiscovery.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Prism.Core.Configuration;

namespace Prism.Core.Codegen.Platforms.Msvc;

internal static class WindowsSdkDiscovery
{
    public static WindowsSdk LocateSdk(TargetArchitecture architecture)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new NotSupportedException("Cannot locate Windows SDK on non-Windows platforms.");

        var sdkPath = GetDefaultSdkPath();

        var sdks = new List<WindowsSdk>();
        var baseIncludePath = Path.Combine(sdkPath, "Include");
        var baseLibPath = Path.Combine(sdkPath, "Lib");
        var baseBinPath = Path.Combine(sdkPath, "bin");

        var validIncludePaths = Directory.GetDirectories(baseIncludePath);
        foreach (var includePaths in validIncludePaths)
        {
            var versionString = Path.GetFileName(includePaths);
            var libPath = Path.Combine(baseLibPath, versionString);
            var binPath = Path.Combine(baseBinPath, versionString);
            if (!Directory.Exists(libPath) || !Directory.Exists(binPath))
                continue;

            if (!Version.TryParse(versionString, out var version))
                continue;

            sdks.Add(
                new WindowsSdk
                {
                    Version = version,
                    RootPath = sdkPath,
                    IncludePath = includePaths,
                    UcrtPath = Path.Combine(libPath, "ucrt", architecture.ArchitectureFolder),
                    UmPath = Path.Combine(libPath, "um", architecture.ArchitectureFolder),
                    BinPath = Path.Combine(binPath, architecture.ArchitectureFolder),
                }
            );
        }

        return sdks.OrderByDescending(x => x.Version).FirstOrDefault()
            ?? throw new ToolchainNotFoundException("Could not locate a valid Windows SDK");
    }

    private static string GetDefaultSdkPath()
    {
        return Environment.Is64BitOperatingSystem
            ? "C:/Program Files (x86)/Windows Kits/10"
            : "C:/Program Files/Windows Kits/10";
    }
}
