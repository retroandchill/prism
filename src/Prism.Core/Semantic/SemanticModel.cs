using Prism.Core.Binding;
using Prism.Core.BoundTree;
using Prism.Core.Compiling;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Semantic;

public class SemanticModel
{
    internal SemanticModel(Compilation compilation, SyntaxTree syntaxTree)
    {
        Compilation = compilation;
        SyntaxTree = syntaxTree;
    }

    public Compilation Compilation { get; }

    public SyntaxTree SyntaxTree { get; }

    public Symbol? GetDeclaredSymbol(SyntaxNode syntax)
    {
        ValidateIsPartOfCompilation(syntax);
        return State.GetDeclaredSymbol(syntax);
    }

    public VariableSymbol? GetDeclaredSymbol(VariableDeclarationSyntax syntax)
    {
        return GetDeclaredSymbol((SyntaxNode)syntax) as VariableSymbol;
    }

    public FunctionSymbol? GetDeclaredSymbol(FunctionDeclarationSyntax syntax)
    {
        return GetDeclaredSymbol((SyntaxNode)syntax) as FunctionSymbol;
    }

    public NamespaceSymbol? GetDeclaredSymbol(NamespaceDeclarationSyntax syntax)
    {
        return GetDeclaredSymbol((SyntaxNode)syntax) as NamespaceSymbol;
    }

    internal SemanticModelState State
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                new SemanticModelState(Compilation, SyntaxTree),
                null
            );
            return field;
        }
    }

    internal Binder GetBinder(SyntaxNode syntax)
    {
        return State.GetBinder(syntax);
    }

    internal BoundExpression GetBoundVariableInitializer(
        VariableDeclarationSyntax syntax,
        LookupContext context
    )
    {
        return State.GetBoundVariableInitializer(syntax, context);
    }

    internal BoundExpression GetBoundVariableInitializer(
        VariableDeclarationSyntax syntax,
        Binder binder,
        LookupContext context
    )
    {
        return State.GetBoundVariableInitializer(syntax, binder, context);
    }

    internal BoundStatement GetBoundFunctionBody(
        FunctionDeclarationSyntax syntax,
        LookupContext context
    )
    {
        return State.GetBoundFunctionBody(syntax, context);
    }

    private void ValidateIsPartOfCompilation(SyntaxNode node)
    {
        if (!ReferenceEquals(SyntaxTree, node.SyntaxTree))
            throw new ArgumentException("The syntax node is not part of the compilation");
    }
}
