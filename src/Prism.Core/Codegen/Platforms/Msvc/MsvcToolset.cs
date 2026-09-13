// @file MsvcToolset.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Codegen.Platforms.Msvc;

internal sealed record MsvcToolset
{
    public required Version Version { get; init; }
    public required string RootPath { get; init; }

    public required string IncludePath { get; init; }
    public required string LibraryPath { get; init; }
    public required string LinkerPath { get; init; }
}
