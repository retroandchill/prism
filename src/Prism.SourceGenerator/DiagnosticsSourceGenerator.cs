using System;
using System.IO;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Prism.SourceGenerator.Emitters;
using Prism.SourceGenerator.Mappers;
using Prism.SourceGenerator.Models.Spec;
using Prism.SourceGenerator.Resolution;

namespace Prism.SourceGenerator;

[Generator]
public class DiagnosticsSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var textFiles = context.AdditionalTextsProvider.Where(static file =>
            Path.GetFileName(file.Path) == "diagnostics.json"
        );

        var fileContents = textFiles.Select(static (text, ctx) => text.GetText(ctx)!.ToString());

        context.RegisterSourceOutput(fileContents, Execute);
    }

    private static void Execute(SourceProductionContext context, string content)
    {
        var options = new JsonSerializerOptions()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        var deserialized = JsonSerializer.Deserialize<DiagnosticsSpecification>(content, options);
        var syntax =
            deserialized
            ?? throw new InvalidOperationException("Failed to deserialize syntax specification");
        var builder = new DiagnosticModelBuilder();
        var resolvedModel = builder.Build(syntax);
        var csharpModel = resolvedModel.ToCSharp();
        using var writer = new CodeWriter();

        writer.EmitDiagnosticCodes(csharpModel);
        WriteCode(writer, "DiagnosticCode.g.cs", context);

        writer.EmitDiagnosticDescriptors(csharpModel);
        WriteCode(writer, "DiagnosticDescriptors.g.cs", context);

        writer.EmitDiagnosticFactories(csharpModel);
        WriteCode(writer, "DiagnosticExtensions.g.cs", context);
    }

    private static void WriteCode(CodeWriter writer, string name, SourceProductionContext context)
    {
        context.AddSource(name, writer.ToString());
        writer.Clear();
    }
}
