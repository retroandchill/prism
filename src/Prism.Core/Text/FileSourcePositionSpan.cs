namespace Prism.Core.Text;

public readonly record struct FileSourcePositionSpan(string FilePath, SourcePositionSpan Span)
{
    public SourcePosition StartLinePosition => Span.Start;

    public SourcePosition EndLinePosition => Span.End;

    public FileSourcePositionSpan(string filePath, SourcePosition start, SourcePosition end)
        : this(filePath, new SourcePositionSpan(start, end)) { }

    public override string ToString()
    {
        return $"{FilePath}:{Span}";
    }
}
