// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public abstract class StatementSyntax : SyntaxNode
{
    private protected StatementSyntax(GreenStatement node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }
};

public sealed class EmptyStatementSyntax : StatementSyntax
{
    internal EmptyStatementSyntax(GreenEmptyStatement node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken Semicolon
    {
        get { return new SyntaxToken(((GreenEmptyStatement)Green).Semicolon, this, Position); }
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

public sealed class VariableDeclarationStatementSyntax : StatementSyntax
{
    internal VariableDeclarationStatementSyntax(
        GreenVariableDeclarationStatement node,
        SyntaxNode? parent,
        int position
    )
        : base(node, parent, position) { }

    private VariableDeclarationSyntax? _declaration;
    public VariableDeclarationSyntax Declaration
    {
        get { return GetRed(ref _declaration); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 0 ? GetRed(ref _declaration) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 0 ? _declaration : null;
    }
};

public sealed class BlockSyntax : StatementSyntax
{
    internal BlockSyntax(GreenBlock node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken OpenBrace
    {
        get { return new SyntaxToken(((GreenBlock)Green).OpenBrace, this, Position); }
    }
    private SyntaxNode? _statements;
    public SyntaxList<StatementSyntax> Statements
    {
        get
        {
            var red = GetRed(ref _statements, 1);
            return new SyntaxList<StatementSyntax>(red);
        }
    }
    public SyntaxToken CloseBrace
    {
        get { return new SyntaxToken(((GreenBlock)Green).CloseBrace, this, GetSlotPosition(2)); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 1 ? GetRed(ref _statements) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 1 ? _statements : null;
    }
};

public sealed class ReturnStatementSyntax : StatementSyntax
{
    internal ReturnStatementSyntax(GreenReturnStatement node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken ReturnKeyword
    {
        get { return new SyntaxToken(((GreenReturnStatement)Green).ReturnKeyword, this, Position); }
    }
    private ExpressionSyntax? _expression;
    public ExpressionSyntax? Expression
    {
        get { return GetRed(ref _expression, 1); }
    }
    public SyntaxToken Semicolon
    {
        get
        {
            return new SyntaxToken(
                ((GreenReturnStatement)Green).Semicolon,
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

public sealed class ExpressionStatementSyntax : StatementSyntax
{
    internal ExpressionStatementSyntax(
        GreenExpressionStatement node,
        SyntaxNode? parent,
        int position
    )
        : base(node, parent, position) { }

    private ExpressionSyntax? _expression;
    public ExpressionSyntax Expression
    {
        get { return GetRed(ref _expression); }
    }
    public SyntaxToken Semicolon
    {
        get
        {
            return new SyntaxToken(
                ((GreenExpressionStatement)Green).Semicolon,
                this,
                GetSlotPosition(1)
            );
        }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 0 ? GetRed(ref _expression) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 0 ? _expression : null;
    }
};

public sealed class IfStatementSyntax : StatementSyntax
{
    internal IfStatementSyntax(GreenIfStatement node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken IfKeyword
    {
        get { return new SyntaxToken(((GreenIfStatement)Green).IfKeyword, this, Position); }
    }
    public SyntaxToken OpenParen
    {
        get
        {
            return new SyntaxToken(((GreenIfStatement)Green).OpenParen, this, GetSlotPosition(1));
        }
    }
    private ExpressionSyntax? _condition;
    public ExpressionSyntax Condition
    {
        get { return GetRed(ref _condition, 2); }
    }
    public SyntaxToken CloseParen
    {
        get
        {
            return new SyntaxToken(((GreenIfStatement)Green).CloseParen, this, GetSlotPosition(3));
        }
    }
    private BlockSyntax? _block;
    public BlockSyntax Block
    {
        get { return GetRed(ref _block, 4); }
    }
    private ElseClauseSyntax? _elseClause;
    public ElseClauseSyntax? ElseClause
    {
        get { return GetRed(ref _elseClause, 5); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index switch
        {
            2 => GetRed(ref _condition, 2),
            4 => GetRed(ref _block, 4),
            5 => GetRed(ref _elseClause, 5),
            _ => null,
        };
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index switch
        {
            2 => _condition,
            4 => _block,
            5 => _elseClause,
            _ => null,
        };
    }
};

public sealed class WhileStatementSyntax : StatementSyntax
{
    internal WhileStatementSyntax(GreenWhileStatement node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken WhileKeyword
    {
        get { return new SyntaxToken(((GreenWhileStatement)Green).WhileKeyword, this, Position); }
    }
    public SyntaxToken OpenParen
    {
        get
        {
            return new SyntaxToken(
                ((GreenWhileStatement)Green).OpenParen,
                this,
                GetSlotPosition(1)
            );
        }
    }
    private ExpressionSyntax? _condition;
    public ExpressionSyntax Condition
    {
        get { return GetRed(ref _condition, 2); }
    }
    public SyntaxToken CloseParen
    {
        get
        {
            return new SyntaxToken(
                ((GreenWhileStatement)Green).CloseParen,
                this,
                GetSlotPosition(3)
            );
        }
    }
    private BlockSyntax? _block;
    public BlockSyntax Block
    {
        get { return GetRed(ref _block, 4); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index switch
        {
            2 => GetRed(ref _condition, 2),
            4 => GetRed(ref _block, 4),
            _ => null,
        };
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index switch
        {
            2 => _condition,
            4 => _block,
            _ => null,
        };
    }
};

public sealed class LoopStatementSyntax : StatementSyntax
{
    internal LoopStatementSyntax(GreenLoopStatement node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken LoopKeyword
    {
        get { return new SyntaxToken(((GreenLoopStatement)Green).LoopKeyword, this, Position); }
    }
    private BlockSyntax? _block;
    public BlockSyntax Block
    {
        get { return GetRed(ref _block, 1); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 1 ? GetRed(ref _block) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 1 ? _block : null;
    }
};

public sealed class ForStatementSyntax : StatementSyntax
{
    internal ForStatementSyntax(GreenForStatement node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken ForKeyword
    {
        get { return new SyntaxToken(((GreenForStatement)Green).ForKeyword, this, Position); }
    }
    public SyntaxToken OpenParen
    {
        get
        {
            return new SyntaxToken(((GreenForStatement)Green).OpenParen, this, GetSlotPosition(1));
        }
    }
    private VariableDeclarationStatementSyntax? _declaration;
    public VariableDeclarationStatementSyntax? Declaration
    {
        get { return GetRed(ref _declaration, 2); }
    }
    private SyntaxNode? _initializers;
    public SeparatedSyntaxList<ExpressionSyntax> Initializers
    {
        get
        {
            var red = GetRed(ref _initializers, 3);
            return new SeparatedSyntaxList<ExpressionSyntax>(red);
        }
    }
    public SyntaxToken FirstSemicolon
    {
        get
        {
            return new SyntaxToken(
                ((GreenForStatement)Green).FirstSemicolon,
                this,
                GetSlotPosition(4)
            );
        }
    }
    private ExpressionSyntax? _condition;
    public ExpressionSyntax? Condition
    {
        get { return GetRed(ref _condition, 5); }
    }
    public SyntaxToken SecondSemicolon
    {
        get
        {
            return new SyntaxToken(
                ((GreenForStatement)Green).SecondSemicolon,
                this,
                GetSlotPosition(6)
            );
        }
    }
    private SyntaxNode? _incrementors;
    public SeparatedSyntaxList<ExpressionSyntax> Incrementors
    {
        get
        {
            var red = GetRed(ref _incrementors, 7);
            return new SeparatedSyntaxList<ExpressionSyntax>(red);
        }
    }
    public SyntaxToken CloseParen
    {
        get
        {
            return new SyntaxToken(((GreenForStatement)Green).CloseParen, this, GetSlotPosition(8));
        }
    }
    private BlockSyntax? _block;
    public BlockSyntax Block
    {
        get { return GetRed(ref _block, 9); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index switch
        {
            2 => GetRed(ref _declaration, 2),
            3 => GetRed(ref _initializers, 3),
            5 => GetRed(ref _condition, 5),
            7 => GetRed(ref _incrementors, 7),
            9 => GetRed(ref _block, 9),
            _ => null,
        };
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index switch
        {
            2 => _declaration,
            3 => _initializers,
            5 => _condition,
            7 => _incrementors,
            9 => _block,
            _ => null,
        };
    }
};

public sealed class BreakStatementSyntax : StatementSyntax
{
    internal BreakStatementSyntax(GreenBreakStatement node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken Keyword
    {
        get { return new SyntaxToken(((GreenBreakStatement)Green).Keyword, this, Position); }
    }
    public SyntaxToken? Label
    {
        get
        {
            var green = ((GreenBreakStatement)Green).Label;
            return green is not null ? new SyntaxToken(green, this, GetSlotPosition(1)) : null;
        }
    }
    public SyntaxToken Semicolon
    {
        get
        {
            return new SyntaxToken(
                ((GreenBreakStatement)Green).Semicolon,
                this,
                GetSlotPosition(2)
            );
        }
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

public sealed class ContinueStatementSyntax : StatementSyntax
{
    internal ContinueStatementSyntax(GreenContinueStatement node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken Keyword
    {
        get { return new SyntaxToken(((GreenContinueStatement)Green).Keyword, this, Position); }
    }
    public SyntaxToken? Label
    {
        get
        {
            var green = ((GreenContinueStatement)Green).Label;
            return green is not null ? new SyntaxToken(green, this, GetSlotPosition(1)) : null;
        }
    }
    public SyntaxToken Semicolon
    {
        get
        {
            return new SyntaxToken(
                ((GreenContinueStatement)Green).Semicolon,
                this,
                GetSlotPosition(2)
            );
        }
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

public sealed class LabeledStatementSyntax : StatementSyntax
{
    internal LabeledStatementSyntax(GreenLabeledStatement node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken Identifier
    {
        get { return new SyntaxToken(((GreenLabeledStatement)Green).Identifier, this, Position); }
    }
    public SyntaxToken Colon
    {
        get
        {
            return new SyntaxToken(((GreenLabeledStatement)Green).Colon, this, GetSlotPosition(1));
        }
    }
    private StatementSyntax? _statement;
    public StatementSyntax Statement
    {
        get { return GetRed(ref _statement, 2); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 2 ? GetRed(ref _statement) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 2 ? _statement : null;
    }
};
