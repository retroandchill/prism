// @file Helpers.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using CaseConverter;
using HandlebarsDotNet;

namespace Prism.SyntaxGenerator;

internal static class Helpers
{
    private static void Joined(
        EncodedTextWriter output,
        BlockHelperOptions options,
        Context context,
        Arguments arguments
    )
    {
        if (arguments.Length != 2)
        {
            throw new HandlebarsException("Joined helper requires exactly two argument");
        }

        var delimiter = arguments.At<string>(0);
        var items = arguments.At<IEnumerable>(1);

        var i = 0;
        foreach (var item in items)
        {
            if (i > 0)
                output.Write(delimiter);

            options.Template(output, item);
            i++;
        }
    }

    private static void Equals(
        EncodedTextWriter output,
        BlockHelperOptions options,
        Context context,
        Arguments arguments
    )
    {
        if (arguments.Length != 2)
        {
            throw new HandlebarsException("Equals helper requires exactly two arguments");
        }

        var left = arguments.At<int>(0);
        var right = arguments.At<int>(1);

        if (left == right)
        {
            options.Template(output, context);
        }
        else
        {
            options.Inverse(output, context);
        }
    }

    private static void NotNull(
        EncodedTextWriter output,
        BlockHelperOptions options,
        Context context,
        Arguments arguments
    )
    {
        if (arguments.Length != 1)
        {
            throw new HandlebarsException("NotNull helper requires exactly one argument");
        }

        var left = arguments.At<object?>(0);

        if (left is not null)
        {
            options.Template(output, context);
        }
        else
        {
            options.Inverse(output, context);
        }
    }

    private static void Indexed(
        EncodedTextWriter output,
        BlockHelperOptions options,
        Context context,
        Arguments arguments
    )
    {
        if (arguments.Length != 1)
        {
            throw new HandlebarsException("Indexed helper requires exactly one argument");
        }

        var items = arguments.At<IEnumerable>(0);

        var i = 0;
        foreach (var item in items)
        {
            options.Data.CreateProperty("Index", i, out _);
            options.Template(output, item);
            i++;
        }
    }

    public static void RegisterAllHelpers(this IHandlebars handlebars)
    {
        handlebars.RegisterHelper("Joined", Joined);
        handlebars.RegisterHelper("Equals", Equals);
        handlebars.RegisterHelper("NotNull", NotNull);
        handlebars.RegisterHelper("Indexed", Indexed);
    }
}
