using System;
using System.Runtime.CompilerServices;
using Cysharp.Text;

namespace Prism.SourceGenerator;

[InterpolatedStringHandler]
public ref struct CodeWriterInterpolatedStringHandler : IDisposable
{
    private Utf16ValueStringBuilder _builder;

    public ReadOnlySpan<char> Span => _builder.AsSpan();

    public CodeWriterInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        _builder = ZString.CreateStringBuilder(true);
    }

    public void AppendLiteral(string s)
    {
        _builder.Append(s);
    }

    public void AppendFormatted<T>(T t)
    {
        _builder.Append(t);
    }

    public void AppendFormatted(ReadOnlySpan<char> t)
    {
        _builder.Append(t);
    }

    public void AppendFormatted<T>(T t, string format)
        where T : IFormattable
    {
        _builder.Append(t.ToString(format, null));
    }

    public void Dispose()
    {
        _builder.Dispose();
    }
}
