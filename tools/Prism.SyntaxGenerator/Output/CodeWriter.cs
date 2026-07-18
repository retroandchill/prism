// @file CodeWriter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using Cysharp.Text;
using Utf8StringInterpolation;

namespace Prism.SyntaxGenerator.Output;

public sealed class CodeWriter : IDisposable
{
    private const int IndentSize = 4;

    private Utf8ValueStringBuilder _builder = ZString.CreateUtf8StringBuilder();

    private int _indentLevel;
    private bool _needsIndent = true;

    public IndentationScope EnterIndentationScope(
        int indentationChange = 1,
        string? blockOpen = null,
        string? blockClose = null
    )
    {
        ArgumentOutOfRangeException.ThrowIfNegative(_indentLevel + indentationChange);
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

    public void Write(ref Utf8StringWriter<ArrayBufferWriter<byte>> handler)
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

    public void WriteLine(ref Utf8StringWriter<ArrayBufferWriter<byte>> handler)
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

    public void WriteUnindented(ref Utf8StringWriter<ArrayBufferWriter<byte>> handler)
    {
        handler.Flush();
        _builder.AppendLiteral(handler.GetBufferWriter().WrittenSpan);
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

    public void WriteLineUnindented(ref Utf8StringWriter<ArrayBufferWriter<byte>> handler)
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

    public void WriteToStream(Stream stream)
    {
        stream.Write(_builder.AsSpan());
    }

    public ValueTask WriteToStreamAsync(
        Stream stream,
        CancellationToken cancellationToken = default
    )
    {
        return stream.WriteAsync(_builder.AsMemory(), cancellationToken);
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
