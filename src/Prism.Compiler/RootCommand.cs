// @file RootCommand.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using DotMake.CommandLine;
using Prism.Core.Compiling;
using Prism.Core.Syntax;

namespace Prism.Compiler;

[CliCommand(Description = "Invoke the Prism compiler")]
public class RootCommand
{
    [CliArgument(
        Description = "Input files to compile",
        Required = true,
        ValidationRules = CliValidationRules.ExistingFile
    )]
    public IEnumerable<FileInfo> Input { get; set; } = null!;

    [CliOption(Alias = "o", Description = "Output file")]
    public FileInfo Output { get; set; } = null!;

    public async Task<int> RunAsync(CliContext context)
    {
        Console.WriteLine("Parsing Program");
        var syntaxTress = await Task.WhenAll(
            Input.Select(f => ParseFile(f, context.CancellationToken)).ToArray()
        );

        var compilation = Compilation.Create(
            "test",
            ImmutableCollectionsMarshal.AsImmutableArray(syntaxTress)
        );

        if (compilation.Emit(Output.Directory!.FullName) is (false, var diagnostics))
        {
            Console.WriteLine("Compilation Failed");
            foreach (var diagnostic in diagnostics)
                Console.WriteLine(diagnostic);
            return 1;
        }

        Console.WriteLine("Compilation Succeeded");
        return 0;
    }

    private async Task<SyntaxTree> ParseFile(FileInfo fileInfo, CancellationToken cancellationToken)
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
