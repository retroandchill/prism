// @file OperatingSystem.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Prism.Core.Configuration;

public enum TargetOperatingSystem : byte
{
    Linux,
    Windows,
    MacOS,
    Freestanding,
}

public static class OperatingSystemExtensions
{
    extension(TargetOperatingSystem)
    {
        public static TargetOperatingSystem CurrentPlatform
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return TargetOperatingSystem.Linux;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return TargetOperatingSystem.Windows;

                return RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? TargetOperatingSystem.MacOS
                    : TargetOperatingSystem.Freestanding;
            }
        }
    }
}
