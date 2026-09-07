using System.Collections.Immutable;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.BoundTree;

internal abstract record BoundStatement : BoundNode
{
    protected BoundStatement(SyntaxNode syntax)
        : base(syntax) { }
}

internal sealed record BoundBlock : BoundStatement
{
    public BoundBlock(SyntaxNode syntax, ImmutableArray<BoundStatement> statements)
        : base(syntax)
    {
        Statements = statements;
    }

    public ImmutableArray<BoundStatement> Statements { get; }
}

internal sealed record BoundVariableDeclaration : BoundStatement
{
    public BoundVariableDeclaration(
        SyntaxNode syntax,
        VariableSymbol variable,
        BoundExpression? initializer
    )
        : base(syntax)
    {
        Variable = variable;
        Initializer = initializer;
    }

    public VariableSymbol Variable { get; }

    public BoundExpression? Initializer { get; }
}

internal sealed record BoundExpressionStatement : BoundStatement
{
    public BoundExpressionStatement(SyntaxNode syntax, BoundExpression expression)
        : base(syntax)
    {
        Expression = expression;
    }

    public BoundExpression Expression { get; }
}

internal sealed record BoundReturnStatement : BoundStatement
{
    public BoundReturnStatement(SyntaxNode syntax, BoundExpression? expression)
        : base(syntax)
    {
        Expression = expression;
    }

    public BoundExpression? Expression { get; }
}

internal sealed record BoundIfStatement : BoundStatement
{
    public BoundIfStatement(
        SyntaxNode syntax,
        BoundExpression condition,
        BoundStatement thenStatement,
        BoundStatement? elseStatement
    )
        : base(syntax)
    {
        Condition = condition;
        ThenStatement = thenStatement;
        ElseStatement = elseStatement;
    }

    public BoundExpression Condition { get; }

    public BoundStatement ThenStatement { get; }

    public BoundStatement? ElseStatement { get; }
}

internal abstract record BoundLoopBase : BoundStatement
{
    protected BoundLoopBase(SyntaxNode syntax, BoundStatement loopBody, LabelSymbol label)
        : base(syntax)
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
        SyntaxNode syntax,
        BoundExpression condition,
        BoundStatement body,
        LabelSymbol label
    )
        : base(syntax, body, label)
    {
        Condition = condition;
    }

    public BoundExpression Condition { get; }
}

internal sealed record BoundLoopStatement : BoundLoopBase
{
    public BoundLoopStatement(SyntaxNode syntax, BoundStatement loopBody, LabelSymbol label)
        : base(syntax, loopBody, label) { }
}

internal sealed record BoundForStatement : BoundLoopBase
{
    public BoundForStatement(
        SyntaxNode syntax,
        BoundVariableDeclaration? variable,
        ImmutableArray<BoundExpression> initializers,
        BoundExpression? condition,
        ImmutableArray<BoundExpression> incrementors,
        BoundStatement body,
        LabelSymbol label
    )
        : base(syntax, body, label)
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
    public BoundBreakStatement(SyntaxNode syntax, LabelSymbol label)
        : base(syntax)
    {
        Label = label;
    }

    public LabelSymbol Label { get; }
}

internal sealed record BoundContinueStatement : BoundStatement
{
    public BoundContinueStatement(SyntaxNode syntax, LabelSymbol label)
        : base(syntax)
    {
        Label = label;
    }

    public LabelSymbol Label { get; }
}
