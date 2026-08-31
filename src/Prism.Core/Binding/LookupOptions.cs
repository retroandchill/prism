// @file LookupOptions.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Cysharp.Text;
using ZLinq;

namespace Prism.Core.Binding;

[Flags]
internal enum LookupOptions : uint
{
    None = 0,
    Namespace = 1 << 0,
    Type = 1 << 1,
    Value = 1 << 2,
    Callable = 1 << 3,

    All = Namespace | Type | Value | Callable,
    NamespaceOrType = Namespace | Type,
    ValueOrCallable = Value | Callable,
}

internal static class LookupOptionsExtensions
{
    public static string ToDisplayString(this LookupOptions options)
    {
        var result = new List<string>();
        if (options.HasFlag(LookupOptions.Namespace))
            result.Add("namespace");
        if (options.HasFlag(LookupOptions.Type))
            result.Add("type");
        if (options.HasFlag(LookupOptions.Value))
            result.Add("value");
        if (options.HasFlag(LookupOptions.Callable))
            result.Add("callable");

        switch (result.Count)
        {
            case 0:
                return "none";
            case 1:
                return result[0];
            case 2:
                return $"{result[0]} or {result[2]}";
        }

        using var builder = ZString.CreateStringBuilder();
        foreach (var (i, str) in result.AsValueEnumerable().Index())
        {
            if (i > 0)
            {
                builder.Append(i == result.Count - 1 ? ", or " : ", ");
            }

            builder.Append(str);
        }

        return builder.ToString();
    }
}
