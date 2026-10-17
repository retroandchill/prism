// @file RuntimeEnvironment.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using NetEscapades.EnumGenerators;

namespace Prism.Core.Configuration;

[EnumExtensions]
public enum ToolchainKind : byte
{
    Msvc,
    Clang,
    Gnu,
}

public static partial class ToolchainKindExtensions
{
    extension(ToolchainKind)
    {
        public static ToolchainKind Detect(TargetOperatingSystem operatingSystem)
        {
            return operatingSystem switch
            {
                TargetOperatingSystem.Linux => ToolchainKind.Gnu,
                TargetOperatingSystem.Windows => ToolchainKind.Msvc,
                TargetOperatingSystem.MacOS => ToolchainKind.Clang,
                TargetOperatingSystem.Freestanding => ToolchainKind.Clang,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(operatingSystem),
                    operatingSystem,
                    null
                ),
            };
        }
    }
}
