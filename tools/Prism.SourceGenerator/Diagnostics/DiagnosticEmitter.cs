// @file TokenKindTemplateEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using HandlebarsDotNet;

namespace Prism.SourceGenerator.Diagnostics;

internal sealed class DiagnosticEmitter
{
    private readonly HandlebarsTemplate<object?, object?> _template;

    public DiagnosticEmitter()
    {
        var handlebars = Handlebars.Create();
        handlebars.Configuration.TextEncoder = null;
        handlebars.RegisterHelper("Joined", Helpers.Joined);
        handlebars.RegisterHelper("Equals", Helpers.Equals);
        handlebars.RegisterHelper("NotNull", Helpers.NotNull);
        _template = handlebars.Compile(TemplateLoader.LoadTemplate("Diagnostic"));
    }

    public string Emit(GeneratedDiagnostic data)
    {
        return _template(data);
    }
}
