// @file LinkRequest.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Reflection.PortableExecutable;
using Prism.Core.Configuration;

namespace Prism.Core.Codegen;

internal enum LinkOutput
{
    Executable,
    DynamicLibrary,
}

public enum Subsystem
{
    Console,
    Windows,
}

public enum DebugInfoKind
{
    None,
    ProgramDatabase,
}

internal sealed class LinkRequest
{
    public required LinkOutput OutputKind { get; init; }

    public required string OutputPath { get; init; }

    public ImmutableArray<string> ObjectFiles { get; init; } = [];

    public ImmutableArray<string> Libraries { get; init; } = [];

    public ImmutableArray<string> LibraryPaths { get; init; } = [];

    public string? EntryPoint { get; init; }

    public Subsystem? Subsystem { get; init; }

    public DebugInfoKind DebugInfo { get; init; }

    public string? DebugInfoPath { get; init; }

    public string? ImportLibraryPath { get; init; }

    public ImmutableArray<string> ExportedSymbols { get; init; } = [];

    public string? ModuleDefinitionFile { get; init; }
}
