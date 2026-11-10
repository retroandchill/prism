using Prism.Diagnostics;
using Prism.Utils;

namespace Prism.Core.Syntax;

internal static class SyntaxLocationExtensions
{
    extension(SourceLocation)
    {
        public static SourceLocation Create(SyntaxReference reference)
        {
            return new SourceLocation(reference.SyntaxTree, reference.SourceSpan);
        }

        public static SourceLocation Create(SyntaxNode reference)
        {
            return new SourceLocation(reference.SyntaxTree, reference.FullSpan);
        }

        public static SourceLocation Create(in SyntaxToken token)
        {
            return new SourceLocation(token.SyntaxTree.RequireNonNull(), token.FullSpan);
        }
    }
}
