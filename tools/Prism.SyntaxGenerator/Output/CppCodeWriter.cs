// @file CppCodeWriter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.SyntaxGenerator.Output;

public static class CppCodeWriter
{
    extension(CodeWriter writer)
    {
        public CodeWriter.IndentationScope EnterBlockScope(bool needsSemicolon = false)
        {
            return writer.EnterIndentationScope(
                blockOpen: "{",
                blockClose: needsSemicolon ? "};" : "}"
            );
        }

        public CodeWriter.IndentationScope EnterNamespaceScope(ReadOnlySpan<char> name)
        {
            writer.WriteLine($"namespace {name}");
            return writer.EnterBlockScope();
        }
    }
}
