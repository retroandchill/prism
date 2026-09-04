// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public abstract class ExpressionSyntax : SyntaxNode
{
    private protected ExpressionSyntax(GreenExpression node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }
};

public sealed class LiteralExpressionSyntax : ExpressionSyntax
{
    internal LiteralExpressionSyntax(GreenLiteralExpression node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken Value
    {
        get { return new SyntaxToken(((GreenLiteralExpression)Green).Value, this, Position); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return null;
    }
};

public sealed class IdentifierExpressionSyntax : ExpressionSyntax
{
    internal IdentifierExpressionSyntax(
        GreenIdentifierExpression node,
        SyntaxNode? parent,
        int position
    )
        : base(node, parent, position) { }

    private NameSyntax? _value;
    public NameSyntax Value
    {
        get { return GetRed(ref _value); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 0 ? GetRed(ref _value) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 0 ? _value : null;
    }
};

public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
    internal ParenthesizedExpressionSyntax(
        GreenParenthesizedExpression node,
        SyntaxNode? parent,
        int position
    )
        : base(node, parent, position) { }

    public SyntaxToken Open
    {
        get { return new SyntaxToken(((GreenParenthesizedExpression)Green).Open, this, Position); }
    }
    private ExpressionSyntax? _expression;
    public ExpressionSyntax Expression
    {
        get { return GetRed(ref _expression, 1); }
    }
    public SyntaxToken Close
    {
        get
        {
            return new SyntaxToken(
                ((GreenParenthesizedExpression)Green).Close,
                this,
                GetSlotPosition(2)
            );
        }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 1 ? GetRed(ref _expression) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 1 ? _expression : null;
    }
};

public sealed class BinaryExpressionSyntax : ExpressionSyntax
{
    internal BinaryExpressionSyntax(GreenBinaryExpression node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    private ExpressionSyntax? _left;
    public ExpressionSyntax Left
    {
        get { return GetRed(ref _left); }
    }
    public SyntaxToken Op
    {
        get { return new SyntaxToken(((GreenBinaryExpression)Green).Op, this, GetSlotPosition(1)); }
    }
    private ExpressionSyntax? _right;
    public ExpressionSyntax Right
    {
        get { return GetRed(ref _right, 2); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index switch
        {
            0 => GetRed(ref _left),
            2 => GetRed(ref _right, 2),
            _ => null,
        };
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index switch
        {
            0 => _left,
            2 => _right,
            _ => null,
        };
    }
};

public sealed class AssignmentExpressionSyntax : ExpressionSyntax
{
    internal AssignmentExpressionSyntax(
        GreenAssignmentExpression node,
        SyntaxNode? parent,
        int position
    )
        : base(node, parent, position) { }

    private ExpressionSyntax? _left;
    public ExpressionSyntax Left
    {
        get { return GetRed(ref _left); }
    }
    public SyntaxToken Op
    {
        get
        {
            return new SyntaxToken(((GreenAssignmentExpression)Green).Op, this, GetSlotPosition(1));
        }
    }
    private ExpressionSyntax? _right;
    public ExpressionSyntax Right
    {
        get { return GetRed(ref _right, 2); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index switch
        {
            0 => GetRed(ref _left),
            2 => GetRed(ref _right, 2),
            _ => null,
        };
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index switch
        {
            0 => _left,
            2 => _right,
            _ => null,
        };
    }
};

public sealed class PrefixExpressionSyntax : ExpressionSyntax
{
    internal PrefixExpressionSyntax(GreenPrefixExpression node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken Op
    {
        get { return new SyntaxToken(((GreenPrefixExpression)Green).Op, this, Position); }
    }
    private ExpressionSyntax? _operand;
    public ExpressionSyntax Operand
    {
        get { return GetRed(ref _operand, 1); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 1 ? GetRed(ref _operand) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 1 ? _operand : null;
    }
};

public sealed class PostfixExpressionSyntax : ExpressionSyntax
{
    internal PostfixExpressionSyntax(GreenPostfixExpression node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    private ExpressionSyntax? _operand;
    public ExpressionSyntax Operand
    {
        get { return GetRed(ref _operand); }
    }
    public SyntaxToken Op
    {
        get
        {
            return new SyntaxToken(((GreenPostfixExpression)Green).Op, this, GetSlotPosition(1));
        }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 0 ? GetRed(ref _operand) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 0 ? _operand : null;
    }
};

public sealed class TernaryExpressionSyntax : ExpressionSyntax
{
    internal TernaryExpressionSyntax(GreenTernaryExpression node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    private ExpressionSyntax? _condition;
    public ExpressionSyntax Condition
    {
        get { return GetRed(ref _condition); }
    }
    public SyntaxToken QuestionMark
    {
        get
        {
            return new SyntaxToken(
                ((GreenTernaryExpression)Green).QuestionMark,
                this,
                GetSlotPosition(1)
            );
        }
    }
    private ExpressionSyntax? _whenTrue;
    public ExpressionSyntax WhenTrue
    {
        get { return GetRed(ref _whenTrue, 2); }
    }
    public SyntaxToken Colon
    {
        get
        {
            return new SyntaxToken(((GreenTernaryExpression)Green).Colon, this, GetSlotPosition(3));
        }
    }
    private ExpressionSyntax? _whenFalse;
    public ExpressionSyntax WhenFalse
    {
        get { return GetRed(ref _whenFalse, 4); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index switch
        {
            0 => GetRed(ref _condition),
            2 => GetRed(ref _whenTrue, 2),
            4 => GetRed(ref _whenFalse, 4),
            _ => null,
        };
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index switch
        {
            0 => _condition,
            2 => _whenTrue,
            4 => _whenFalse,
            _ => null,
        };
    }
};

public sealed class InvocationExpressionSyntax : ExpressionSyntax
{
    internal InvocationExpressionSyntax(
        GreenInvocationExpression node,
        SyntaxNode? parent,
        int position
    )
        : base(node, parent, position) { }

    private ExpressionSyntax? _callee;
    public ExpressionSyntax Callee
    {
        get { return GetRed(ref _callee); }
    }
    private ArgumentListSyntax? _arguments;
    public ArgumentListSyntax Arguments
    {
        get { return GetRed(ref _arguments, 1); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index switch
        {
            0 => GetRed(ref _callee),
            1 => GetRed(ref _arguments, 1),
            _ => null,
        };
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index switch
        {
            0 => _callee,
            1 => _arguments,
            _ => null,
        };
    }
};

public sealed class CastExpressionSyntax : ExpressionSyntax
{
    internal CastExpressionSyntax(GreenCastExpression node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    private ExpressionSyntax? _operand;
    public ExpressionSyntax Operand
    {
        get { return GetRed(ref _operand); }
    }
    public SyntaxToken AsKeyword
    {
        get
        {
            return new SyntaxToken(
                ((GreenCastExpression)Green).AsKeyword,
                this,
                GetSlotPosition(1)
            );
        }
    }
    private TypeSyntax? _type;
    public TypeSyntax Type
    {
        get { return GetRed(ref _type, 2); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index switch
        {
            0 => GetRed(ref _operand),
            2 => GetRed(ref _type, 2),
            _ => null,
        };
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index switch
        {
            0 => _operand,
            2 => _type,
            _ => null,
        };
    }
};

public sealed class IndexExpressionSyntax : ExpressionSyntax
{
    internal IndexExpressionSyntax(GreenIndexExpression node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    private ExpressionSyntax? _operand;
    public ExpressionSyntax Operand
    {
        get { return GetRed(ref _operand); }
    }
    public SyntaxToken OpenBracket
    {
        get
        {
            return new SyntaxToken(
                ((GreenIndexExpression)Green).OpenBracket,
                this,
                GetSlotPosition(1)
            );
        }
    }
    private ExpressionSyntax? _index;
    public ExpressionSyntax Index
    {
        get { return GetRed(ref _index, 2); }
    }
    public SyntaxToken CloseBracket
    {
        get
        {
            return new SyntaxToken(
                ((GreenIndexExpression)Green).CloseBracket,
                this,
                GetSlotPosition(3)
            );
        }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index switch
        {
            0 => GetRed(ref _operand),
            2 => GetRed(ref _index, 2),
            _ => null,
        };
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index switch
        {
            0 => _operand,
            2 => _index,
            _ => null,
        };
    }
};
