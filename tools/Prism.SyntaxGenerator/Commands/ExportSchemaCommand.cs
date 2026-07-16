// @file ExportSchemaCommand.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Schema;
using DotMake.CommandLine;
using Prism.SyntaxGenerator.Models;
using Prism.SyntaxGenerator.Models.Spec;
using ModelJsonSerializerContext = Prism.SyntaxGenerator.Serialization.ModelJsonSerializerContext;

namespace Prism.SyntaxGenerator.Commands;

[CliCommand(Description = "Export Prism syntax schema", Name = "schema")]
public class ExportSchemaCommand
{
    [CliOption(Description = "Path to export schema to", Name = "export", Required = false)]
    public string? ExportPath { get; set; }

    public async Task RunAsync(CliContext context)
    {
        var options = ModelJsonSerializerContext.CompositeOptions;
        var schema = options.GetJsonSchemaAsNode(typeof(SyntaxSpecification));
        if (ExportPath is not null)
        {
            await using var file = File.Create(ExportPath);
            await JsonSerializer.SerializeAsync(file, schema, options);
        }
        else
        {
            await context.Output.WriteLineAsync(schema.ToJsonString(options));
        }
    }
}
