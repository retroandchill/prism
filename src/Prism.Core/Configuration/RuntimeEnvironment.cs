// @file RuntimeEnvironment.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Prism.Core.Configuration;

public enum RuntimeEnvironment : byte
{
    Gnu,
    Musl,
    Msvc,
    None,
}

public static class RuntimeEnvironmentExtensions
{
    extension(RuntimeEnvironment)
    {
        public static RuntimeEnvironment CurrentPlatform
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return RuntimeEnvironment.Msvc;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return RuntimeEnvironment.None;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return RuntimeInformation.RuntimeIdentifier.Contains(
                        "musl",
                        StringComparison.OrdinalIgnoreCase
                    )
                        ? RuntimeEnvironment.Musl
                        : RuntimeEnvironment.Gnu;
                }

                return RuntimeEnvironment.None;
            }
        }
    }
}
