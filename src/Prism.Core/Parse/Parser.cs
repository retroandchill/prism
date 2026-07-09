// @file Parser.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Numerics;
using Cysharp.Text;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using Prism.Core.Syntax.Green;
using Prism.Core.Utils;

namespace Prism.Core.Parse;

public sealed class Parser(SourceDocument sourceDocument)
{
    private readonly TokenStream _stream = new(sourceDocument.SourceFile);

    public SourceUnit ParseCompilationUnit()
    {
        var builder = ImmutableArray.CreateBuilder<DeclarationSyntax>();
        while (!_stream.AtEnd)
        {
            builder.Add(ParseDeclaration());
        }

        var declarations = builder.DrainToImmutable();

        var syntax = new CompilationUnitSyntax
        {
            Range = declarations.IsEmpty
                ? TextSpan.Empty
                : declarations[0].Range.Concat(declarations[^1].Range),
            Declarations = declarations,
        };

        return new SourceUnit(sourceDocument.SourceFile, syntax, sourceDocument.Diagnostics);
    }

    private IdentifierSyntax ParseIdentifier()
    {
        var identifier = Expect(SyntaxKind.IdentifierToken);
        var flags = SyntaxFlags.None;
        if (identifier.IsMissing)
        {
            flags |= SyntaxFlags.Missing;
        }

        identifier.TryGetIdentifierInfo(out var info);
        return new IdentifierSyntax(info.Identifier, identifier.Span, flags);
    }

    public DeclarationSyntax ParseDeclaration()
    {
        var modifiers = ParseModifiers();
        var next = _stream.Peek();
        switch (next.Kind)
        {
            case SyntaxKind.VarKeyword:
                return ParseVariableDeclaration(modifiers);
            case SyntaxKind.FuncKeyword:
                return ParseFunctionDeclaration(modifiers);
            case SyntaxKind.SemicolonToken when modifiers.IsEmpty:
                _stream.Advance();
                //sourceDocument.Diagnostics.EmptyStatement(next.Range);
                return new EmptyDeclarationSyntax { Range = next.Span };
            default:
                //sourceDocument.Diagnostics.UnexpectedToken(next.Range, next.Kind);
                return new EmptyDeclarationSyntax { Range = next.Span };
        }
    }

