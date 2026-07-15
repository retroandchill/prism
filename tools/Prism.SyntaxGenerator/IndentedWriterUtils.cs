// @file IntentedWriterUtils.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;

namespace Prism.SyntaxGenerator;

public readonly ref struct BlockIndentScope(IndentedTextWriter writer) : IDisposable
{
    public void Dispose()
    {
        writer.Indent--;
        writer.WriteLine('}');
    }
}

public readonly ref struct UnmarkedIndentScope(IndentedTextWriter writer) : IDisposable
{
    public void Dispose()
    {
        writer.Indent--;
    }
}

public static class IndentedWriterUtils
{
    extension(IndentedTextWriter writer)
    {
        public BlockIndentScope OpenBlock()
        {
            writer.WriteLine('{');
            writer.Indent++;

            return new BlockIndentScope(writer);
        }

        public UnmarkedIndentScope OpenCaseBlock()
        {
            writer.Indent++;

            return new UnmarkedIndentScope(writer);
        }
    }
}
