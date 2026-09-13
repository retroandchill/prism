// @file RootCommand.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using DotMake.CommandLine;
using Prism.Core.Compiling;
using Prism.Core.Configuration;
using Prism.Core.Syntax;

namespace Prism.Compiler;

[CliCommand(Description = "Invoke the Prism compiler")]
public class RootCommand
{
    internal const string Executable = "executable";
    private const string StaticLib = "static-lib";
    private const string SharedLib = "shared-lib";

    [CliOption(Alias = "n", Description = "The name of the target assembly", Required = true)]
    public string Name { get; set; } = null!;

    [CliArgument(
        Description = "Input files to compile",
        Required = true,
        ValidationRules = CliValidationRules.ExistingFile
    )]
    public IEnumerable<FileInfo> Input { get; set; } = null!;

    [CliOption(
        Alias = "k",
        Description = "The type of output",
        AllowedValues = [Executable, StaticLib, SharedLib]
    )]
    public string Kind { get; set; } = Executable;

    public async Task<int> RunAsync(CliContext context)
    {
        var syntaxTress = await Task.WhenAll(
            Input.Select(f => ParseFile(f, context.CancellationToken)).ToArray()
        );

        var compilation = Compilation.Create(
            Name,
            ImmutableCollectionsMarshal.AsImmutableArray(syntaxTress),
            new CompilationSettings { OutputKind = GetOutputKind() }
        );

        var (succeeded, diagnostics) = await compilation.EmitAsync(
            Directory.GetCurrentDirectory(),
            context.CancellationToken
        );

        if (succeeded)
        {
            await context.Output.WriteLineAsync("Compilation Succeeded");
        }
        else
        {
            await context.Output.WriteLineAsync("Compilation Failed");
        }

        foreach (var diagnostic in diagnostics)
            await context.Output.WriteLineAsync(diagnostic.ToString());

        return succeeded ? 0 : 1;
    }

    private OutputKind GetOutputKind()
    {
        return Kind switch
        {
            Executable => OutputKind.Executable,
            StaticLib => OutputKind.StaticLibrary,
            SharedLib => OutputKind.SharedLibrary,
            _ => throw new ArgumentOutOfRangeException(nameof(Kind), Kind, null),
        };
    }

    private static async Task<SyntaxTree> ParseFile(
        FileInfo fileInfo,
        CancellationToken cancellationToken
    )
    {
        string program;
        await using (var stream = fileInfo.OpenRead())
        {
            using var reader = new StreamReader(stream);
            program = await reader.ReadToEndAsync(cancellationToken);
        }

        return SyntaxTree.Parse(program);
    }
}
