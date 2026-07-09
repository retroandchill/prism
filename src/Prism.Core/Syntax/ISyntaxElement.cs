using Prism.Core.Parse;

namespace Prism.Core.Syntax;

public interface ISyntaxElement
{
    SyntaxKind Kind { get; }

    TextSpan FullSpan { get; }

    TextSpan Span { get; }

    bool ContainsDiagnostics { get; }
}
