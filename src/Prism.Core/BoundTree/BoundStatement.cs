using System.Collections.Immutable;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.BoundTree;

internal abstract class BoundStatement(SyntaxNode syntax) : BoundNode(syntax);

internal sealed class BoundBlock(SyntaxNode syntax, ImmutableArray<BoundStatement> statements)
    : BoundStatement(syntax)
{
    public ImmutableArray<BoundStatement> Statements { get; } = statements;
}

internal sealed class BoundVariableDeclaration(
    SyntaxNode syntax,
    VariableSymbol variable,
    BoundExpression? initializer
) : BoundStatement(syntax)
{
    public VariableSymbol Variable { get; } = variable;

    public BoundExpression? Initializer { get; } = initializer;
}

internal sealed class BoundExpressionStatement(SyntaxNode syntax, BoundExpression expression)
    : BoundStatement(syntax)
{
    public BoundExpression Expression { get; } = expression;
}

internal sealed class BoundReturnStatement(SyntaxNode syntax, BoundExpression? expression)
    : BoundStatement(syntax)
{
    public BoundExpression? Expression { get; } = expression;
}

internal sealed class BoundIfStatement(
    SyntaxNode syntax,
    BoundExpression condition,
    BoundStatement thenStatement,
    BoundStatement? elseStatement
) : BoundStatement(syntax)
{
    public BoundExpression Condition { get; } = condition;

    public BoundStatement ThenStatement { get; } = thenStatement;

    public BoundStatement? ElseStatement { get; } = elseStatement;
}

internal abstract class BoundLoopBase(SyntaxNode syntax, BoundStatement loopBody, LabelSymbol label)
    : BoundStatement(syntax)
{
    public BoundStatement Body { get; } = loopBody;

    public LabelSymbol Label { get; } = label;
}

internal sealed class BoundWhileStatement(
    SyntaxNode syntax,
    BoundExpression condition,
    BoundStatement body,
    LabelSymbol label
) : BoundLoopBase(syntax, body, label)
{
    public BoundExpression Condition { get; } = condition;
}

internal sealed class BoundLoopStatement(
    SyntaxNode syntax,
    BoundStatement loopBody,
    LabelSymbol label
) : BoundLoopBase(syntax, loopBody, label);

internal sealed class BoundForStatement(
    SyntaxNode syntax,
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

internal sealed class BoundBreakStatement(SyntaxNode syntax, LabelSymbol label)
    : BoundStatement(syntax)
{
    public LabelSymbol Label { get; } = label;
}

internal sealed class BoundContinueStatement(SyntaxNode syntax, LabelSymbol label)
    : BoundStatement(syntax)
{
    public LabelSymbol Label { get; } = label;
}
