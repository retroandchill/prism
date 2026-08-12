// @file ExportNodesCommand.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.Json;
using DotMake.CommandLine;
using Humanizer;
using Prism.SyntaxGenerator.Emitters;
using Prism.SyntaxGenerator.Mappers;
using Prism.SyntaxGenerator.Models.Resolved;
using Prism.SyntaxGenerator.Models.Spec;
using Prism.SyntaxGenerator.Output;
using Prism.SyntaxGenerator.Resolution;
using ModelJsonSerializerContext = Prism.SyntaxGenerator.Serialization.ModelJsonSerializerContext;

namespace Prism.SyntaxGenerator.Commands;

public enum OutputLanguage
{
    CSharp,
    Cpp,
}

[CliCommand(Description = "Export Prism syntax schema", Name = "generate")]
public class ExportNodesCommand
{
    [CliOption(Description = "Path to the imported nodes", Name = "input")]
    public string InputPath { get; set; } = null!;

    [CliOption(Description = "The language to output to", Name = "language")]
    public OutputLanguage Language { get; set; }

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
        switch (Language)
        {
            case OutputLanguage.CSharp:
                await EmitCSharpAsync(resolvedModel, context.CancellationToken);
                break;
            case OutputLanguage.Cpp:
                await EmitCppAsync(resolvedModel, context.CancellationToken);
                break;
            default:
                throw new InvalidOperationException("Unsupported language");
        }
    }

    private static async ValueTask WriteCodeAsync(
        CodeWriter writer,
        string filePath,
        CancellationToken cancellationToken = default
    )
    {
        var fileInfo = new FileInfo(filePath);
        fileInfo.Directory?.Create();
        await using var fileStream = fileInfo.Open(FileMode.Create, FileAccess.Write);
        await writer.WriteToStreamAsync(fileStream, cancellationToken);
        writer.Clear();
    }

    private async ValueTask EmitCppAsync(
        SyntaxModel resolvedModel,
        CancellationToken cancellationToken
    )
    {
        var publicDir = Path.Combine(OutputPath, "public");
        var publicSyntaxDir = Path.Combine(publicDir, "syntax");
        var publicGreenDir = Path.Combine(publicSyntaxDir, "green");
        var privateSyntaxDir = Path.Combine(OutputPath, "private", "syntax");
        var privateGreenDir = Path.Combine(privateSyntaxDir, "green");
        var publicDiagnosticDir = Path.Combine(publicDir, "diagnostics");

        var cppModel = resolvedModel.ToCpp();

        using var writer = new CodeWriter();
        writer.EmitSyntaxKinds(cppModel);
        await WriteCodeAsync(writer, Path.Join(publicSyntaxDir, "kind.ixx"), cancellationToken);

        writer.EmitLexingUtils(cppModel);
        await WriteCodeAsync(
            writer,
            Path.Join(publicSyntaxDir, "lexing_utils.ixx"),
            cancellationToken
        );

        foreach (var module in cppModel.Modules)
        {
            var moduleName = module.Name.Underscore();
            writer.EmitGreenNodeInterface(module);
            await WriteCodeAsync(
                writer,
                Path.Join(publicGreenDir, $"{moduleName}.ixx"),
                cancellationToken
            );

            writer.EmitGreenNodeImplementation(module);
            await WriteCodeAsync(
                writer,
                Path.Join(privateGreenDir, $"{moduleName}.cpp"),
                cancellationToken
            );

            writer.EmitRedNodeInterface(module);
            await WriteCodeAsync(
                writer,
                Path.Join(publicSyntaxDir, $"{moduleName}.ixx"),
                cancellationToken
            );

            writer.EmitRedNodeImplementation(module);
            await WriteCodeAsync(
                writer,
                Path.Join(privateSyntaxDir, $"{moduleName}.cpp"),
                cancellationToken
            );
        }

        writer.EmitGreenVisitorFunctions(cppModel);
        await WriteCodeAsync(writer, Path.Join(publicGreenDir, "visit.ixx"), cancellationToken);

        writer.EmitRedVisitorFunctions(cppModel);
        await WriteCodeAsync(writer, Path.Join(publicSyntaxDir, "visit.ixx"), cancellationToken);

        writer.EmitDiagnosticCodes(cppModel);
        await WriteCodeAsync(
            writer,
            Path.Join(publicDiagnosticDir, "codes.ixx"),
            cancellationToken
        );

        writer.EmitDiagnosticDescriptors(cppModel);
        await WriteCodeAsync(
            writer,
            Path.Join(publicDiagnosticDir, "registry.ixx"),
            cancellationToken
        );

        writer.EmitDiagnosticTraits(cppModel);
        await WriteCodeAsync(
            writer,
            Path.Join(publicDiagnosticDir, "traits.ixx"),
            cancellationToken
        );
    }

    private async ValueTask EmitCSharpAsync(
        SyntaxModel resolvedModel,
        CancellationToken cancellationToken
    )
    {
        var syntaxDir = Path.Combine(OutputPath, "Syntax");

        var csharpModel = resolvedModel.ToCSharp();
        using var writer = new CodeWriter();
        writer.EmitSyntaxKinds(csharpModel);
        await WriteCodeAsync(writer, Path.Join(syntaxDir, "SyntaxKind.cs"), cancellationToken);
    }
}
