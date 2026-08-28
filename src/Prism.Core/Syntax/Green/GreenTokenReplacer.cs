namespace Prism.Core.Syntax.Green;

internal abstract partial class GreenTokenReplacer
{
    private GreenSyntaxList<T> Visit<T>(GreenSyntaxList<T> list)
        where T : GreenNode
    {
        throw new NotImplementedException();
    }

    private GreenSeparatedList<T> Visit<T>(GreenSeparatedList<T> list)
        where T : GreenNode
    {
        throw new NotImplementedException();
    }

    public abstract GreenToken Visit(GreenToken token);
    public abstract GreenTrivia Visit(GreenTrivia trivia);
    public abstract GreenListNode Visit(GreenListNode node);
}
