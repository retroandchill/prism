// @file MsvcToolchain.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Prism.Core.Codegen.Platforms.Msvc;

internal sealed class MsvcToolchain(
    MsvcToolset toolset,
    WindowsSdk windowsSdk,
    string linkerPath,
    string librarianPath
) : IToolchain
{
    public async Task<ToolResult> LinkAsync(
        LinkRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var arguments = CreateLinkArguments(request);
        return await RunToolAsync(linkerPath, arguments, cancellationToken);
    }

    private static async Task<ToolResult> RunToolAsync(
        string binaryPath,
        List<string> arguments,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = binaryPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var arg in arguments)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        return new ToolResult(process.ExitCode, stdout, stderr);
    }

    private List<string> CreateLinkArguments(LinkRequest request)
    {
        var arguments = new List<string> { $"/Out:{request.OutputPath}" };

        if (request.OutputKind == LinkOutput.DynamicLibrary)
            arguments.Add("/DLL");

        if (request.Subsystem is { } subsystem)
            arguments.Add($"/SUBSYSTEM:{subsystem}");

        if (request.EntryPoint is { } entryPoint)
            arguments.Add($"/ENTRY:{entryPoint}");

        if (request.DebugInfo != DebugInfoKind.None)
        {
            arguments.Add("/DEBUG");

            if (request.DebugInfoPath is { } pdb)
                arguments.Add($"/PDB:{pdb}");
        }

        if (request.ImportLibraryPath is { } implib)
            arguments.Add($"/IMPLIB:{implib}");

        arguments.AddRange(request.ExportedSymbols.Select(export => $"/EXPORT:{export}"));

        arguments.AddRange(request.LibraryPaths.Select(path => $"/LIBPATH:{path}"));

        arguments.Add($"/LIBPATH:{toolset.LibraryPath}");
        arguments.Add($"/LIBPATH:{windowsSdk.UmPath}");
        arguments.Add($"/LIBPATH:{windowsSdk.UcrtPath}");

        arguments.AddRange(request.ObjectFiles.AsSpan());

        arguments.AddRange(request.Libraries.AsSpan());

        arguments.Add("kernel32.lib");
        arguments.Add("user32.lib");
        return arguments;
    }

    public async Task<ToolResult> CreateStaticLibraryAsync(
        StaticLibraryRequest request,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var arguments = new List<string>(1 + request.ObjectFiles.Length)
        {
            $"/OUT:{request.OutputPath}",
        };

        arguments.AddRange(request.ObjectFiles.AsSpan());

        return await RunToolAsync(librarianPath, arguments, cancellationToken);
    }
}
