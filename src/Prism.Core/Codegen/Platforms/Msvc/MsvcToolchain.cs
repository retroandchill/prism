// @file MsvcToolchain.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Prism.Core.Codegen.Platforms.Msvc;

internal sealed class MsvcToolchain(
    MsvcInstallation installation,
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
        cancellationToken.ThrowIfCancellationRequested();

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = linkerPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        AddLinkArguments(process.StartInfo, request);

        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        return new ToolResult(process.ExitCode, stdout, stderr);
    }

    private void AddLinkArguments(ProcessStartInfo startInfo, LinkRequest request)
    {
        startInfo.ArgumentList.Add($"/Out:{request.OutputPath}");

        if (request.OutputKind == LinkOutput.DynamicLibrary)
            startInfo.ArgumentList.Add("/DLL");

        if (request.Subsystem is { } subsystem)
            startInfo.ArgumentList.Add($"/SUBSYSTEM:{subsystem}");

        if (request.EntryPoint is { } entryPoint)
            startInfo.ArgumentList.Add($"/ENTRY:{entryPoint}");

        if (request.DebugInfo != DebugInfoKind.None)
        {
            startInfo.ArgumentList.Add("/DEBUG");

            if (request.DebugInfoPath is { } pdb)
                startInfo.ArgumentList.Add($"/PDB:{pdb}");
        }

        if (request.ImportLibraryPath is { } implib)
            startInfo.ArgumentList.Add($"/IMPLIB:{implib}");

        foreach (var export in request.ExportedSymbols)
            startInfo.ArgumentList.Add($"/EXPORT:{export}");

        foreach (var path in request.LibraryPaths)
            startInfo.ArgumentList.Add($"/LIBPATH:{path}");

        startInfo.ArgumentList.Add($"/LIBPATH:{toolset.LibraryPath}");
        startInfo.ArgumentList.Add($"/LIBPATH:{windowsSdk.UmPath}");
        startInfo.ArgumentList.Add($"/LIBPATH:{windowsSdk.UcrtPath}");

        foreach (var file in request.ObjectFiles)
            startInfo.ArgumentList.Add(file);

        foreach (var library in request.Libraries)
            startInfo.ArgumentList.Add(library);

        startInfo.ArgumentList.Add("kernel32.lib");
        startInfo.ArgumentList.Add("user32.lib");
    }
}
