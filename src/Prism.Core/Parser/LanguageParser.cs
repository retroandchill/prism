using System.Diagnostics;
using System.Runtime.CompilerServices;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Parser;

internal sealed class LanguageParser(string text) : SyntaxParser(text)
{
    private readonly record struct NamespaceBody(
        GreenSyntaxList<GreenUsingDirective> Usings,
        GreenSyntaxList<GreenDeclaration> Members
    );

    public T ConsumeUnexpectedTokens<T>(T node)
        where T : GreenNode
    {
        var token = PeekToken();
        if (token.Kind == SyntaxKind.EofToken)
            return node;

        var builder = GreenSyntaxList.CreateBuilder<GreenToken>();
        while (token.Kind != SyntaxKind.EofToken)
        {
            builder.Add(ConsumeToken());
        }

        var list = builder.BuildAndClear();
        var copy = Unsafe.As<T>(
            node.WithDiagnostics(
                node.Diagnostics.Add(
                    new SyntaxDiagnosticInfo(DiagnosticInfo.UnexpectedToken(list[0].ToString()))
                )
            )
        );
        Debug.Assert(list.Node is not null);
        return AddTrailingSkippedSyntax(copy, list.Node);
    }

    public GreenCompilationUnit ParseCompilationUnit()
    {
        var (usings, members) = ParseNamespaceBody();
        return new GreenCompilationUnit(usings, members);
    }

    public GreenDeclaration ParseDeclaration()
    {
        var modifiers = ParseModifiers();
        return PeekToken().Kind switch
        {
            SyntaxKind.NamespaceKeyword => ParseNamespaceDeclaration(modifiers),
            SyntaxKind.VarKeyword => ParseVariableDeclaration(modifiers),
            SyntaxKind.FuncKeyword => ParseFunctionDeclaration(modifiers),
            _ => new GreenIncompleteDeclaration(modifiers),
        };
    }

    private GreenNamespaceDeclaration ParseNamespaceDeclaration(
        GreenSyntaxList<GreenToken> modifiers
    )
    {
        var namespaceKeyword = ExpectToken(SyntaxKind.NamespaceKeyword);
        var identifier = ParseName();
        var semicolon = MatchToken(SyntaxKind.SemicolonToken);
        if (semicolon is not null)
        {
            var (usings, members) = ParseNamespaceBody();
            return new GreenFileScopedNamespaceDeclaration(
                modifiers,
                namespaceKeyword,
                identifier,
                semicolon,
                usings,
                members
            );
        }
        else
        {
            var openBrace = ExpectToken(SyntaxKind.OpenBraceToken);
            var (usings, members) = ParseNamespaceBody(t => t.Kind != SyntaxKind.CloseBraceToken);
            return new GreenBlockNamespaceDeclaration(
                modifiers,
                namespaceKeyword,
                identifier,
                openBrace,
                usings,
                members,
                ExpectToken(SyntaxKind.CloseBraceToken)
            );
        }
    }

    private GreenVariableDeclaration ParseVariableDeclaration(
        GreenSyntaxList<GreenToken> modifiers = default
    )
    {
        return new GreenVariableDeclaration(
            modifiers,
            ExpectToken(SyntaxKind.VarKeyword),
            ExpectToken(SyntaxKind.IdentifierToken),
            ParseTypeSpecifier(),
            ParseInitializer(),
            ExpectToken(SyntaxKind.SemicolonToken)
        );
    }

    private GreenFunctionDeclaration ParseFunctionDeclaration(GreenSyntaxList<GreenToken> modifiers)
    {
        var funcKeyword = ExpectToken(SyntaxKind.FuncKeyword);
        var name = ExpectToken(SyntaxKind.IdentifierToken);
        var parameters = ParseParameterList();
        var returnType = ParseTypeSpecifier();

        var (block, expressionBody, semicolon) = PeekToken().Kind switch
        {
            SyntaxKind.OpenBraceToken => (
                ParseBlock(),
                (GreenExpressionBody?)null,
                (GreenToken?)null
            ),
            SyntaxKind.ArrowToken => (
                null,
                ParseExpressionBody(),
                ExpectToken(SyntaxKind.SemicolonToken)
            ),
            _ => (null, null, ExpectToken(SyntaxKind.SemicolonToken)),
        };

        return new GreenFunctionDeclaration(
            modifiers,
            funcKeyword,
            name,
            parameters,
            returnType,
            block,
            expressionBody,
            semicolon
        );
    }

