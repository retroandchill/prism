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
    private readonly HandlebarsTemplate<object?, object?> _syntaxKindTemplate;
    private readonly HandlebarsTemplate<object?, object?> _lexerUtilsTemplate;
    private readonly HandlebarsTemplate<object?, object?> _greenInterfaceTemplate;
    private readonly HandlebarsTemplate<object?, object?> _greenImplementationTemplate;

    public Emitter()
    {
        var handlebars = Handlebars.Create();
        handlebars.Configuration.TextEncoder = null;
        handlebars.RegisterAllHelpers();
        _syntaxKindTemplate = handlebars.Compile(TemplateLoader.LoadTemplate("SyntaxKind.ixx"));
        _lexerUtilsTemplate = handlebars.Compile(TemplateLoader.LoadTemplate("LexerUtils.ixx"));
        _greenInterfaceTemplate = handlebars.Compile(TemplateLoader.LoadTemplate("GreenNode.ixx"));
        _greenImplementationTemplate = handlebars.Compile(
            TemplateLoader.LoadTemplate("GreenNode.cpp")
        );
    }

    public async Task EmitNodesAsync(
        GeneratedSyntaxModel model,
        string outputPath,
        CancellationToken cancellationToken = default
    )
    {
        var publicSyntaxDir = Path.Combine(outputPath, "public", "syntax");
        var publicGreenDir = Path.Combine(publicSyntaxDir, "green");
        var privateSyntaxDir = Path.Combine(outputPath, "private", "syntax");
        var privateGreenDir = Path.Combine(privateSyntaxDir, "green");

        await GenerateAndSave(
            Path.Combine(publicSyntaxDir, "kind.ixx"),
            model.SyntaxKinds,
            _syntaxKindTemplate,
            cancellationToken
        );
        await GenerateAndSave(
            Path.Combine(publicSyntaxDir, "lexing_utils.ixx"),
            model.SyntaxKinds,
            _lexerUtilsTemplate,
            cancellationToken
        );
        foreach (var module in model.Modules)
        {
            await GenerateAndSave(
                Path.Combine(publicGreenDir, $"{module.Name}.ixx"),
                module,
                _greenInterfaceTemplate,
                cancellationToken
            );
            await GenerateAndSave(
                Path.Combine(privateGreenDir, $"{module.Name}.cpp"),
                module,
                _greenImplementationTemplate,
                cancellationToken
            );
        }
    }

    private static async Task GenerateAndSave<TModel>(
        string filePath,
        TModel module,
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
