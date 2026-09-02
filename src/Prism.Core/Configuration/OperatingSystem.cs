// @file OperatingSystem.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Prism.Core.Configuration;

public enum OperatingSystem : byte
{
    Linux,
    Windows,
    MacOS,
    Freestanding,
}

public static class OperatingSystemExtensions
{
    extension(OperatingSystem)
    {
        public static OperatingSystem CurrentPlatform
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return OperatingSystem.Linux;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return OperatingSystem.Windows;

                return RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? OperatingSystem.MacOS
                    : OperatingSystem.Freestanding;
            }
        }
    }
}
