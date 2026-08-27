// @file TemplateLoader.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reflection;

namespace Prism.SourceGenerator;

internal class TemplateLoader
{
    private const string TemplateNamespace = "Prism.SourceGenerator.Templates";
    private static readonly ConcurrentDictionary<string, string> Templates = new();

    public static string LoadTemplate(string name)
    {
        return Templates.GetOrAdd(
            name,
            static n =>
            {
                var resourceName = $"{TemplateNamespace}.{n}.mustache";
                var asm = Assembly.GetExecutingAssembly();
                using var stream =
                    asm.GetManifestResourceStream(resourceName)
                    ?? throw new InvalidOperationException($"Resource '{resourceName}' not found.");

                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        );
    }
}
