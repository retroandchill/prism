// @file MsvcInstallation.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Core.Codegen.Platforms.Msvc;

internal sealed record MsvcInstallation
{
    public required string InstallationPath { get; init; }

    public required Version InstallationVersion { get; init; }

    public required bool SupportsX86 { get; init; }

    public required bool SupportsX64 { get; init; }

    public required bool SupportsArm64 { get; init; }
}