    public GreenStatement ParseStatement()
    {
        var variableDeclaration = ParseVariableDeclarationStatement();
        if (variableDeclaration is not null)
            return variableDeclaration;

        return PeekToken().Kind switch
        {
            SyntaxKind.ReturnKeyword => ParseReturnStatement(),
            SyntaxKind.OpenBraceToken => ParseBlock(),
            SyntaxKind.SemicolonToken => new GreenEmptyStatement(ConsumeToken()),
            SyntaxKind.IfKeyword => ParseIfStatement(),
            SyntaxKind.BreakKeyword => ParseBreakStatement(),
            SyntaxKind.ContinueKeyword => ParseContinueStatement(),
            SyntaxKind.WhileKeyword => ParseWhileStatement(),
            SyntaxKind.LoopKeyword => ParseLoopStatement(),
            SyntaxKind.ForKeyword => ParseForStatement(),
            _ => ParseExpressionStatement(),
        };
    }

    private GreenReturnStatement ParseReturnStatement()
    {
        return new GreenReturnStatement(
            ExpectToken(SyntaxKind.ReturnKeyword),
            ParseExpression(),
            ExpectToken(SyntaxKind.SemicolonToken)
        );
    }

    private GreenVariableDeclarationStatement? ParseVariableDeclarationStatement()
    {
        return PeekToken().Kind switch
        {
            SyntaxKind.MutableKeyword => new GreenVariableDeclarationStatement(
                ParseVariableDeclaration(ParseModifiers())
            ),
            SyntaxKind.VarKeyword => new GreenVariableDeclarationStatement(
                ParseVariableDeclaration(ParseModifiers())
            ),
            _ => null,
        };
    }

    private GreenExpressionStatement ParseExpressionStatement()
    {
        return new GreenExpressionStatement(
            ParseExpression(),
            ExpectToken(SyntaxKind.SemicolonToken)
        );
    }

    private GreenBlock ParseBlock()
    {
        var builder = GreenSyntaxList.CreateBuilder<GreenStatement>();
        var start = ExpectToken(SyntaxKind.OpenBraceToken);

        while (!AtEnd && PeekToken().Kind != SyntaxKind.CloseBraceToken)
        {
            builder.Add(ParseStatement());
        }

        return new GreenBlock(
            start,
            builder.BuildAndClear(),
            ExpectToken(SyntaxKind.CloseBraceToken)
        );
    }

    private GreenIfStatement ParseIfStatement()
    {
        return new GreenIfStatement(
            ExpectToken(SyntaxKind.IfKeyword),
            ExpectToken(SyntaxKind.OpenParenToken),
            ParseExpression(),
            ExpectToken(SyntaxKind.CloseParenToken),
            ParseBlock(),
            ParseElseClause()
        );
    }

    private GreenWhileStatement ParseWhileStatement()
    {
        return new GreenWhileStatement(
            ExpectToken(SyntaxKind.WhileKeyword),
            ExpectToken(SyntaxKind.OpenParenToken),
            ParseExpression(),
            ExpectToken(SyntaxKind.CloseParenToken),
            ParseBlock()
        );
    }

    private GreenLoopStatement ParseLoopStatement()
    {
        return new GreenLoopStatement(ExpectToken(SyntaxKind.LoopKeyword), ParseBlock());
    }

    private GreenForStatement ParseForStatement()
    {
        var forKeyword = ExpectToken(SyntaxKind.ForKeyword);
        var openKeyword = ExpectToken(SyntaxKind.OpenParenToken);
        var declaration = ParseVariableDeclarationStatement();
        var initializers = GreenSeparatedList.CreateBuilder<GreenExpression>();
        if (declaration is null)
        {
            while (true)
            {
                initializers.AddItem(ParseExpression());

                var comma = MatchToken(SyntaxKind.CommaToken);
                if (comma is null)
                    break;

                initializers.AddSeparator(comma);
            }
        }

        var firstSemicolon = ExpectToken(SyntaxKind.SemicolonToken);
        var condition = PeekToken().Kind != SyntaxKind.SemicolonToken ? ParseExpression() : null;
        var secondSemicolon = ExpectToken(SyntaxKind.SemicolonToken);
        var incrementors = GreenSeparatedList.CreateBuilder<GreenExpression>();
        while (true)
        {
            incrementors.AddItem(ParseExpression());

            var comma = MatchToken(SyntaxKind.CommaToken);
            if (comma is null)
                break;

            incrementors.AddSeparator(comma);
        }

        return new GreenForStatement(
            forKeyword,
            openKeyword,
            declaration,
            initializers.BuildAndClear(),
            firstSemicolon,
            condition,
            secondSemicolon,
            incrementors.BuildAndClear(),
            ExpectToken(SyntaxKind.CloseParenToken),
            ParseBlock()
        );
    }

