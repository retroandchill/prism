// @file CppCodeWriter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using ZLinq;

namespace Prism.SyntaxGenerator.Output;

public enum CppAccessSpecifier
{
    Public,
    Protected,
    Private,
}

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

        public void WriteAccessSpecifier(CppAccessSpecifier specifier)
        {
            using var scope = writer.EnterIndentationScope(-1);
            switch (specifier)
            {
                case CppAccessSpecifier.Public:
                    writer.WriteLine("public:");
                    break;
                case CppAccessSpecifier.Protected:
                    writer.WriteLine("protected:");
                    break;
                case CppAccessSpecifier.Private:
                    writer.WriteLine("private:");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(specifier), specifier, null);
            }
        }
    }
}
