// @file Emitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using HandlebarsDotNet;
using Prism.SyntaxGenerator.Models;

namespace Prism.SyntaxGenerator;

public sealed class Emitter
{
    private HandlebarsTemplate<object?, object?> GreenInterfaceTemplate { get; }
    private HandlebarsTemplate<object?, object?> GreenImplementationTemplate { get; }

    public Emitter()
    {
        var handlebars = Handlebars.Create();
        handlebars.Configuration.TextEncoder = null;
        handlebars.RegisterAllHelpers();
        GreenInterfaceTemplate = handlebars.Compile(TemplateLoader.LoadTemplate("GreenNode.ixx"));
        GreenImplementationTemplate = handlebars.Compile(
            TemplateLoader.LoadTemplate("GreenNode.cpp")
        );
    }

    public async Task EmitNodesAsync(
        ImmutableArray<GeneratedModule> modules,
        string outputPath,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var module in modules)
        {
            await EmitNode(module, outputPath, cancellationToken);
        }
    }

    private async Task EmitNode(
        GeneratedModule module,
        string outputPath,
        CancellationToken cancellationToken = default
    )
    {
        var publicSyntaxDir = Path.Combine(outputPath, "public", "syntax");
        var publicGreenDir = Path.Combine(publicSyntaxDir, "green");
        var privateSyntaxDir = Path.Combine(outputPath, "private", "syntax");
        var privateGreenDir = Path.Combine(privateSyntaxDir, "green");

        await GenerateAndSave(
            Path.Combine(publicGreenDir, $"{module.Name}.ixx"),
            module,
            GreenInterfaceTemplate,
            cancellationToken
        );
        await GenerateAndSave(
            Path.Combine(privateGreenDir, $"{module.Name}.cpp"),
            module,
            GreenImplementationTemplate,
            cancellationToken
        );
    }

    private static async Task GenerateAndSave(
        string filePath,
        GeneratedModule module,
        HandlebarsTemplate<object?, object?> template,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write);
        await using var writer = new StreamWriter(fileStream);
        var content = template(module);
        await writer.WriteAsync(content);
    }
}
