// @file MsvcDiscovery.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Setup.Configuration;
using Prism.Core.Configuration;

namespace Prism.Core.Codegen.Platforms.Msvc;

internal static class MsvcDiscovery
{
    private const int RegdbEClassnotreg = unchecked((int)0x80040154);

    public static ImmutableArray<MsvcInstallation> FindInstallations()
    {
        try
        {
            var configuration = new SetupConfiguration();

            var enumerator = configuration.EnumAllInstances();

            var result = ImmutableArray.CreateBuilder<MsvcInstallation>();

            const int batchSize = 10;
            var instances = new ISetupInstance[batchSize];

            while (true)
            {
                enumerator.Next(batchSize, instances, out var fetched);

                if (fetched <= 0)
                    break;

                for (var i = 0; i < batchSize; i++)
                {
                    var instance = instances[i];
                    if (instance is not ISetupInstance2 instance2)
                        continue;

                    if (instance2.GetState() != InstanceState.Complete)
                        continue;

                    result.Add(CreateInstallation(instance2));
                }
            }

            return result.DrainToImmutable();
        }
        catch (COMException ex) when (ex.HResult == RegdbEClassnotreg)
        {
            return [];
        }
    }

    private static MsvcInstallation CreateInstallation(ISetupInstance2 instance)
    {
        var components = GetComponentIds(instance);

        return new MsvcInstallation
        {
            InstallationPath = instance.GetInstallationPath(),

            InstallationVersion = Version.Parse(instance.GetInstallationVersion()),

            SupportsX86 = components.Contains("Microsoft.VisualStudio.Component.VC.Tools.x86.x64"),

            SupportsX64 = components.Contains("Microsoft.VisualStudio.Component.VC.Tools.x86.x64"),

            SupportsArm64 = components.Contains("Microsoft.VisualStudio.Component.VC.Tools.ARM64"),
        };
    }

    private static ImmutableHashSet<string> GetComponentIds(ISetupInstance2 instance)
    {
        var packages = instance.GetPackages();

        return packages
            .Select(static package => package.GetId())
            .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public static MsvcToolset LocateToolset(
        MsvcInstallation installation,
        TargetArchitecture architecture
    )
    {
        var toolsPath = Path.Join(installation.InstallationPath, "VC", "Tools", "MSVC");

        var versions = Directory.GetDirectories(toolsPath);
        Version? version = null;
        foreach (var v in versions)
        {
            if (!Version.TryParse(Path.GetFileName(v), out var ver))
                continue;

            if (version == null || ver > version)
                version = ver;
        }

        Debug.Assert(version is not null);

        var versionFolder = Path.Join(toolsPath, version.ToString());
        var architectureFolder = architecture.ArchitectureFolder;

        var binFolder = Path.Join(versionFolder, "bin", GetHostFolder(), architectureFolder);
        var includeFolder = Path.Join(versionFolder, "include");
        var libFolder = Path.Join(versionFolder, "lib", architectureFolder);

        return new MsvcToolset
        {
            Version = version,
            RootPath = versionFolder,
            IncludePath = includeFolder,
            LibraryPath = libFolder,
            LinkerPath = binFolder,
        };
    }

    private static string GetHostFolder()
    {
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86 => "Hostx86",
            Architecture.X64 => "Hostx64",
            Architecture.Arm64 => "HostArm64",
            _ => throw new NotSupportedException("Unsupported architecture"),
        };
    }
}
