// @file ModelJsonSerializerContext.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Prism.SyntaxGenerator.Models.Spec;

namespace Prism.SyntaxGenerator.Serialization;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
[JsonSerializable(typeof(SyntaxSpecification))]
internal partial class ModelJsonSerializerContext : JsonSerializerContext
{
    public static JsonSerializerOptions CompositeOptions { get; }

    static ModelJsonSerializerContext()
    {
        CompositeOptions = new JsonSerializerOptions(JsonSerializerOptions.Default)
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        CompositeOptions.TypeInfoResolverChain.Insert(0, Default);

        CompositeOptions.MakeReadOnly();
    }
}
