// @file CodeWriter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Text;

namespace Prism.SourceGenerator;

public sealed class CodeWriter : IDisposable
{
    private const int IndentSize = 4;

    private Utf16ValueStringBuilder _builder = ZString.CreateStringBuilder();

    private int _indentLevel;
    private bool _needsIndent = true;

    private const bool TryIndent = true;
    private const bool DontIndent = false;

    public IndentationScope EnterIndentationScope(
        int indentationChange = 1,
        string? blockOpen = null,
        string? blockClose = null
    )
    {
        if (_indentLevel + indentationChange < 0)
            throw new ArgumentOutOfRangeException(nameof(indentationChange));

        if (blockOpen is not null)
            WriteLine(blockOpen);
        _indentLevel += indentationChange;
        return new IndentationScope(this, indentationChange, blockClose);
    }

    public void Write(char c)
    {
        EnsureIndent();
        WriteUnindented(c);
    }

    public void Write(string? text)
    {
        EnsureIndent();
        WriteUnindented(text);
    }

    private void Write(ReadOnlySpan<char> text)
    {
        EnsureIndent();
        WriteUnindented(text);
    }

    public void Write(ref CodeWriterInterpolatedStringHandler handler)
    {
        EnsureIndent();
        WriteUnindented(ref handler);
    }

    public void WriteLine()
    {
        _builder.AppendLine();
        _needsIndent = true;
    }

    public void WriteLine(char c)
    {
        Write(c);
        WriteLine();
    }

    public void WriteLine(string? text)
    {
        WriteLine(text.AsSpan());
    }

    public void WriteLine(ReadOnlySpan<char> text)
    {
        Write(text);
        WriteLine();
    }

    public void WriteLine(ref CodeWriterInterpolatedStringHandler handler)
    {
        Write(ref handler);
        WriteLine();
    }

    public void WriteUnindented(char c)
    {
        _builder.Append(c);
    }

    public void WriteUnindented(string? text)
    {
        WriteUnindented(text.AsSpan());
    }

    private void WriteUnindented(ReadOnlySpan<char> text)
    {
        _builder.Append(text);
    }

    public void WriteUnindented(ref CodeWriterInterpolatedStringHandler handler)
    {
        _builder.Append(handler.Span);
        handler.Dispose();
    }

    public void WriteLineUnindented(char c)
    {
        Write(c);
        WriteLine();
    }

    public void WriteLineUnindented(string? text)
    {
        WriteLineUnindented(text.AsSpan());
    }

    public void WriteLineUnindented(ReadOnlySpan<char> text)
    {
        WriteUnindented(text);
        WriteLine();
    }

    public void WriteLineUnindented(ref CodeWriterInterpolatedStringHandler handler)
    {
        Write(ref handler);
        WriteLine();
    }

    private void EnsureIndent()
    {
        if (_needsIndent && _indentLevel > 0)
        {
            _builder.Append(' ', _indentLevel * IndentSize);
        }

        _needsIndent = false;
    }

    public override string ToString()
    {
        return _builder.ToString();
    }

    public void Clear()
    {
        _builder.Clear();
    }

    public void Dispose()
    {
        _builder.Dispose();
    }

    public readonly ref struct IndentationScope : IDisposable
    {
        private readonly CodeWriter _writer;
        private readonly int _indentChange;
        private readonly string? _blockClose;

        internal IndentationScope(CodeWriter writer, int indentationChange, string? blockClose)
        {
            _writer = writer;
            _indentChange = indentationChange;
            _blockClose = blockClose;
        }

        public void Dispose()
        {
            _writer._indentLevel -= _indentChange;
            if (_blockClose is not null)
                _writer.WriteLine(_blockClose);
        }
    }
}