    private GreenBreakStatement ParseBreakStatement()
    {
        return new GreenBreakStatement(
            ExpectToken(SyntaxKind.BreakKeyword),
            ExpectToken(SyntaxKind.SemicolonToken)
        );
    }

    private GreenContinueStatement ParseContinueStatement()
    {
        return new GreenContinueStatement(
            ExpectToken(SyntaxKind.ContinueKeyword),
            ExpectToken(SyntaxKind.SemicolonToken)
        );
    }

    public GreenExpression ParseExpression()
    {
        return ParseExpression(ParsePrefixExpression(), 0);
    }

    private GreenExpression ParseExpression(GreenExpression left, int minPrecedence)
    {
        var next = PeekToken();
        var precedence = next.Kind.OperatorPrecedence;
        while (precedence >= minPrecedence)
        {
            if (next.Kind == SyntaxKind.QuestionToken)
            {
                left = ParseTernaryExpression(left);
            }
            else
            {
                var op = ConsumeToken();
                var right = ParsePrefixExpression();
                next = PeekToken();
                var innerPrecedence = next.Kind.OperatorPrecedence;
                while (innerPrecedence >= precedence)
                {
                    right = ParseExpression(
                        right,
                        innerPrecedence > precedence ? precedence + 1 : precedence
                    );
                    innerPrecedence = right.Kind.OperatorPrecedence;
                }

                if (op.Kind.IsAssignmentOperator)
                {
                    left = new GreenAssignmentExpression(left, op, right);
                }
                else
                {
                    left = new GreenBinaryExpression(left, op, right);
                }
            }

            next = PeekToken();
            precedence = next.Kind.OperatorPrecedence;
        }

        return left;
    }

    private GreenTernaryExpression ParseTernaryExpression(GreenExpression condition)
    {
        return new GreenTernaryExpression(
            condition,
            ExpectToken(SyntaxKind.QuestionToken),
            ParseExpression(),
            ExpectToken(SyntaxKind.ColonToken),
            ParseExpression()
        );
    }

    private GreenExpression ParsePrimaryExpression()
    {
        return PeekToken().Kind switch
        {
            SyntaxKind.FalseKeyword
            or SyntaxKind.TrueKeyword
            or SyntaxKind.IntegerLiteralToken
            or SyntaxKind.FloatingPointLiteralToken
            or SyntaxKind.CharacterLiteralToken
            or SyntaxKind.StringLiteralToken => new GreenLiteralExpression(ConsumeToken()),
            SyntaxKind.OpenParenToken => ParseParenthesizedExpression(),
            _ => new GreenIdentifierExpression(ParseName()),
        };
    }

    private GreenExpression ParsePrefixExpression()
    {
        if (!PeekToken().Kind.IsPrefixOperator)
            return ParsePostfixExpression();

        return new GreenPrefixExpression(ConsumeToken(), ParsePrefixExpression());
    }

    private GreenExpression ParsePostfixExpression()
    {
        var expression = ParsePrimaryExpression();
        var kind = PeekToken().Kind;
        return kind switch
        {
            SyntaxKind.OpenParenToken => new GreenInvocationExpression(
                expression,
                ParseArgumentList()
            ),
            SyntaxKind.AsKeyword => new GreenCastExpression(
                expression,
                ConsumeToken(),
                ParseType()
            ),
            { IsPostfixOperator: true } => new GreenPostfixExpression(expression, ConsumeToken()),
            _ => expression,
        };
    }

    private GreenExpression ParseParenthesizedExpression()
    {
        return new GreenParenthesizedExpression(
            ExpectToken(SyntaxKind.OpenParenToken),
            ParseExpression(),
            ExpectToken(SyntaxKind.CloseParenToken)
        );
    }

    private NamespaceBody ParseNamespaceBody(Predicate<GreenToken>? predicate = null)
    {
        var usingDirectives = GreenSyntaxList.CreateBuilder<GreenUsingDirective>();
        while (!AtEnd && PeekToken().Kind == SyntaxKind.UsingKeyword)
        {
            usingDirectives.Add(ParseUsingDirective());
        }

        var members = GreenSyntaxList.CreateBuilder<GreenDeclaration>();
        while (!AtEnd && predicate?.Invoke(PeekToken()) != false)
        {
            members.Add(ParseDeclaration());
        }

        return new NamespaceBody(usingDirectives.BuildAndClear(), members.BuildAndClear());
    }

