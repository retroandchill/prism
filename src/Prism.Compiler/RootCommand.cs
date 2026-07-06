// @file RootCommand.cs
// 
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using DotMake.CommandLine;
using Prism.Core;

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

    public async Task RunAsync()
    {
        var compiler = new CompilerExecutor();
        await compiler.CompileAsync(Input);
    }
}
