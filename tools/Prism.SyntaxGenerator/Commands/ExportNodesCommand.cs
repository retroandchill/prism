// @file ExportNodesCommand.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Text.Json;
using DotMake.CommandLine;
using Prism.SyntaxGenerator.Models;
using Prism.SyntaxGenerator.Resolution;

namespace Prism.SyntaxGenerator.Commands;

[CliCommand(Description = "Export Prism syntax schema", Name = "generate")]
public class ExportNodesCommand
{
    [CliOption(Description = "Path to the imported nodes", Name = "input")]
    public string InputPath { get; set; } = null!;

    [CliOption(Description = "Path for the exported source files", Name = "output")]
    public string OutputPath { get; set; } = null!;

    public async Task RunAsync(CliContext context)
    {
        var options = ModelJsonSerializerContext.CompositeOptions;
        SyntaxSpecification syntax;
        await using (var stream = File.OpenRead(InputPath))
        {
            var deserialized = await JsonSerializer.DeserializeAsync<SyntaxSpecification>(
                stream,
                options,
                context.CancellationToken
            );
            syntax =
                deserialized
                ?? throw new InvalidOperationException(
                    "Failed to deserialize syntax specification"
                );
        }

        var builder = new SyntaxModelBuilder();
        var resolvedModel = builder.Build(syntax);
        var generatedModules = SpecificationTransformer.Transform(resolvedModel);
        var emitter = new Emitter();
        await emitter.EmitNodesAsync(generatedModules, OutputPath, context.CancellationToken);
    }
}
