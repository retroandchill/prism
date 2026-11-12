using Prism.Text;

namespace Prism;

public interface ISourceDocument
{
    string? Path { get; }

    FileSourcePositionSpan GetPositionSpan(TextSpan span);

    ReadOnlySpan<char> GetText(TextSpan span);
}