    private GreenUsingDirective ParseUsingDirective()
    {
        return new GreenUsingDirective(
            ExpectToken(SyntaxKind.UsingKeyword),
            ParseName(),
            ExpectToken(SyntaxKind.SemicolonToken)
        );
    }

    private GreenSyntaxList<GreenToken> ParseModifiers()
    {
        var builder = GreenSyntaxList.CreateBuilder<GreenToken>();
        while (!AtEnd && PeekToken().Kind.IsModifier)
        {
            builder.Add(ConsumeToken());
        }

        return builder.BuildAndClear();
    }

    private GreenTypeSpecifier? ParseTypeSpecifier()
    {
        var colon = MatchToken(SyntaxKind.ColonToken);
        if (colon is null)
            return null;

        return new GreenTypeSpecifier(colon, ParseType());
    }

    private GreenTypeSpecifier ParseRequiredTypeSpecifier()
    {
        return new GreenTypeSpecifier(ExpectToken(SyntaxKind.ColonToken), ParseType());
    }

    private GreenType ParseType()
    {
        return PeekToken().Kind.IsBuiltInType
            ? new GreenPredefinedType(ConsumeToken())
            : new GreenNamedType(ParseName());
    }

    private GreenName ParseName()
    {
        GreenName name = ParseSimpleName();

        while (!AtEnd)
        {
            var separator = MatchToken(SyntaxKind.DoubleColonToken);
            if (separator is null)
                break;

            name = new GreenQualifiedName(name, separator, ParseSimpleName());
        }

        return name;
    }

    private GreenSimpleName ParseSimpleName()
    {
        return new GreenSimpleName(ExpectToken(SyntaxKind.IdentifierToken));
    }

    private GreenInitializer? ParseInitializer()
    {
        var equal = MatchToken(SyntaxKind.EqualToken);
        return equal is not null ? new GreenInitializer(equal, ParseExpression()) : null;
    }

    private GreenParameterList ParseParameterList()
    {
        return new GreenParameterList(
            ExpectToken(SyntaxKind.OpenParenToken),
            ParseParameters(),
            ExpectToken(SyntaxKind.CloseParenToken)
        );
    }

    private GreenSeparatedList<GreenParameter> ParseParameters()
    {
        var builder = GreenSeparatedList.CreateBuilder<GreenParameter>();

        var next = PeekToken();
        while (!AtEnd && next.Kind != SyntaxKind.CloseParenToken)
        {
            if (builder.Count > 0)
            {
                builder.AddSeparator(ExpectToken(SyntaxKind.CommaToken));
            }

            builder.AddItem(
                new GreenParameter(
                    ExpectToken(SyntaxKind.MutableKeyword),
                    ExpectToken(SyntaxKind.IdentifierToken),
                    ParseRequiredTypeSpecifier(),
                    ParseInitializer()
                )
            );

            next = PeekToken();
        }

        return builder.BuildAndClear();
    }

    private GreenExpressionBody ParseExpressionBody()
    {
        return new GreenExpressionBody(ExpectToken(SyntaxKind.ArrowToken), ParseExpression());
    }

    private GreenArgumentList ParseArgumentList()
    {
        var openParen = ExpectToken(SyntaxKind.OpenParenToken);
        var next = PeekToken();

        var builder = GreenSeparatedList.CreateBuilder<GreenArgument>();
        while (!AtEnd && next.Kind != SyntaxKind.CloseParenToken)
        {
            if (builder.Count > 0)
            {
                builder.AddSeparator(ExpectToken(SyntaxKind.CommaToken));
            }

            builder.AddItem(ParseArgument());
            next = PeekToken();
        }

        return new GreenArgumentList(
            openParen,
            builder.BuildAndClear(),
            ExpectToken(SyntaxKind.CloseParenToken)
        );
    }

    private GreenArgument ParseArgument()
    {
        return new GreenArgument(ParseNamedParameter(), ParseExpression());
    }

    private GreenNamedParameter? ParseNamedParameter()
    {
        if (
            PeekToken().Kind != SyntaxKind.IdentifierToken
            && PeekToken(2).Kind != SyntaxKind.ColonToken
        )
            return null;

        return new GreenNamedParameter(
            ExpectToken(SyntaxKind.IdentifierToken),
            ExpectToken(SyntaxKind.ColonToken)
        );
    }

    private GreenElseClause? ParseElseClause()
    {
        var elseToken = MatchToken(SyntaxKind.ElseKeyword);
        if (elseToken is null)
            return null;

        return PeekToken().Kind == SyntaxKind.IfKeyword
            ? new GreenElseClause(elseToken, ParseIfStatement())
            : new GreenElseClause(elseToken, ParseBlock());
    }
}
