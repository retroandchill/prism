using System.Collections.Immutable;
using Prism.Semantic.Symbols;

namespace Prism.BoundTree;

internal closed record BoundStatement : BoundNode
{
    protected BoundStatement(IBoundSemanticContext semanticContext, IBoundSource syntax)
        : base(semanticContext, syntax) { }
}

internal sealed record BoundBlock : BoundStatement
{
    public BoundBlock(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        ImmutableArray<BoundStatement> statements
    )
        : base(semanticContext, syntax)
    {
        Statements = statements;
    }

    public ImmutableArray<BoundStatement> Statements { get; }
}

internal sealed record BoundVariableDeclaration : BoundStatement
{
    public BoundVariableDeclaration(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        VariableSymbol variable,
        BoundExpression? initializer
    )
        : base(semanticContext, syntax)
    {
        Variable = variable;
        Initializer = initializer;
    }

    public VariableSymbol Variable { get; }

    public BoundExpression? Initializer { get; }
}

internal sealed record BoundExpressionStatement : BoundStatement
{
    public BoundExpressionStatement(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        BoundExpression expression
    )
        : base(semanticContext, syntax)
    {
        Expression = expression;
    }

    public BoundExpression Expression { get; }
}

internal sealed record BoundReturnStatement : BoundStatement
{
    public BoundReturnStatement(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        BoundExpression? expression
    )
        : base(semanticContext, syntax)
    {
        Expression = expression;
    }

    public BoundExpression? Expression { get; }
}

internal sealed record BoundIfStatement : BoundStatement
{
    public BoundIfStatement(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        BoundExpression condition,
        BoundStatement thenStatement,
        BoundStatement? elseStatement
    )
        : base(semanticContext, syntax)
    {
        Condition = condition;
        ThenStatement = thenStatement;
        ElseStatement = elseStatement;
    }

    public BoundExpression Condition { get; }

    public BoundStatement ThenStatement { get; }

    public BoundStatement? ElseStatement { get; }
}

internal closed record BoundLoopBase : BoundStatement
{
    protected BoundLoopBase(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        BoundStatement loopBody,
        LabelSymbol label
    )
        : base(semanticContext, syntax)
    {
        Body = loopBody;
        Label = label;
    }

    public BoundStatement Body { get; }

    public LabelSymbol Label { get; }
}

internal sealed record BoundWhileStatement : BoundLoopBase
{
    public BoundWhileStatement(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        BoundExpression condition,
        BoundStatement body,
        LabelSymbol label
    )
        : base(semanticContext, syntax, body, label)
    {
        Condition = condition;
    }

    public BoundExpression Condition { get; }
}

internal sealed record BoundLoopStatement : BoundLoopBase
{
    public BoundLoopStatement(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        BoundStatement loopBody,
        LabelSymbol label
    )
        : base(semanticContext, syntax, loopBody, label) { }
}

internal sealed record BoundForStatement : BoundLoopBase
{
    public BoundForStatement(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        BoundVariableDeclaration? variable,
        ImmutableArray<BoundExpression> initializers,
        BoundExpression? condition,
        ImmutableArray<BoundExpression> incrementors,
        BoundStatement body,
        LabelSymbol label
    )
        : base(semanticContext, syntax, body, label)
    {
        Variable = variable;
        Initializers = initializers;
        Condition = condition;
        Incrementors = incrementors;
    }

    public BoundVariableDeclaration? Variable { get; }

    public ImmutableArray<BoundExpression> Initializers { get; }

    public BoundExpression? Condition { get; }

    public ImmutableArray<BoundExpression> Incrementors { get; }
}

internal sealed record BoundBreakStatement : BoundStatement
{
    public BoundBreakStatement(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        LabelSymbol label
    )
        : base(semanticContext, syntax)
    {
        Label = label;
    }

    public LabelSymbol Label { get; }
}

internal sealed record BoundContinueStatement : BoundStatement
{
    public BoundContinueStatement(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        LabelSymbol label
    )
        : base(semanticContext, syntax)
    {
        Label = label;
    }

    public LabelSymbol Label { get; }
}
