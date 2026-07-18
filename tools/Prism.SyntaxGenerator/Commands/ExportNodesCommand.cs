// @file ExportNodesCommand.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.Json;
using DotMake.CommandLine;
using Humanizer;
using Prism.SyntaxGenerator.Emitters;
using Prism.SyntaxGenerator.Models.Spec;
using Prism.SyntaxGenerator.Output;
using Prism.SyntaxGenerator.Resolution;
using ModelJsonSerializerContext = Prism.SyntaxGenerator.Serialization.ModelJsonSerializerContext;

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

        var publicSyntaxDir = Path.Combine(OutputPath, "public", "syntax");
        var publicGreenDir = Path.Combine(publicSyntaxDir, "green");
        var privateSyntaxDir = Path.Combine(OutputPath, "private", "syntax");
        var privateGreenDir = Path.Combine(privateSyntaxDir, "green");

        var builder = new SyntaxModelBuilder();
        var resolvedModel = builder.Build(syntax);

        using var writer = new CodeWriter();
        var emitter = new CppEmitter(resolvedModel);

        emitter.EmitSyntaxKinds(writer);
        await WriteCodeAsync(
            writer,
            Path.Join(publicSyntaxDir, "kind.ixx"),
            context.CancellationToken
        );

        emitter.EmitLexingUtils(writer);
        await WriteCodeAsync(
            writer,
            Path.Join(publicSyntaxDir, "lexing_utils.ixx"),
            context.CancellationToken
        );

        foreach (var module in resolvedModel.Modules)
        {
            var moduleName = module.Name.Underscore();
            emitter.EmitGreenNodeInterface(writer, module);
            await WriteCodeAsync(
                writer,
                Path.Join(publicGreenDir, $"{moduleName}.ixx"),
                context.CancellationToken
            );

            emitter.EmitGreenNodeImplementation(writer, module);
            await WriteCodeAsync(
                writer,
                Path.Join(privateGreenDir, $"{moduleName}.cpp"),
                context.CancellationToken
            );
        }
    }

    private static async ValueTask WriteCodeAsync(
        CodeWriter writer,
        string filePath,
        CancellationToken cancellationToken = default
    )
    {
        await using var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write);
        await writer.WriteToStreamAsync(fileStream, cancellationToken);
        writer.Clear();
    }
}
