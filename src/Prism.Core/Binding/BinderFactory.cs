using Prism.Core.Compiling;
using Prism.Core.Syntax;

namespace Prism.Core.Binding;

internal sealed class BinderFactory
{
    private readonly Compilation _compilation;

    public BinderFactory(Compilation compilation, SyntaxTree syntaxTree)
    {
        _compilation = compilation;
        SyntaxTree = syntaxTree;
    }

    public SyntaxTree SyntaxTree { get; }

    public Binder GetBinder(SyntaxNode node)
    {
        throw new NotImplementedException();
    }
}
