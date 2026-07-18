// @file CaseExtensions.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using CaseConverter;

namespace Prism.SyntaxGenerator;

public static class CaseExtensions
{
    public static string ToDisplayCase(this string str)
    {
        return str.ToPascalCase().SplitCamelCase().FirstCharToUpperCase();
    }
}
