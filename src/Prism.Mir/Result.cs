// @file Result.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Diagnostics;

namespace Prism.Mir;

public static class Result
{
    public static Result<T> WithPayload<T>(T? value, ImmutableArray<Diagnostic> diagnostics)
        where T : class
    {
        return new Result<T>(value, diagnostics);
    }

    public static Result<T> NoPayload<T>(ImmutableArray<Diagnostic> diagnostics)
        where T : class
    {
        return new Result<T>(null, diagnostics);
    }
}

public readonly record struct Result<T>(T? Value, ImmutableArray<Diagnostic> Diagnostics)
    where T : class
{
    public bool HasErrors => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
}
