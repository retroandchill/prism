// @file TextHelpers.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Compiler;
using Prism.Compiler.Host;

namespace Prism.Tests.Helpers;

internal static class TestHelpers
{
    public static readonly ImmutableArray<MetadataReference> TestReferences =
    [
        new UnresolvedReference("prism-std"),
    ];

    extension(CompilationContext)
    {
        public static CompilationContext CreateTestContext()
        {
            return CompilationContext
                .CreateBuilder()
                .UseReferenceResolver<TestReferenceResolver>()
                .UseAssemblyLoader<TestAssemblyLoader>()
                .Build();
        }
    }
}
