using System.Collections.Immutable;
using Prism.Core.Compiling;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.BoundTree;

internal closed record BoundStatement : BoundNode
{
    protected BoundStatement(Compilation compilation, SyntaxNode syntax)
        : base(compilation, syntax) { }
}

internal sealed record BoundBlock : BoundStatement
{
    public BoundBlock(
        Compilation compilation,
        SyntaxNode syntax,
        ImmutableArray<BoundStatement> statements
    )
        : base(compilation, syntax)
    {
        Statements = statements;
    }

    public ImmutableArray<BoundStatement> Statements { get; }
}

internal sealed record BoundVariableDeclaration : BoundStatement
{
    public BoundVariableDeclaration(
        Compilation compilation,
        SyntaxNode syntax,
        VariableSymbol variable,
        BoundExpression? initializer
    )
        : base(compilation, syntax)
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
        Compilation compilation,
        SyntaxNode syntax,
        BoundExpression expression
    )
        : base(compilation, syntax)
    {
        Expression = expression;
    }

    public BoundExpression Expression { get; }
}

internal sealed record BoundReturnStatement : BoundStatement
{
    public BoundReturnStatement(
        Compilation compilation,
        SyntaxNode syntax,
        BoundExpression? expression
    )
        : base(compilation, syntax)
    {
        Expression = expression;
    }

    public BoundExpression? Expression { get; }
}

internal sealed record BoundIfStatement : BoundStatement
{
    public BoundIfStatement(
        Compilation compilation,
        SyntaxNode syntax,
        BoundExpression condition,
        BoundStatement thenStatement,
        BoundStatement? elseStatement
    )
        : base(compilation, syntax)
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
        Compilation compilation,
        SyntaxNode syntax,
        BoundStatement loopBody,
        LabelSymbol label
    )
        : base(compilation, syntax)
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
        Compilation compilation,
        SyntaxNode syntax,
        BoundExpression condition,
        BoundStatement body,
        LabelSymbol label
    )
        : base(compilation, syntax, body, label)
    {
        Condition = condition;
    }

    public BoundExpression Condition { get; }
}

internal sealed record BoundLoopStatement : BoundLoopBase
{
    public BoundLoopStatement(
        Compilation compilation,
        SyntaxNode syntax,
        BoundStatement loopBody,
        LabelSymbol label
    )
        : base(compilation, syntax, loopBody, label) { }
}

internal sealed record BoundForStatement : BoundLoopBase
{
    public BoundForStatement(
        Compilation compilation,
        SyntaxNode syntax,
        BoundVariableDeclaration? variable,
        ImmutableArray<BoundExpression> initializers,
        BoundExpression? condition,
        ImmutableArray<BoundExpression> incrementors,
        BoundStatement body,
        LabelSymbol label
    )
        : base(compilation, syntax, body, label)
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
    public BoundBreakStatement(Compilation compilation, SyntaxNode syntax, LabelSymbol label)
        : base(compilation, syntax)
    {
        Label = label;
    }

    public LabelSymbol Label { get; }
}

internal sealed record BoundContinueStatement : BoundStatement
{
    public BoundContinueStatement(Compilation compilation, SyntaxNode syntax, LabelSymbol label)
        : base(compilation, syntax)
    {
        Label = label;
    }

    public LabelSymbol Label { get; }
}
