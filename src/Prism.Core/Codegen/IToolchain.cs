// @file IToolchain.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Codegen;

public readonly record struct ToolResult(int ExitCode, string StdOut, string StdErr)
{
    public bool Success => ExitCode == 0;
}

internal interface IToolchain
{
    Task<ToolResult> LinkAsync(LinkRequest request, CancellationToken cancellationToken = default);

    Task<ToolResult> CreateStaticLibraryAsync(
        StaticLibraryRequest request,
        CancellationToken cancellationToken = default
    );
}
