// @file TargetInfo.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Configuration;

namespace Prism.Core.Metadata;

public readonly record struct TargetInfo(
    TargetArchitecture Architecture,
    TargetOperatingSystem OperatingSystem,
    ToolchainKind Toolchain
)
{
    public TargetInfo()
        : this(TargetArchitecture.CurrentPlatform, TargetOperatingSystem.CurrentPlatform) { }

    public TargetInfo(TargetArchitecture architecture, TargetOperatingSystem operatingSystem)
        : this(architecture, operatingSystem, ToolchainKind.Detect(operatingSystem)) { }

    public static TargetInfo CurrentPlatform => new();
}