    private static Modifier? GetModifier(SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.ExternKeyword => Modifier.Extern,
            _ => null,
        };
    }

    private ImmutableArray<ModifierSyntax> ParseModifiers()
    {
        var builder = ImmutableArray.CreateBuilder<ModifierSyntax>();
        while (!_stream.AtEnd)
        {
            var next = _stream.Peek();
            if (GetModifier(next.Kind) is { } modifier)
            {
                _stream.Advance();
                builder.Add(new ModifierSyntax(modifier, next.Span));
            }
            else
            {
                break;
            }
        }

        return builder.DrainToImmutable();
    }

    private VariableDeclarationSyntax ParseVariableDeclaration(
        ImmutableArray<ModifierSyntax> modifiers
    )
    {
        var varKeyword = Expect(SyntaxKind.VarKeyword);
        var start = modifiers.IsEmpty ? varKeyword.Span : modifiers[0].Range;

        var isMutable = Match(SyntaxKind.MutKeyword);
        var name = ParseIdentifier();

        TypeSyntax? type = null;
        if (Match(SyntaxKind.ColonToken))
        {
            type = ParseType();
        }

        if (Match(SyntaxKind.SemicolonToken))
        {
            return new VariableDeclarationSyntax
            {
                Modifiers = modifiers,
                Identifier = name,
                Type = type,
                IsMutable = isMutable,
                Range = start.Concat(_stream.Previous.Span),
            };
        }

        Expect(SyntaxKind.EqualToken);

        var initializer = ParseExpression();
        Expect(SyntaxKind.SemicolonToken);

        return new VariableDeclarationSyntax
        {
            Modifiers = modifiers,
            Identifier = name,
            Type = type,
            IsMutable = isMutable,
            Initializer = initializer,
            Range = start.Concat(_stream.Previous.Span),
        };
    }

    private FunctionDeclarationSyntax ParseFunctionDeclaration(
        ImmutableArray<ModifierSyntax> modifiers
    )
    {
        var funcKeyword = Expect(SyntaxKind.FuncKeyword);
        var start = modifiers.IsEmpty ? funcKeyword.Span : modifiers[0].Range;

        var name = ParseIdentifier();

        ImmutableArray<ParameterDeclarationSyntax> parameters;
        var paramStart = Expect(SyntaxKind.LParenToken);
        if (!paramStart.IsMissing)
        {
            parameters = ParseParameterList();
            Expect(SyntaxKind.RParenToken);
        }
        else
        {
            parameters = [];
        }

        TypeSyntax? returnType = null;
        if (Match(SyntaxKind.ArrowToken))
        {
            returnType = ParseType();
        }

        IFunctionBodySyntax body;
        var next = _stream.Peek();
        switch (next.Kind)
        {
            case SyntaxKind.LBraceToken:
                body = ParseBlock();
                break;
            case SyntaxKind.BigArrowToken:
                _stream.Advance();
                body = ParseExpression();
                Expect(SyntaxKind.SemicolonToken);
                break;
            case SyntaxKind.SemicolonToken:
                _stream.Advance();
                body = EmptyBody.Instance;
                break;
            default:
                body = EmptyBody.Instance;
                //sourceDocument.Diagnostics.UnexpectedToken(next.Range, next.Kind);
                Synchronize(false);
                break;
        }

        return new FunctionDeclarationSyntax
        {
            Identifier = name,
            Modifiers = modifiers,
            ReturnType = returnType,
            Parameters = parameters,
            Body = body,
            Range = start.Concat(_stream.Previous.Span),
        };
    }

    private ImmutableArray<ParameterDeclarationSyntax> ParseParameterList()
    {
        var builder = ImmutableArray.CreateBuilder<ParameterDeclarationSyntax>();

        var next = _stream.Peek();
        while (next.Kind != SyntaxKind.RParenToken)
        {
            if (builder.Count > 0)
            {
                Expect(SyntaxKind.CommaToken);
            }

            var start = next.Span;

            var isMutable = Match(SyntaxKind.MutKeyword);
            var name = ParseIdentifier();

            Expect(SyntaxKind.ColonToken);
            var type = ParseType();
            builder.Add(
                new ParameterDeclarationSyntax
                {
                    Identifier = name,
                    IsMutable = isMutable,
                    Type = type,
                    Range = start.Concat(_stream.Previous.Span),
                }
            );

            next = _stream.Peek();
        }

        return builder.DrainToImmutable();
    }

    public TypeSyntax ParseType()
    {
        var next = _stream.Peek();
        if (BuiltInType.TryGet(next.Kind, out var type))
        {
            _stream.Advance();
            return new BuiltInTypeSyntax { Type = type, Range = next.Span };
        }

        var identifier = Expect(SyntaxKind.IdentifierToken);
        if (identifier.IsMissing)
        {
            return new BuiltInTypeSyntax
            {
                Type = BuiltInType.Void,
                Range = identifier.Span,
                Flags = SyntaxFlags.Missing,
            };
        }

        return new NamedTypeSyntax
        {
            Name = new IdentifierSyntax(
                sourceDocument.SourceFile.GetSpan(identifier.Span),
                identifier.Span
            ),
            Range = identifier.Span,
        };
    }

    public StatementSyntax ParseStatement()
    {
        var next = _stream.Peek();
        switch (next.Kind)
        {
            case SyntaxKind.VarKeyword:
            {
                var declaration = ParseVariableDeclaration([]);
                return new VariableDeclarationStatementSyntax
                {
                    Declaration = declaration,
                    Range = declaration.Range,
                };
            }
            case SyntaxKind.ReturnKeyword:
                return ParseReturnStatement();
            case SyntaxKind.LBraceToken:
                return ParseBlock();
            case SyntaxKind.SemicolonToken:
                _stream.Advance();
                //sourceDocument.Diagnostics.EmptyStatement(next.Range);
                return new EmptyStatementSyntax { Range = next.Span };
            default:
                return ParseExpressionStatement();
        }
    }

    private BlockSyntax ParseBlock()
    {
        var builder = ImmutableArray.CreateBuilder<StatementSyntax>();
        var start = Expect(SyntaxKind.LBraceToken).Span;

        while (_stream.Peek().Kind != SyntaxKind.RBraceToken)
        {
            if (_stream.AtEnd)
            {
                //sourceDocument.Diagnostics.UnexpectedEndOfFile(_stream.Peek().Range);
                break;
            }

            builder.Add(ParseStatement());
        }

        var end = Expect(SyntaxKind.RBraceToken).Span;
        var statements = builder.DrainToImmutable();

        return new BlockSyntax { Statements = statements, Range = start.Concat(end) };
    }

    private ExpressionStatementSyntax ParseExpressionStatement()
    {
        var expression = ParseExpression();
        Expect(SyntaxKind.SemicolonToken);
        return new ExpressionStatementSyntax
        {
            Expression = expression,
            Range = expression.Range.Concat(_stream.Previous.Span),
        };
    }

    private ReturnStatementSyntax ParseReturnStatement()
    {
        var start = Expect(SyntaxKind.ReturnKeyword).Span;
        if (Match(SyntaxKind.SemicolonToken))
        {
            return new ReturnStatementSyntax { Range = start.Concat(_stream.Previous.Span) };
        }

        var expression = ParseExpression();
        Expect(SyntaxKind.SemicolonToken);
        return new ReturnStatementSyntax
        {
            Expression = expression,
            Range = start.Concat(_stream.Previous.Span),
        };
    }

    public ExpressionSyntax ParseExpression()
    {
        return ParseExpression(ParsePrefixExpression(), 0);
    }

    private ExpressionSyntax ParseExpression(ExpressionSyntax lhs, int minPrecedence)
    {
        var next = _stream.Peek();
        var precedence = GetOperatorPrecedence(next.Kind);
        while (precedence >= minPrecedence)
        {
            if (next.Kind == SyntaxKind.QuestionToken)
            {
                lhs = ParseTernaryExpression(lhs);
            }
            else
            {
                var op = next.Kind.ToBinaryOperator();
                _stream.Advance();
                var rhs = ParsePrefixExpression();
                next = _stream.Peek();
                var innerPrecedence = GetOperatorPrecedence(next.Kind);
                while (innerPrecedence >= precedence)
                {
                    rhs = ParseExpression(
                        rhs,
                        innerPrecedence > precedence ? precedence + 1 : precedence
                    );

                    next = _stream.Peek();
                    innerPrecedence = GetOperatorPrecedence(next.Kind);
                }

                lhs = new BinaryExpressionSyntax
                {
                    Left = lhs,
                    Operator = op,
                    Right = rhs,
                    Range = lhs.Range.Concat(rhs.Range),
                };
            }

            next = _stream.Peek();
            precedence = GetOperatorPrecedence(next.Kind);
        }

        return lhs;
    }

    private static int GetOperatorPrecedence(SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.StarToken or SyntaxKind.SlashToken or SyntaxKind.PercentToken => 120,
            SyntaxKind.PlusToken or SyntaxKind.MinusToken => 110,
            SyntaxKind.LessLessToken
            or SyntaxKind.GreaterGreaterToken
            or SyntaxKind.GreaterGreaterGreaterToken => 100,
            SyntaxKind.LessToken
            or SyntaxKind.GreaterToken
            or SyntaxKind.LessEqualToken
            or SyntaxKind.GreaterEqualToken => 90,
            SyntaxKind.SpaceshipToken => 80,
            SyntaxKind.EqualEqualToken or SyntaxKind.ExclaimEqualToken => 70,
            SyntaxKind.AmpToken => 60,
            SyntaxKind.CaretToken => 50,
            SyntaxKind.PipeToken => 40,
            SyntaxKind.QuestionQuestionToken => 30,
            SyntaxKind.QuestionToken => 20,
            SyntaxKind.EqualToken
            or SyntaxKind.PlusEqualToken
            or SyntaxKind.MinusEqualToken
            or SyntaxKind.StarEqualToken
            or SyntaxKind.SlashEqualToken
            or SyntaxKind.PercentEqualToken
            or SyntaxKind.LessLessEqualToken
            or SyntaxKind.GreaterGreaterEqualToken
            or SyntaxKind.GreaterGreaterGreaterEqualToken
            or SyntaxKind.AmpEqualToken
            or SyntaxKind.CaretEqualToken
            or SyntaxKind.PipeEqualToken => 10,
            _ => -1,
        };
    }

    private TernaryExpressionSyntax ParseTernaryExpression(ExpressionSyntax lhs)
    {
        Expect(SyntaxKind.QuestionToken);
        var ifTrue = ParseExpression();
        Expect(SyntaxKind.ColonToken);
        var ifFalse = ParseExpression();
        return new TernaryExpressionSyntax
        {
            Condition = lhs,
            TrueExpression = ifTrue,
            FalseExpression = ifFalse,
            Range = lhs.Range.Concat(ifFalse.Range),
        };
    }

    private ExpressionSyntax ParsePrimaryExpression()
    {
        var next = _stream.Peek();
        var span = sourceDocument.SourceFile.GetSpan(next.Span);
        switch (next.Kind)
        {
            case SyntaxKind.FalseKeyword:
                _stream.Advance();
                return new BooleanLiteralExpressionSyntax { Value = false, Range = next.Span };
            case SyntaxKind.TrueKeyword:
                _stream.Advance();
                return new BooleanLiteralExpressionSyntax { Value = true, Range = next.Span };
            case SyntaxKind.IntegerLiteralToken:
            {
                next.TryGetLiteralValue<IntegerLiteralValue>(out var info);
                _stream.Advance();
                return new IntegerLiteralExpressionSyntax
                {
                    Base = info.Base,
                    Value = info.Value,
                    Suffix = info.Suffix,
                    Range = next.Span,
                };
            }
            case SyntaxKind.FloatingPointLiteralToken:
            {
                next.TryGetLiteralValue<FloatLiteralValue>(out var info);
                _stream.Advance();
                return new FloatLiteralExpressionSyntax
                {
                    Value = info.Value,
                    Suffix = info.Suffix,
                    Range = next.Span,
                };
            }
            case SyntaxKind.StringLiteralToken:
            {
                _stream.Advance();
                next.TryGetLiteralValue<StringLiteralValue>(out var value);
                return new StringLiteralExpressionSyntax { Value = value.Value, Range = next.Span };
            }
            case SyntaxKind.IdentifierToken:
            {
                var identifier = ParseIdentifier();
                return new IdentifierExpressionSyntax
                {
                    Name = identifier.Name,
                    Range = identifier.Range,
                };
            }
            case SyntaxKind.LParenToken:
            {
                _stream.Advance();
                var expression = ParseExpression();
                Expect(SyntaxKind.RParenToken);
                return expression;
            }
            default:
                //sourceDocument.Diagnostics.UnexpectedToken(next.Range, next.Kind);
                return new InvalidExpressionSyntax()
                {
                    Range = next.Span,
                    Flags = SyntaxFlags.Unknown,
                };
        }
    }

    private ExpressionSyntax ParsePrefixExpression()
    {
        var next = _stream.Peek();
        if (TryGetUnaryOperator(next.Kind) is not { } op)
            return ParsePostfixExpression();

        _stream.Advance();
        var operand = ParsePrefixExpression();
        return new UnaryExpressionSyntax
        {
            Operator = op,
            Operand = operand,
            Range = next.Span.Concat(operand.Range),
        };
    }

    private static UnaryOperator? TryGetUnaryOperator(SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.PlusToken => UnaryOperator.Positive,
            SyntaxKind.MinusToken => UnaryOperator.Negate,
            SyntaxKind.ExclaimToken => UnaryOperator.LogicalNot,
            SyntaxKind.TildeToken => UnaryOperator.BitNot,
            SyntaxKind.PlusPlusToken => UnaryOperator.PreIncrement,
            SyntaxKind.MinusMinusToken => UnaryOperator.PreDecrement,
            SyntaxKind.AmpToken => UnaryOperator.AddressOf,
            SyntaxKind.StarToken => UnaryOperator.Dereference,
            _ => null,
        };
    }

    private ExpressionSyntax ParsePostfixExpression()
    {
        var expression = ParsePrimaryExpression();
        var next = _stream.Peek();
        switch (next.Kind)
        {
            case SyntaxKind.LParenToken:
                _stream.Advance();
                var (arguments, range) = ParseArgumentList(next);
                var end = Expect(SyntaxKind.RParenToken);
                return new InvocationExpressionSyntax
                {
                    Callee = expression,
                    Arguments = arguments,
                    ArgumentsRange = range,
                    Range = expression.Range.Concat(end.Span),
                };
            case SyntaxKind.PlusPlusToken:
                _stream.Advance();
                return new UnaryExpressionSyntax
                {
                    Operator = UnaryOperator.PostIncrement,
                    Operand = expression,
                    Range = expression.Range.Concat(next.Span),
                };
            case SyntaxKind.MinusMinusToken:
                _stream.Advance();
                return new UnaryExpressionSyntax
                {
                    Operator = UnaryOperator.PostDecrement,
                    Operand = expression,
                    Range = expression.Range.Concat(next.Span),
                };
            default:
                return expression;
        }
    }

    private (ImmutableArray<ExpressionSyntax>, TextSpan) ParseArgumentList(SyntaxToken start)
    {
        var builder = ImmutableArray.CreateBuilder<ExpressionSyntax>();
        var next = _stream.Peek();
        while (next.Kind != SyntaxKind.RParenToken)
        {
            if (_stream.AtEnd)
            {
                //sourceDocument.Diagnostics.UnexpectedEndOfFile(next.Range);
                break;
            }

            if (builder.Count > 0)
            {
                Expect(SyntaxKind.CommaToken);
            }

            // If we start with <identifier>: then we have a named argument
            if (
                next.Kind == SyntaxKind.IdentifierToken
                && _stream.Peek(2).Kind == SyntaxKind.ColonToken
            )
            {
                var name = ParseIdentifier();
                Expect(SyntaxKind.ColonToken);
                next = _stream.Peek();
                ExpressionSyntax expression;
                if (next.Kind != SyntaxKind.CommaToken)
                {
                    expression = ParseExpression();
                }
                else
                {
                    expression = new InvalidExpressionSyntax
                    {
                        Range = name.Range.AsEmpty(),
                        Flags = SyntaxFlags.Missing,
                    };
                    //sourceDocument.Diagnostics.UnexpectedToken(next.Range, next.Kind);
                }

                builder.Add(
                    new NamedArgumentSyntax
                    {
                        Identifier = name,
                        Value = expression,
                        Range = name.Range.Concat(expression.Range),
                    }
                );
            }
            else
            {
                builder.Add(ParseExpression());
            }

            next = _stream.Peek();
        }
        return (builder.DrainToImmutable(), start.Span.Concat(next.Span));
    }

    private bool Match(SyntaxKind kind) => Match(kind, out _);

    private bool Match(SyntaxKind kind, out SyntaxToken token)
    {
        var next = _stream.Peek();
        if (next.Kind == kind)
        {
            _stream.Advance();
            token = next;
            return true;
        }

        token = new SyntaxToken(new GreenToken(kind, 0), next.Position);
        return false;
    }

    private SyntaxToken Expect(SyntaxKind kind)
    {
        if (!Match(kind, out var token))
        {
            //sourceDocument.Diagnostics.UnexpectedToken(_stream.Peek().Range, token.Kind);
        }

        return token;
    }

    private void Synchronize(bool includeSemicolon = true)
    {
        while (!_stream.AtEnd)
        {
            if (_stream.Previous.Kind == SyntaxKind.SemicolonToken)
            {
                if (includeSemicolon)
                {
                    _stream.Advance();
                }

                return;
            }

            if (
                IsNext(
                    SyntaxKind.VarKeyword,
                    SyntaxKind.FuncKeyword,
                    SyntaxKind.RBraceToken,
                    SyntaxKind.ExternKeyword
                )
            )
                return;

            _stream.Advance();
        }
    }

    private bool IsNext(params ReadOnlySpan<SyntaxKind> kinds)
    {
        return kinds.Contains(_stream.Peek().Kind);
    }
}
