// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;

namespace Prism.Core.Syntax.Green;

internal abstract class GreenExpression : GreenNode
{
    protected GreenExpression(SyntaxKind kind)
        : base(kind) { }
}

internal sealed class GreenLiteralExpression : GreenExpression
{
    public GreenLiteralExpression(GreenToken value)
        : base(SyntaxKind.LiteralExpression)
    {
        SlotCount = 1;
        Value = value;
        AdjustFlagsAndWidth(Value);
    }

    public GreenToken Value { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index == 0 ? Value : null;
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        return new LiteralExpressionSyntax(this, parent, position);
    }

    public GreenLiteralExpression WithValue(GreenToken value)
    {
        if (Value == value)
            return this;

        return new GreenLiteralExpression(value) { Diagnostics = Diagnostics };
    }

    public override GreenLiteralExpression WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenLiteralExpression(Value) { Diagnostics = diagnostics };
    }

    public GreenLiteralExpression Update(GreenToken value)
    {
        if (Value == value)
        {
            return this;
        }

        return new GreenLiteralExpression(value) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenIdentifierExpression : GreenExpression
{
    public GreenIdentifierExpression(GreenName value)
        : base(SyntaxKind.IdentifierExpression)
    {
        SlotCount = 1;
        Value = value;
        AdjustFlagsAndWidth(Value);
    }

    public GreenName Value { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index == 0 ? Value : null;
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        return new IdentifierExpressionSyntax(this, parent, position);
    }

    public GreenIdentifierExpression WithValue(GreenName value)
    {
        if (Value == value)
            return this;

        return new GreenIdentifierExpression(value) { Diagnostics = Diagnostics };
    }

    public override GreenIdentifierExpression WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenIdentifierExpression(Value) { Diagnostics = diagnostics };
    }

    public GreenIdentifierExpression Update(GreenName value)
    {
        if (Value == value)
        {
            return this;
        }

        return new GreenIdentifierExpression(value) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenParenthesizedExpression : GreenExpression
{
    public GreenParenthesizedExpression(
        GreenToken open,
        GreenExpression expression,
        GreenToken close
    )
        : base(SyntaxKind.ParenthesizedExpression)
    {
        SlotCount = 3;
        Open = open;
        AdjustFlagsAndWidth(Open);
        Expression = expression;
        AdjustFlagsAndWidth(Expression);
        Close = close;
        AdjustFlagsAndWidth(Close);
    }

    public GreenToken Open { get; }
    public GreenExpression Expression { get; }
    public GreenToken Close { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Open,
            1 => Expression,
            2 => Close,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        return new ParenthesizedExpressionSyntax(this, parent, position);
    }

    public GreenParenthesizedExpression WithOpen(GreenToken open)
    {
        if (Open == open)
            return this;

        return new GreenParenthesizedExpression(open, Expression, Close)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenParenthesizedExpression WithExpression(GreenExpression expression)
    {
        if (Expression == expression)
            return this;

        return new GreenParenthesizedExpression(Open, expression, Close)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenParenthesizedExpression WithClose(GreenToken close)
    {
        if (Close == close)
            return this;

        return new GreenParenthesizedExpression(Open, Expression, close)
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenParenthesizedExpression WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenParenthesizedExpression(Open, Expression, Close)
        {
            Diagnostics = diagnostics,
        };
    }

    public GreenParenthesizedExpression Update(
        GreenToken open,
        GreenExpression expression,
        GreenToken close
    )
    {
        if (Open == open && Expression == expression && Close == close)
        {
            return this;
        }

        return new GreenParenthesizedExpression(open, expression, close)
        {
            Diagnostics = Diagnostics,
        };
    }
}

internal sealed class GreenBinaryExpression : GreenExpression
{
    public GreenBinaryExpression(GreenExpression left, GreenToken op, GreenExpression right)
        : base(SyntaxKind.BinaryExpression)
    {
        SlotCount = 3;
        Left = left;
        AdjustFlagsAndWidth(Left);
        Op = op;
        AdjustFlagsAndWidth(Op);
        Right = right;
        AdjustFlagsAndWidth(Right);
    }

    public GreenExpression Left { get; }
    public GreenToken Op { get; }
    public GreenExpression Right { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Left,
            1 => Op,
            2 => Right,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        return new BinaryExpressionSyntax(this, parent, position);
    }

    public GreenBinaryExpression WithLeft(GreenExpression left)
    {
        if (Left == left)
            return this;

        return new GreenBinaryExpression(left, Op, Right) { Diagnostics = Diagnostics };
    }

    public GreenBinaryExpression WithOp(GreenToken op)
    {
        if (Op == op)
            return this;

        return new GreenBinaryExpression(Left, op, Right) { Diagnostics = Diagnostics };
    }

    public GreenBinaryExpression WithRight(GreenExpression right)
    {
        if (Right == right)
            return this;

        return new GreenBinaryExpression(Left, Op, right) { Diagnostics = Diagnostics };
    }

    public override GreenBinaryExpression WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenBinaryExpression(Left, Op, Right) { Diagnostics = diagnostics };
    }

    public GreenBinaryExpression Update(GreenExpression left, GreenToken op, GreenExpression right)
    {
        if (Left == left && Op == op && Right == right)
        {
            return this;
        }

        return new GreenBinaryExpression(left, op, right) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenAssignmentExpression : GreenExpression
{
    public GreenAssignmentExpression(GreenExpression left, GreenToken op, GreenExpression right)
        : base(SyntaxKind.AssignmentExpression)
    {
        SlotCount = 3;
        Left = left;
        AdjustFlagsAndWidth(Left);
        Op = op;
        AdjustFlagsAndWidth(Op);
        Right = right;
        AdjustFlagsAndWidth(Right);
    }

    public GreenExpression Left { get; }
    public GreenToken Op { get; }
    public GreenExpression Right { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Left,
            1 => Op,
            2 => Right,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        return new AssignmentExpressionSyntax(this, parent, position);
    }

    public GreenAssignmentExpression WithLeft(GreenExpression left)
    {
        if (Left == left)
            return this;

        return new GreenAssignmentExpression(left, Op, Right) { Diagnostics = Diagnostics };
    }

    public GreenAssignmentExpression WithOp(GreenToken op)
    {
        if (Op == op)
            return this;

        return new GreenAssignmentExpression(Left, op, Right) { Diagnostics = Diagnostics };
    }

    public GreenAssignmentExpression WithRight(GreenExpression right)
    {
        if (Right == right)
            return this;

        return new GreenAssignmentExpression(Left, Op, right) { Diagnostics = Diagnostics };
    }

    public override GreenAssignmentExpression WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenAssignmentExpression(Left, Op, Right) { Diagnostics = diagnostics };
    }

    public GreenAssignmentExpression Update(
        GreenExpression left,
        GreenToken op,
        GreenExpression right
    )
    {
        if (Left == left && Op == op && Right == right)
        {
            return this;
        }

        return new GreenAssignmentExpression(left, op, right) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenPrefixExpression : GreenExpression
{
    public GreenPrefixExpression(GreenToken op, GreenExpression operand)
        : base(SyntaxKind.PrefixExpression)
    {
        SlotCount = 2;
        Op = op;
        AdjustFlagsAndWidth(Op);
        Operand = operand;
        AdjustFlagsAndWidth(Operand);
    }

    public GreenToken Op { get; }
    public GreenExpression Operand { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Op,
            1 => Operand,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        return new PrefixExpressionSyntax(this, parent, position);
    }

    public GreenPrefixExpression WithOp(GreenToken op)
    {
        if (Op == op)
            return this;

        return new GreenPrefixExpression(op, Operand) { Diagnostics = Diagnostics };
    }

    public GreenPrefixExpression WithOperand(GreenExpression operand)
    {
        if (Operand == operand)
            return this;

        return new GreenPrefixExpression(Op, operand) { Diagnostics = Diagnostics };
    }

    public override GreenPrefixExpression WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenPrefixExpression(Op, Operand) { Diagnostics = diagnostics };
    }

    public GreenPrefixExpression Update(GreenToken op, GreenExpression operand)
    {
        if (Op == op && Operand == operand)
        {
            return this;
        }

        return new GreenPrefixExpression(op, operand) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenPostfixExpression : GreenExpression
{
    public GreenPostfixExpression(GreenExpression operand, GreenToken op)
        : base(SyntaxKind.PostfixExpression)
    {
        SlotCount = 2;
        Operand = operand;
        AdjustFlagsAndWidth(Operand);
        Op = op;
        AdjustFlagsAndWidth(Op);
    }

    public GreenExpression Operand { get; }
    public GreenToken Op { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Operand,
            1 => Op,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        return new PostfixExpressionSyntax(this, parent, position);
    }

    public GreenPostfixExpression WithOperand(GreenExpression operand)
    {
        if (Operand == operand)
            return this;

        return new GreenPostfixExpression(operand, Op) { Diagnostics = Diagnostics };
    }

    public GreenPostfixExpression WithOp(GreenToken op)
    {
        if (Op == op)
            return this;

        return new GreenPostfixExpression(Operand, op) { Diagnostics = Diagnostics };
    }

    public override GreenPostfixExpression WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenPostfixExpression(Operand, Op) { Diagnostics = diagnostics };
    }

    public GreenPostfixExpression Update(GreenExpression operand, GreenToken op)
    {
        if (Operand == operand && Op == op)
        {
            return this;
        }

        return new GreenPostfixExpression(operand, op) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenTernaryExpression : GreenExpression
{
    public GreenTernaryExpression(
        GreenExpression condition,
        GreenToken questionMark,
        GreenExpression whenTrue,
        GreenToken colon,
        GreenExpression whenFalse
    )
        : base(SyntaxKind.TernaryExpression)
    {
        SlotCount = 5;
        Condition = condition;
        AdjustFlagsAndWidth(Condition);
        QuestionMark = questionMark;
        AdjustFlagsAndWidth(QuestionMark);
        WhenTrue = whenTrue;
        AdjustFlagsAndWidth(WhenTrue);
        Colon = colon;
        AdjustFlagsAndWidth(Colon);
        WhenFalse = whenFalse;
        AdjustFlagsAndWidth(WhenFalse);
    }

    public GreenExpression Condition { get; }
    public GreenToken QuestionMark { get; }
    public GreenExpression WhenTrue { get; }
    public GreenToken Colon { get; }
    public GreenExpression WhenFalse { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Condition,
            1 => QuestionMark,
            2 => WhenTrue,
            3 => Colon,
            4 => WhenFalse,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        return new TernaryExpressionSyntax(this, parent, position);
    }

    public GreenTernaryExpression WithCondition(GreenExpression condition)
    {
        if (Condition == condition)
            return this;

        return new GreenTernaryExpression(condition, QuestionMark, WhenTrue, Colon, WhenFalse)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenTernaryExpression WithQuestionMark(GreenToken questionMark)
    {
        if (QuestionMark == questionMark)
            return this;

        return new GreenTernaryExpression(Condition, questionMark, WhenTrue, Colon, WhenFalse)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenTernaryExpression WithWhenTrue(GreenExpression whenTrue)
    {
        if (WhenTrue == whenTrue)
            return this;

        return new GreenTernaryExpression(Condition, QuestionMark, whenTrue, Colon, WhenFalse)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenTernaryExpression WithColon(GreenToken colon)
    {
        if (Colon == colon)
            return this;

        return new GreenTernaryExpression(Condition, QuestionMark, WhenTrue, colon, WhenFalse)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenTernaryExpression WithWhenFalse(GreenExpression whenFalse)
    {
        if (WhenFalse == whenFalse)
            return this;

        return new GreenTernaryExpression(Condition, QuestionMark, WhenTrue, Colon, whenFalse)
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenTernaryExpression WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenTernaryExpression(Condition, QuestionMark, WhenTrue, Colon, WhenFalse)
        {
            Diagnostics = diagnostics,
        };
    }

    public GreenTernaryExpression Update(
        GreenExpression condition,
        GreenToken questionMark,
        GreenExpression whenTrue,
        GreenToken colon,
        GreenExpression whenFalse
    )
    {
        if (
            Condition == condition
            && QuestionMark == questionMark
            && WhenTrue == whenTrue
            && Colon == colon
            && WhenFalse == whenFalse
        )
        {
            return this;
        }

        return new GreenTernaryExpression(condition, questionMark, whenTrue, colon, whenFalse)
        {
            Diagnostics = Diagnostics,
        };
    }
}

internal sealed class GreenInvocationExpression : GreenExpression
{
    public GreenInvocationExpression(GreenExpression callee, GreenArgumentList arguments)
        : base(SyntaxKind.InvocationExpression)
    {
        SlotCount = 2;
        Callee = callee;
        AdjustFlagsAndWidth(Callee);
        Arguments = arguments;
        AdjustFlagsAndWidth(Arguments);
    }

    public GreenExpression Callee { get; }
    public GreenArgumentList Arguments { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Callee,
            1 => Arguments,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        return new InvocationExpressionSyntax(this, parent, position);
    }

    public GreenInvocationExpression WithCallee(GreenExpression callee)
    {
        if (Callee == callee)
            return this;

        return new GreenInvocationExpression(callee, Arguments) { Diagnostics = Diagnostics };
    }

    public GreenInvocationExpression WithArguments(GreenArgumentList arguments)
    {
        if (Arguments == arguments)
            return this;

        return new GreenInvocationExpression(Callee, arguments) { Diagnostics = Diagnostics };
    }

    public override GreenInvocationExpression WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenInvocationExpression(Callee, Arguments) { Diagnostics = diagnostics };
    }

    public GreenInvocationExpression Update(GreenExpression callee, GreenArgumentList arguments)
    {
        if (Callee == callee && Arguments == arguments)
        {
            return this;
        }

        return new GreenInvocationExpression(callee, arguments) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenCastExpression : GreenExpression
{
    public GreenCastExpression(GreenExpression operand, GreenToken asKeyword, GreenType type)
        : base(SyntaxKind.CastExpression)
    {
        SlotCount = 3;
        Operand = operand;
        AdjustFlagsAndWidth(Operand);
        AsKeyword = asKeyword;
        AdjustFlagsAndWidth(AsKeyword);
        Type = type;
        AdjustFlagsAndWidth(Type);
    }

    public GreenExpression Operand { get; }
    public GreenToken AsKeyword { get; }
    public GreenType Type { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Operand,
            1 => AsKeyword,
            2 => Type,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        return new CastExpressionSyntax(this, parent, position);
    }

    public GreenCastExpression WithOperand(GreenExpression operand)
    {
        if (Operand == operand)
            return this;

        return new GreenCastExpression(operand, AsKeyword, Type) { Diagnostics = Diagnostics };
    }

    public GreenCastExpression WithAsKeyword(GreenToken asKeyword)
    {
        if (AsKeyword == asKeyword)
            return this;

        return new GreenCastExpression(Operand, asKeyword, Type) { Diagnostics = Diagnostics };
    }

    public GreenCastExpression WithType(GreenType type)
    {
        if (Type == type)
            return this;

        return new GreenCastExpression(Operand, AsKeyword, type) { Diagnostics = Diagnostics };
    }

    public override GreenCastExpression WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenCastExpression(Operand, AsKeyword, Type) { Diagnostics = diagnostics };
    }

    public GreenCastExpression Update(GreenExpression operand, GreenToken asKeyword, GreenType type)
    {
        if (Operand == operand && AsKeyword == asKeyword && Type == type)
        {
            return this;
        }

        return new GreenCastExpression(operand, asKeyword, type) { Diagnostics = Diagnostics };
    }
}
