// @file TextHelpers.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Compiler;
using Prism.Compiler.Host;

namespace Prism.Tests;

internal static class TextHelpers
{
    extension(CompilationContext)
    {
        public static CompilationContext CreateTestContext()
        {
            return CompilationContext.CreateBuilder().Build();
        }
    }
}
