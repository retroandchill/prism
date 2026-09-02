using System.Collections.Immutable;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.BoundTree;

internal abstract class BoundStatement : BoundNode
{
    protected BoundStatement(StatementSyntax syntax)
        : base(syntax) { }

    protected BoundStatement(ExpressionBodySyntax syntax)
        : base(syntax) { }
}

internal sealed class BoundBlock(BlockSyntax syntax, ImmutableArray<BoundStatement> statements)
    : BoundStatement(syntax)
{
    public ImmutableArray<BoundStatement> Statements { get; } = statements;
}

internal sealed class BoundVariableDeclaration(
    VariableDeclarationStatementSyntax syntax,
    VariableSymbol variable,
    BoundExpression? initializer
) : BoundStatement(syntax)
{
    public VariableSymbol Variable { get; } = variable;

    public BoundExpression? Initializer { get; } = initializer;
}

internal sealed class BoundExpressionStatement : BoundStatement
{
    public BoundExpressionStatement(ExpressionStatementSyntax syntax, BoundExpression expression)
        : base(syntax)
    {
        Expression = expression;
    }

    public BoundExpressionStatement(ExpressionBodySyntax syntax, BoundExpression expression)
        : base(syntax)
    {
        Expression = expression;
    }

    public BoundExpression Expression { get; }
}

internal sealed class BoundReturnStatement : BoundStatement
{
    public BoundReturnStatement(ReturnStatementSyntax syntax, BoundExpression? expression)
        : base(syntax)
    {
        Expression = expression;
    }

    public BoundReturnStatement(ExpressionBodySyntax syntax, BoundExpression expression)
        : base(syntax)
    {
        Expression = expression;
    }

    public BoundExpression? Expression { get; }
}

internal sealed class BoundIfStatement(
    IfStatementSyntax syntax,
    BoundExpression condition,
    BoundStatement thenStatement,
    BoundStatement? elseStatement
) : BoundStatement(syntax)
{
    public BoundExpression Condition { get; } = condition;

    public BoundStatement ThenStatement { get; } = thenStatement;

    public BoundStatement? ElseStatement { get; } = elseStatement;
}

internal abstract class BoundLoopBase(
    StatementSyntax syntax,
    BoundStatement loopBody,
    LabelSymbol label
) : BoundStatement(syntax)
{
    public BoundStatement Body { get; } = loopBody;

    public LabelSymbol Label { get; } = label;
}

internal sealed class BoundWhileStatement(
    WhileStatementSyntax syntax,
    BoundExpression condition,
    BoundStatement body,
    LabelSymbol label
) : BoundLoopBase(syntax, body, label)
{
    public BoundExpression Condition { get; } = condition;
}

internal sealed class BoundLoopStatement(
    LoopStatementSyntax syntax,
    BoundStatement loopBody,
    LabelSymbol label
) : BoundLoopBase(syntax, loopBody, label);

internal sealed class BoundForStatement(
    ForStatementSyntax syntax,
    BoundVariableDeclaration? variable,
    ImmutableArray<BoundExpression> initializers,
    BoundExpression? condition,
    ImmutableArray<BoundExpression> incrementors,
    BoundStatement body,
    LabelSymbol label
) : BoundLoopBase(syntax, body, label)
{
    public BoundVariableDeclaration? Variable { get; } = variable;

    public ImmutableArray<BoundExpression> Initializers { get; } = initializers;

    public BoundExpression? Condition { get; } = condition;

    public ImmutableArray<BoundExpression> Incrementors { get; } = incrementors;
}

internal sealed class BoundBreakStatement(BreakStatementSyntax syntax, LabelSymbol label)
    : BoundStatement(syntax)
{
    public LabelSymbol Label { get; } = label;
}

internal sealed class BoundContinueStatement(ContinueStatementSyntax syntax, LabelSymbol label)
    : BoundStatement(syntax)
{
    public LabelSymbol Label { get; } = label;
}
