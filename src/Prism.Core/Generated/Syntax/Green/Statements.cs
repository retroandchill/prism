// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;

namespace Prism.Core.Syntax.Green;

internal abstract class GreenStatement : GreenNode
{
    protected GreenStatement(SyntaxKind kind)
        : base(kind) { }
}

internal sealed class GreenEmptyStatement : GreenStatement
{
    GreenEmptyStatement(GreenToken semicolon)
        : base(SyntaxKind.EmptyStatement)
    {
        Semicolon = semicolon;
    }

    public GreenToken Semicolon { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index == 0 ? Semicolon : null;
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenEmptyStatement WithSemicolon(GreenToken semicolon)
    {
        if (Semicolon == semicolon)
            return this;

        return new GreenEmptyStatement(semicolon) { Diagnostics = Diagnostics };
    }

    public override GreenEmptyStatement WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenEmptyStatement(Semicolon) { Diagnostics = diagnostics };
    }

    public GreenEmptyStatement Update(GreenToken semicolon)
    {
        if (Semicolon == semicolon)
        {
            return this;
        }

        return new GreenEmptyStatement(semicolon) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenVariableDeclarationStatement : GreenStatement
{
    GreenVariableDeclarationStatement(GreenVariableDeclaration declaration)
        : base(SyntaxKind.VariableDeclarationStatement)
    {
        Declaration = declaration;
    }

    public GreenVariableDeclaration Declaration { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index == 0 ? Declaration : null;
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenVariableDeclarationStatement WithDeclaration(GreenVariableDeclaration declaration)
    {
        if (Declaration == declaration)
            return this;

        return new GreenVariableDeclarationStatement(declaration) { Diagnostics = Diagnostics };
    }

    public override GreenVariableDeclarationStatement WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenVariableDeclarationStatement(Declaration) { Diagnostics = diagnostics };
    }

    public GreenVariableDeclarationStatement Update(GreenVariableDeclaration declaration)
    {
        if (Declaration == declaration)
        {
            return this;
        }

        return new GreenVariableDeclarationStatement(declaration) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenBlock : GreenStatement
{
    GreenBlock(
        GreenToken openBrace,
        GreenSyntaxList<GreenStatement> statements,
        GreenToken closeBrace
    )
        : base(SyntaxKind.Block)
    {
        OpenBrace = openBrace;
        Statements = statements;
        CloseBrace = closeBrace;
    }

    public GreenToken OpenBrace { get; }
    public GreenSyntaxList<GreenStatement> Statements { get; }
    public GreenToken CloseBrace { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => OpenBrace,
            1 => Statements.Node,
            2 => CloseBrace,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenBlock WithOpenBrace(GreenToken openBrace)
    {
        if (OpenBrace == openBrace)
            return this;

        return new GreenBlock(openBrace, Statements, CloseBrace) { Diagnostics = Diagnostics };
    }

    public GreenBlock WithStatements(GreenSyntaxList<GreenStatement> statements)
    {
        if (Statements == statements)
            return this;

        return new GreenBlock(OpenBrace, statements, CloseBrace) { Diagnostics = Diagnostics };
    }

    public GreenBlock WithCloseBrace(GreenToken closeBrace)
    {
        if (CloseBrace == closeBrace)
            return this;

        return new GreenBlock(OpenBrace, Statements, closeBrace) { Diagnostics = Diagnostics };
    }

    public override GreenBlock WithDiagnostics(ImmutableArray<SyntaxDiagnosticInfo> diagnostics)
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenBlock(OpenBrace, Statements, CloseBrace) { Diagnostics = diagnostics };
    }

    public GreenBlock Update(
        GreenToken openBrace,
        GreenSyntaxList<GreenStatement> statements,
        GreenToken closeBrace
    )
    {
        if (OpenBrace == openBrace && Statements == statements && CloseBrace == closeBrace)
        {
            return this;
        }

        return new GreenBlock(openBrace, statements, closeBrace) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenReturnStatement : GreenStatement
{
    GreenReturnStatement(
        GreenToken returnKeyword,
        GreenExpression? expression,
        GreenToken semicolon
    )
        : base(SyntaxKind.ReturnStatement)
    {
        ReturnKeyword = returnKeyword;
        Expression = expression;
        Semicolon = semicolon;
    }

    public GreenToken ReturnKeyword { get; }
    public GreenExpression? Expression { get; }
    public GreenToken Semicolon { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => ReturnKeyword,
            1 => Expression,
            2 => Semicolon,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenReturnStatement WithReturnKeyword(GreenToken returnKeyword)
    {
        if (ReturnKeyword == returnKeyword)
            return this;

        return new GreenReturnStatement(returnKeyword, Expression, Semicolon)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenReturnStatement WithExpression(GreenExpression? expression)
    {
        if (Expression == expression)
            return this;

        return new GreenReturnStatement(ReturnKeyword, expression, Semicolon)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenReturnStatement WithSemicolon(GreenToken semicolon)
    {
        if (Semicolon == semicolon)
            return this;

        return new GreenReturnStatement(ReturnKeyword, Expression, semicolon)
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenReturnStatement WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenReturnStatement(ReturnKeyword, Expression, Semicolon)
        {
            Diagnostics = diagnostics,
        };
    }

    public GreenReturnStatement Update(
        GreenToken returnKeyword,
        GreenExpression? expression,
        GreenToken semicolon
    )
    {
        if (ReturnKeyword == returnKeyword && Expression == expression && Semicolon == semicolon)
        {
            return this;
        }

        return new GreenReturnStatement(returnKeyword, expression, semicolon)
        {
            Diagnostics = Diagnostics,
        };
    }
}

internal sealed class GreenExpressionStatement : GreenStatement
{
    GreenExpressionStatement(GreenExpression expression, GreenToken semicolon)
        : base(SyntaxKind.ExpressionStatement)
    {
        Expression = expression;
        Semicolon = semicolon;
    }

    public GreenExpression Expression { get; }
    public GreenToken Semicolon { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Expression,
            1 => Semicolon,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenExpressionStatement WithExpression(GreenExpression expression)
    {
        if (Expression == expression)
            return this;

        return new GreenExpressionStatement(expression, Semicolon) { Diagnostics = Diagnostics };
    }

    public GreenExpressionStatement WithSemicolon(GreenToken semicolon)
    {
        if (Semicolon == semicolon)
            return this;

        return new GreenExpressionStatement(Expression, semicolon) { Diagnostics = Diagnostics };
    }

    public override GreenExpressionStatement WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenExpressionStatement(Expression, Semicolon) { Diagnostics = diagnostics };
    }

    public GreenExpressionStatement Update(GreenExpression expression, GreenToken semicolon)
    {
        if (Expression == expression && Semicolon == semicolon)
        {
            return this;
        }

        return new GreenExpressionStatement(expression, semicolon) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenIfStatement : GreenStatement
{
    GreenIfStatement(
        GreenToken ifKeyword,
        GreenToken openParen,
        GreenExpression condition,
        GreenToken closeParen,
        GreenBlock block,
        GreenElseClause? elseClause
    )
        : base(SyntaxKind.IfStatement)
    {
        IfKeyword = ifKeyword;
        OpenParen = openParen;
        Condition = condition;
        CloseParen = closeParen;
        Block = block;
        ElseClause = elseClause;
    }

    public GreenToken IfKeyword { get; }
    public GreenToken OpenParen { get; }
    public GreenExpression Condition { get; }
    public GreenToken CloseParen { get; }
    public GreenBlock Block { get; }
    public GreenElseClause? ElseClause { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => IfKeyword,
            1 => OpenParen,
            2 => Condition,
            3 => CloseParen,
            4 => Block,
            5 => ElseClause,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenIfStatement WithIfKeyword(GreenToken ifKeyword)
    {
        if (IfKeyword == ifKeyword)
            return this;

        return new GreenIfStatement(ifKeyword, OpenParen, Condition, CloseParen, Block, ElseClause)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenIfStatement WithOpenParen(GreenToken openParen)
    {
        if (OpenParen == openParen)
            return this;

        return new GreenIfStatement(IfKeyword, openParen, Condition, CloseParen, Block, ElseClause)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenIfStatement WithCondition(GreenExpression condition)
    {
        if (Condition == condition)
            return this;

        return new GreenIfStatement(IfKeyword, OpenParen, condition, CloseParen, Block, ElseClause)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenIfStatement WithCloseParen(GreenToken closeParen)
    {
        if (CloseParen == closeParen)
            return this;

        return new GreenIfStatement(IfKeyword, OpenParen, Condition, closeParen, Block, ElseClause)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenIfStatement WithBlock(GreenBlock block)
    {
        if (Block == block)
            return this;

        return new GreenIfStatement(IfKeyword, OpenParen, Condition, CloseParen, block, ElseClause)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenIfStatement WithElseClause(GreenElseClause? elseClause)
    {
        if (ElseClause == elseClause)
            return this;

        return new GreenIfStatement(IfKeyword, OpenParen, Condition, CloseParen, Block, elseClause)
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenIfStatement WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenIfStatement(IfKeyword, OpenParen, Condition, CloseParen, Block, ElseClause)
        {
            Diagnostics = diagnostics,
        };
    }

    public GreenIfStatement Update(
        GreenToken ifKeyword,
        GreenToken openParen,
        GreenExpression condition,
        GreenToken closeParen,
        GreenBlock block,
        GreenElseClause? elseClause
    )
    {
        if (
            IfKeyword == ifKeyword
            && OpenParen == openParen
            && Condition == condition
            && CloseParen == closeParen
            && Block == block
            && ElseClause == elseClause
        )
        {
            return this;
        }

        return new GreenIfStatement(ifKeyword, openParen, condition, closeParen, block, elseClause)
        {
            Diagnostics = Diagnostics,
        };
    }
}

internal sealed class GreenWhileStatement : GreenStatement
{
    GreenWhileStatement(
        GreenToken whileKeyword,
        GreenToken openParen,
        GreenExpression condition,
        GreenToken closeParen,
        GreenBlock block
    )
        : base(SyntaxKind.WhileStatement)
    {
        WhileKeyword = whileKeyword;
        OpenParen = openParen;
        Condition = condition;
        CloseParen = closeParen;
        Block = block;
    }

    public GreenToken WhileKeyword { get; }
    public GreenToken OpenParen { get; }
    public GreenExpression Condition { get; }
    public GreenToken CloseParen { get; }
    public GreenBlock Block { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => WhileKeyword,
            1 => OpenParen,
            2 => Condition,
            3 => CloseParen,
            4 => Block,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenWhileStatement WithWhileKeyword(GreenToken whileKeyword)
    {
        if (WhileKeyword == whileKeyword)
            return this;

        return new GreenWhileStatement(whileKeyword, OpenParen, Condition, CloseParen, Block)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenWhileStatement WithOpenParen(GreenToken openParen)
    {
        if (OpenParen == openParen)
            return this;

        return new GreenWhileStatement(WhileKeyword, openParen, Condition, CloseParen, Block)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenWhileStatement WithCondition(GreenExpression condition)
    {
        if (Condition == condition)
            return this;

        return new GreenWhileStatement(WhileKeyword, OpenParen, condition, CloseParen, Block)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenWhileStatement WithCloseParen(GreenToken closeParen)
    {
        if (CloseParen == closeParen)
            return this;

        return new GreenWhileStatement(WhileKeyword, OpenParen, Condition, closeParen, Block)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenWhileStatement WithBlock(GreenBlock block)
    {
        if (Block == block)
            return this;

        return new GreenWhileStatement(WhileKeyword, OpenParen, Condition, CloseParen, block)
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenWhileStatement WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenWhileStatement(WhileKeyword, OpenParen, Condition, CloseParen, Block)
        {
            Diagnostics = diagnostics,
        };
    }

    public GreenWhileStatement Update(
        GreenToken whileKeyword,
        GreenToken openParen,
        GreenExpression condition,
        GreenToken closeParen,
        GreenBlock block
    )
    {
        if (
            WhileKeyword == whileKeyword
            && OpenParen == openParen
            && Condition == condition
            && CloseParen == closeParen
            && Block == block
        )
        {
            return this;
        }

        return new GreenWhileStatement(whileKeyword, openParen, condition, closeParen, block)
        {
            Diagnostics = Diagnostics,
        };
    }
}

internal sealed class GreenLoopStatement : GreenStatement
{
    GreenLoopStatement(GreenToken loopKeyword, GreenBlock block)
        : base(SyntaxKind.LoopStatement)
    {
        LoopKeyword = loopKeyword;
        Block = block;
    }

    public GreenToken LoopKeyword { get; }
    public GreenBlock Block { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => LoopKeyword,
            1 => Block,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenLoopStatement WithLoopKeyword(GreenToken loopKeyword)
    {
        if (LoopKeyword == loopKeyword)
            return this;

        return new GreenLoopStatement(loopKeyword, Block) { Diagnostics = Diagnostics };
    }

    public GreenLoopStatement WithBlock(GreenBlock block)
    {
        if (Block == block)
            return this;

        return new GreenLoopStatement(LoopKeyword, block) { Diagnostics = Diagnostics };
    }

    public override GreenLoopStatement WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenLoopStatement(LoopKeyword, Block) { Diagnostics = diagnostics };
    }

    public GreenLoopStatement Update(GreenToken loopKeyword, GreenBlock block)
    {
        if (LoopKeyword == loopKeyword && Block == block)
        {
            return this;
        }

        return new GreenLoopStatement(loopKeyword, block) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenForStatement : GreenStatement
{
    GreenForStatement(
        GreenToken forKeyword,
        GreenToken openParen,
        GreenVariableDeclarationStatement? declaration,
        GreenSeparatedList<GreenExpression> initializers,
        GreenToken firstSemicolon,
        GreenExpression? condition,
        GreenToken secondSemicolon,
        GreenSeparatedList<GreenExpression> incrementors,
        GreenToken closeParen,
        GreenBlock block
    )
        : base(SyntaxKind.ForStatement)
    {
        ForKeyword = forKeyword;
        OpenParen = openParen;
        Declaration = declaration;
        Initializers = initializers;
        FirstSemicolon = firstSemicolon;
        Condition = condition;
        SecondSemicolon = secondSemicolon;
        Incrementors = incrementors;
        CloseParen = closeParen;
        Block = block;
    }

    public GreenToken ForKeyword { get; }
    public GreenToken OpenParen { get; }
    public GreenVariableDeclarationStatement? Declaration { get; }
    public GreenSeparatedList<GreenExpression> Initializers { get; }
    public GreenToken FirstSemicolon { get; }
    public GreenExpression? Condition { get; }
    public GreenToken SecondSemicolon { get; }
    public GreenSeparatedList<GreenExpression> Incrementors { get; }
    public GreenToken CloseParen { get; }
    public GreenBlock Block { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => ForKeyword,
            1 => OpenParen,
            2 => Declaration,
            3 => Initializers.Node,
            4 => FirstSemicolon,
            5 => Condition,
            6 => SecondSemicolon,
            7 => Incrementors.Node,
            8 => CloseParen,
            9 => Block,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenForStatement WithForKeyword(GreenToken forKeyword)
    {
        if (ForKeyword == forKeyword)
            return this;

        return new GreenForStatement(
            forKeyword,
            OpenParen,
            Declaration,
            Initializers,
            FirstSemicolon,
            Condition,
            SecondSemicolon,
            Incrementors,
            CloseParen,
            Block
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenForStatement WithOpenParen(GreenToken openParen)
    {
        if (OpenParen == openParen)
            return this;

        return new GreenForStatement(
            ForKeyword,
            openParen,
            Declaration,
            Initializers,
            FirstSemicolon,
            Condition,
            SecondSemicolon,
            Incrementors,
            CloseParen,
            Block
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenForStatement WithDeclaration(GreenVariableDeclarationStatement? declaration)
    {
        if (Declaration == declaration)
            return this;

        return new GreenForStatement(
            ForKeyword,
            OpenParen,
            declaration,
            Initializers,
            FirstSemicolon,
            Condition,
            SecondSemicolon,
            Incrementors,
            CloseParen,
            Block
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenForStatement WithInitializers(GreenSeparatedList<GreenExpression> initializers)
    {
        if (Initializers == initializers)
            return this;

        return new GreenForStatement(
            ForKeyword,
            OpenParen,
            Declaration,
            initializers,
            FirstSemicolon,
            Condition,
            SecondSemicolon,
            Incrementors,
            CloseParen,
            Block
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenForStatement WithFirstSemicolon(GreenToken firstSemicolon)
    {
        if (FirstSemicolon == firstSemicolon)
            return this;

        return new GreenForStatement(
            ForKeyword,
            OpenParen,
            Declaration,
            Initializers,
            firstSemicolon,
            Condition,
            SecondSemicolon,
            Incrementors,
            CloseParen,
            Block
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenForStatement WithCondition(GreenExpression? condition)
    {
        if (Condition == condition)
            return this;

        return new GreenForStatement(
            ForKeyword,
            OpenParen,
            Declaration,
            Initializers,
            FirstSemicolon,
            condition,
            SecondSemicolon,
            Incrementors,
            CloseParen,
            Block
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenForStatement WithSecondSemicolon(GreenToken secondSemicolon)
    {
        if (SecondSemicolon == secondSemicolon)
            return this;

        return new GreenForStatement(
            ForKeyword,
            OpenParen,
            Declaration,
            Initializers,
            FirstSemicolon,
            Condition,
            secondSemicolon,
            Incrementors,
            CloseParen,
            Block
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenForStatement WithIncrementors(GreenSeparatedList<GreenExpression> incrementors)
    {
        if (Incrementors == incrementors)
            return this;

        return new GreenForStatement(
            ForKeyword,
            OpenParen,
            Declaration,
            Initializers,
            FirstSemicolon,
            Condition,
            SecondSemicolon,
            incrementors,
            CloseParen,
            Block
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenForStatement WithCloseParen(GreenToken closeParen)
    {
        if (CloseParen == closeParen)
            return this;

        return new GreenForStatement(
            ForKeyword,
            OpenParen,
            Declaration,
            Initializers,
            FirstSemicolon,
            Condition,
            SecondSemicolon,
            Incrementors,
            closeParen,
            Block
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenForStatement WithBlock(GreenBlock block)
    {
        if (Block == block)
            return this;

        return new GreenForStatement(
            ForKeyword,
            OpenParen,
            Declaration,
            Initializers,
            FirstSemicolon,
            Condition,
            SecondSemicolon,
            Incrementors,
            CloseParen,
            block
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenForStatement WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenForStatement(
            ForKeyword,
            OpenParen,
            Declaration,
            Initializers,
            FirstSemicolon,
            Condition,
            SecondSemicolon,
            Incrementors,
            CloseParen,
            Block
        )
        {
            Diagnostics = diagnostics,
        };
    }

    public GreenForStatement Update(
        GreenToken forKeyword,
        GreenToken openParen,
        GreenVariableDeclarationStatement? declaration,
        GreenSeparatedList<GreenExpression> initializers,
        GreenToken firstSemicolon,
        GreenExpression? condition,
        GreenToken secondSemicolon,
        GreenSeparatedList<GreenExpression> incrementors,
        GreenToken closeParen,
        GreenBlock block
    )
    {
        if (
            ForKeyword == forKeyword
            && OpenParen == openParen
            && Declaration == declaration
            && Initializers == initializers
            && FirstSemicolon == firstSemicolon
            && Condition == condition
            && SecondSemicolon == secondSemicolon
            && Incrementors == incrementors
            && CloseParen == closeParen
            && Block == block
        )
        {
            return this;
        }

        return new GreenForStatement(
            forKeyword,
            openParen,
            declaration,
            initializers,
            firstSemicolon,
            condition,
            secondSemicolon,
            incrementors,
            closeParen,
            block
        )
        {
            Diagnostics = Diagnostics,
        };
    }
}

internal sealed class GreenBreakStatement : GreenStatement
{
    GreenBreakStatement(GreenToken keyword, GreenToken semicolon)
        : base(SyntaxKind.BreakStatement)
    {
        Keyword = keyword;
        Semicolon = semicolon;
    }

    public GreenToken Keyword { get; }
    public GreenToken Semicolon { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Keyword,
            1 => Semicolon,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenBreakStatement WithKeyword(GreenToken keyword)
    {
        if (Keyword == keyword)
            return this;

        return new GreenBreakStatement(keyword, Semicolon) { Diagnostics = Diagnostics };
    }

    public GreenBreakStatement WithSemicolon(GreenToken semicolon)
    {
        if (Semicolon == semicolon)
            return this;

        return new GreenBreakStatement(Keyword, semicolon) { Diagnostics = Diagnostics };
    }

    public override GreenBreakStatement WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenBreakStatement(Keyword, Semicolon) { Diagnostics = diagnostics };
    }

    public GreenBreakStatement Update(GreenToken keyword, GreenToken semicolon)
    {
        if (Keyword == keyword && Semicolon == semicolon)
        {
            return this;
        }

        return new GreenBreakStatement(keyword, semicolon) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenContinueStatement : GreenStatement
{
    GreenContinueStatement(GreenToken keyword, GreenToken semicolon)
        : base(SyntaxKind.ContinueStatement)
    {
        Keyword = keyword;
        Semicolon = semicolon;
    }

    public GreenToken Keyword { get; }
    public GreenToken Semicolon { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Keyword,
            1 => Semicolon,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenContinueStatement WithKeyword(GreenToken keyword)
    {
        if (Keyword == keyword)
            return this;

        return new GreenContinueStatement(keyword, Semicolon) { Diagnostics = Diagnostics };
    }

    public GreenContinueStatement WithSemicolon(GreenToken semicolon)
    {
        if (Semicolon == semicolon)
            return this;

        return new GreenContinueStatement(Keyword, semicolon) { Diagnostics = Diagnostics };
    }

    public override GreenContinueStatement WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenContinueStatement(Keyword, Semicolon) { Diagnostics = diagnostics };
    }

    public GreenContinueStatement Update(GreenToken keyword, GreenToken semicolon)
    {
        if (Keyword == keyword && Semicolon == semicolon)
        {
            return this;
        }

        return new GreenContinueStatement(keyword, semicolon) { Diagnostics = Diagnostics };
    }
}
