// @file TokenKindTemplateEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using HandlebarsDotNet;

namespace Prism.SourceGenerator.Tokens;

internal sealed class TokenKindEmitter
{
    private readonly HandlebarsTemplate<object?, object?> _template;

    public TokenKindEmitter()
    {
        var handlebars = Handlebars.Create();
        handlebars.Configuration.TextEncoder = null;
        _template = handlebars.Compile(TemplateLoader.LoadTemplate("TokenKind"));
    }

    public string Emit(TokenKindData data)
    {
        return _template(data);
    }
}
