// @file CppPropertyInfo.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Humanizer;
using Prism.SyntaxGenerator.Models.Resolved;

namespace Prism.SyntaxGenerator.Models.Cpp;

public sealed class CppPropertyInfo
{
    public string GetterName { get; }
    public string FieldName { get; }
    public string ParameterName { get; }

    public CppPropertyInfo(SyntaxProperty property)
    {
        GetterName = property.Name.Underscore();
        FieldName = $"{GetterName}_";
        ParameterName = GetterName;
    }

    private static string GetGreenName(string? baseName)
    {
        return baseName is not null ? $"Green{baseName}" : "GreenNode";
    }
}
