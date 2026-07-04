// @file Parser.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Ast;

namespace Prism.Core.Parse;

public sealed class Parser(CompilationContext context)
{
    private readonly TokenStream _stream = new(context.SourceFile);

    public CompilationUnitSyntax ParseCompilationUnit()
    {
        var builder = ImmutableArray.CreateBuilder<DeclarationSyntax>();
        while (!_stream.AtEnd)
        {
            builder.Add(ParseDeclaration());
        }

        var declarations = builder.DrainToImmutable();

        return new CompilationUnitSyntax
        {
            Range = declarations.IsEmpty
                ? SourceRange.Empty
                : declarations[0].Range.Concat(declarations[^1].Range),
            Declarations = declarations,
        };
    }

    private IdentifierSyntax ParseIdentifier()
    {
        var identifier = Expect(TokenKind.Identifier);
        var flags = SyntaxFlags.None;
        if (identifier.IsSynthetic)
        {
            flags |= SyntaxFlags.Missing;
        }

        return new IdentifierSyntax(context.GetSpan(identifier.Range), identifier.Range, flags);
    }

    public DeclarationSyntax ParseDeclaration()
    {
        var modifiers = ParseModifiers();
        var next = _stream.Peek();
        switch (next.Kind)
        {
            case TokenKind.Var:
                return ParseVariableDeclaration(modifiers);
            case TokenKind.Func:
                return ParseFunctionDeclaration(modifiers);
            case TokenKind.Semicolon when modifiers.IsEmpty:
                _stream.Advance();
                context.ReportDiagnostic(
                    new Diagnostic
                    {
                        Descriptor = ParseDiagnostics.EmptyStatement,
                        Range = next.Range,
                    }
                );
                return new EmptyDeclarationSyntax { Range = next.Range };
            default:
                context.ReportDiagnostic(
                    new Diagnostic
                    {
                        Descriptor = ParseDiagnostics.UnexpectedToken,
                        Range = next.Range,
                        Arguments = [next.Kind],
                    }
                );
                return new EmptyDeclarationSyntax { Range = next.Range };
        }
    }

    private static Modifier? GetModifier(TokenKind kind)
    {
        return kind switch
        {
            TokenKind.Extern => Modifier.Extern,
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
                builder.Add(new ModifierSyntax(modifier, next.Range));
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
        var varKeyword = Expect(TokenKind.Var);
        var start = modifiers.IsEmpty ? varKeyword.Range : modifiers[0].Range;

        var isMutable = Match(TokenKind.Mut);
        var name = ParseIdentifier();

        TypeSyntax? type = null;
        if (Match(TokenKind.Colon))
        {
            type = ParseType();
        }

        if (Match(TokenKind.Semicolon))
        {
            return new VariableDeclarationSyntax
            {
                Modifiers = modifiers,
                Identifier = name,
                Type = type,
                IsMutable = isMutable,
                Range = start.Concat(_stream.Previous.Range),
            };
        }

        Expect(TokenKind.Equal);

        var initializer = ParseExpression();
        Expect(TokenKind.Semicolon);

        return new VariableDeclarationSyntax
        {
            Modifiers = modifiers,
            Identifier = name,
            Type = type,
            IsMutable = isMutable,
            Initializer = initializer,
            Range = start.Concat(_stream.Previous.Range),
        };
    }

    private FunctionDeclarationSyntax ParseFunctionDeclaration(
        ImmutableArray<ModifierSyntax> modifiers
    )
    {
        var funcKeyword = Expect(TokenKind.Func);
        var start = modifiers.IsEmpty ? funcKeyword.Range : modifiers[0].Range;

        var name = ParseIdentifier();

        ImmutableArray<ParameterDeclarationSyntax> parameters;
        var paramStart = Expect(TokenKind.LParen);
        if (!paramStart.IsSynthetic)
        {
            parameters = ParseParameterList();
            Expect(TokenKind.RParen);
        }
        else
        {
            parameters = [];
        }

        TypeSyntax? returnType = null;
        if (Match(TokenKind.Arrow))
        {
            returnType = ParseType();
        }

        FunctionBodySyntax body;
        var next = _stream.Peek();
        switch (next.Kind)
        {
            case TokenKind.LBrace:
                body = ParseBlock();
                break;
            case TokenKind.BigArrow:
                _stream.Advance();
                body = ParseExpression();
                Expect(TokenKind.Semicolon);
                break;
            case TokenKind.Semicolon:
                _stream.Advance();
                body = EmptyBody.Instance;
                break;
            default:
                body = EmptyBody.Instance;
                context.ReportDiagnostic(
                    new Diagnostic
                    {
                        Descriptor = ParseDiagnostics.UnexpectedToken,
                        Range = next.Range,
                        Arguments = [next.Kind],
                    }
                );
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
            Range = start.Concat(_stream.Previous.Range),
        };
    }

    private ImmutableArray<ParameterDeclarationSyntax> ParseParameterList()
    {
        var builder = ImmutableArray.CreateBuilder<ParameterDeclarationSyntax>();

        var next = _stream.Peek();
        while (next.Kind != TokenKind.RParen)
        {
            if (builder.Count > 0)
            {
                Expect(TokenKind.Comma);
            }

            var start = next.Range;

            var isMutable = Match(TokenKind.Mut);
            var name = ParseIdentifier();

            Expect(TokenKind.Colon);
            var type = ParseType();
            builder.Add(
                new ParameterDeclarationSyntax
                {
                    Identifier = name,
                    IsMutable = isMutable,
                    Type = type,
                    Range = start.Concat(_stream.Previous.Range),
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
            return new BuiltInTypeSyntax { Type = type, Range = next.Range };
        }

        var identifier = Expect(TokenKind.Identifier);
        if (identifier.IsSynthetic)
        {
            return new BuiltInTypeSyntax
            {
                Type = BuiltInType.Void,
                Range = identifier.Range,
                Flags = SyntaxFlags.Missing,
            };
        }

        return new NamedTypeSyntax
        {
            Name = new IdentifierSyntax(context.GetSpan(identifier.Range), identifier.Range),
            Range = identifier.Range,
        };
    }

    public StatementSyntax ParseStatement()
    {
        var next = _stream.Peek();
        switch (next.Kind)
        {
            case TokenKind.Var:
            {
                var declaration = ParseVariableDeclaration([]);
                return new VariableDeclarationStatementSyntax
                {
                    Declaration = declaration,
                    Range = declaration.Range,
                };
            }
            case TokenKind.Return:
                return ParseReturnStatement();
            case TokenKind.LBrace:
                return ParseBlock();
            case TokenKind.Semicolon:
                _stream.Advance();
                context.ReportDiagnostic(
                    new Diagnostic
                    {
                        Descriptor = ParseDiagnostics.EmptyStatement,
                        Range = next.Range,
                    }
                );
                return new EmptyStatementSyntax { Range = next.Range };
            default:
                return ParseExpressionStatement();
        }
    }

    private BlockSyntax ParseBlock()
    {
        var builder = ImmutableArray.CreateBuilder<StatementSyntax>();
        var start = Expect(TokenKind.LBrace).Range;

        while (_stream.Peek().Kind != TokenKind.RBrace)
        {
            if (_stream.AtEnd)
            {
                context.ReportDiagnostic(
                    new Diagnostic
                    {
                        Descriptor = ParseDiagnostics.UnexpectedEndOfFile,
                        Range = _stream.Peek().Range,
                    }
                );
                break;
            }

            builder.Add(ParseStatement());
        }

        var end = Expect(TokenKind.RBrace).Range;
        var statements = builder.DrainToImmutable();

        return new BlockSyntax { Statements = statements, Range = start.Concat(end) };
    }

    private ExpressionStatementSyntax ParseExpressionStatement()
    {
        var expression = ParseExpression();
        Expect(TokenKind.Semicolon);
        return new ExpressionStatementSyntax
        {
            Expression = expression,
            Range = expression.Range.Concat(_stream.Previous.Range),
        };
    }

    private ReturnStatementSyntax ParseReturnStatement()
    {
        var start = Expect(TokenKind.Return).Range;
        var expression = ParseExpression();
        Expect(TokenKind.Semicolon);
        return new ReturnStatementSyntax
        {
            Expression = expression,
            Range = start.Concat(_stream.Previous.Range),
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
            if (next.Kind == TokenKind.Question)
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

    private static int GetOperatorPrecedence(TokenKind kind)
    {
        return kind switch
        {
            TokenKind.Star or TokenKind.Slash or TokenKind.Percent => 120,
            TokenKind.Plus or TokenKind.Minus => 110,
            TokenKind.LessLess or TokenKind.GreaterGreater or TokenKind.GreaterGreaterGreater =>
                100,
            TokenKind.Less or TokenKind.Greater or TokenKind.LessEqual or TokenKind.GreaterEqual =>
                90,
            TokenKind.Spaceship => 80,
            TokenKind.EqualEqual or TokenKind.ExclaimEqual => 70,
            TokenKind.Amp => 60,
            TokenKind.Caret => 50,
            TokenKind.Pipe => 40,
            TokenKind.QuestionQuestion => 30,
            TokenKind.Question => 20,
            TokenKind.Equal
            or TokenKind.PlusEqual
            or TokenKind.MinusEqual
            or TokenKind.StarEqual
            or TokenKind.SlashEqual
            or TokenKind.PercentEqual
            or TokenKind.LessLessEqual
            or TokenKind.GreaterGreaterEqual
            or TokenKind.GreaterGreaterGreaterEqual
            or TokenKind.AmpEqual
            or TokenKind.CaretEqual
            or TokenKind.PipeEqual => 10,
            _ => -1,
        };
    }

    private TernaryExpressionSyntax ParseTernaryExpression(ExpressionSyntax lhs)
    {
        Expect(TokenKind.Question);
        var ifTrue = ParseExpression();
        Expect(TokenKind.Colon);
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
        switch (next.Kind)
        {
            case TokenKind.False:
                _stream.Advance();
                return new LiteralExpressionSyntax
                {
                    Kind = LiteralKind.BoolFalse,
                    Range = next.Range,
                };
            case TokenKind.True:
                _stream.Advance();
                return new LiteralExpressionSyntax
                {
                    Kind = LiteralKind.BoolTrue,
                    Range = next.Range,
                };
            case TokenKind.IntegerLiteral:
                _stream.Advance();
                return new LiteralExpressionSyntax
                {
                    Kind = LiteralKind.Integer,
                    Range = next.Range,
                };
            case TokenKind.FloatingPointLiteral:
                _stream.Advance();
                return new LiteralExpressionSyntax { Kind = LiteralKind.Float, Range = next.Range };
            case TokenKind.StringLiteral:
                _stream.Advance();
                return new LiteralExpressionSyntax
                {
                    Kind = LiteralKind.String,
                    Range = next.Range,
                };
            case TokenKind.Identifier:
            {
                var identifier = ParseIdentifier();
                return new IdentifierExpressionSyntax
                {
                    Name = identifier.Name,
                    Range = identifier.Range,
                };
            }
            case TokenKind.LParen:
            {
                _stream.Advance();
                var expression = ParseExpression();
                Expect(TokenKind.RParen);
                return expression;
            }
            default:
                context.ReportDiagnostic(
                    new Diagnostic
                    {
                        Descriptor = ParseDiagnostics.UnexpectedToken,
                        Range = _stream.Peek().Range,
                        Arguments = [next.Kind],
                    }
                );
                return new InvalidExpressionSyntax()
                {
                    Range = next.Range,
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
            Range = next.Range.Concat(operand.Range),
        };
    }

    private static UnaryOperator? TryGetUnaryOperator(TokenKind kind)
    {
        return kind switch
        {
            TokenKind.Plus => UnaryOperator.Positive,
            TokenKind.Minus => UnaryOperator.Negate,
            TokenKind.Exclaim => UnaryOperator.LogicalNot,
            TokenKind.Tilde => UnaryOperator.BitNot,
            TokenKind.PlusPlus => UnaryOperator.PreIncrement,
            TokenKind.MinusMinus => UnaryOperator.PreDecrement,
            TokenKind.Amp => UnaryOperator.AddressOf,
            TokenKind.Star => UnaryOperator.Dereference,
            _ => null,
        };
    }

    private ExpressionSyntax ParsePostfixExpression()
    {
        var expression = ParsePrimaryExpression();
        var next = _stream.Peek();
        switch (next.Kind)
        {
            case TokenKind.LParen:
                _stream.Advance();
                var arguments = ParseArgumentList();
                var end = Expect(TokenKind.RParen);
                return new InvocationExpressionSyntax
                {
                    Callee = expression,
                    Arguments = arguments,
                    Range = expression.Range.Concat(end.Range),
                };
            case TokenKind.PlusPlus:
                _stream.Advance();
                return new UnaryExpressionSyntax
                {
                    Operator = UnaryOperator.PostIncrement,
                    Operand = expression,
                    Range = expression.Range.Concat(next.Range),
                };
            case TokenKind.MinusMinus:
                _stream.Advance();
                return new UnaryExpressionSyntax
                {
                    Operator = UnaryOperator.PostDecrement,
                    Operand = expression,
                    Range = expression.Range.Concat(next.Range),
                };
            default:
                return expression;
        }
    }

    private ImmutableArray<ExpressionSyntax> ParseArgumentList()
    {
        var builder = ImmutableArray.CreateBuilder<ExpressionSyntax>();
        var next = _stream.Peek();
        while (next.Kind != TokenKind.RParen)
        {
            if (_stream.AtEnd)
            {
                context.ReportDiagnostic(
                    new Diagnostic
                    {
                        Descriptor = ParseDiagnostics.UnexpectedEndOfFile,
                        Range = next.Range,
                    }
                );
                break;
            }

            if (builder.Count > 0)
            {
                Expect(TokenKind.Comma);
            }

            // If we start with <identifier>: then we have a named argument
            if (next.Kind == TokenKind.Identifier && _stream.Peek(2).Kind == TokenKind.Colon)
            {
                var name = ParseIdentifier();
                Expect(TokenKind.Colon);
                next = _stream.Peek();
                ExpressionSyntax expression;
                if (next.Kind != TokenKind.Comma)
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
                    context.ReportDiagnostic(
                        new Diagnostic
                        {
                            Descriptor = ParseDiagnostics.UnexpectedToken,
                            Range = next.Range,
                            Arguments = [next.Kind],
                        }
                    );
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
        return builder.DrainToImmutable();
    }

    private bool Match(TokenKind kind) => Match(kind, out _);

    private bool Match(TokenKind kind, out Token token)
    {
        var next = _stream.Peek();
        if (next.Kind == kind)
        {
            _stream.Advance();
            token = next;
            return true;
        }

        token = new Token(kind, next.Range with { Length = 0 }, TokenFlags.Synthetic);
        return false;
    }

    private Token Expect(TokenKind kind)
    {
        if (!Match(kind, out var token))
        {
            context.ReportDiagnostic(
                new Diagnostic
                {
                    Descriptor = ParseDiagnostics.UnexpectedToken,
                    Range = _stream.Peek().Range,
                    Arguments = [token.Kind],
                }
            );
        }

        return token;
    }

    private void Synchronize(bool includeSemicolon = true)
    {
        while (!_stream.AtEnd)
        {
            if (_stream.Previous.Kind == TokenKind.Semicolon)
            {
                if (includeSemicolon)
                {
                    _stream.Advance();
                }

                return;
            }

            if (IsNext(TokenKind.Var, TokenKind.Func, TokenKind.RBrace, TokenKind.Extern))
                return;

            _stream.Advance();
        }
    }

    private bool IsNext(params ReadOnlySpan<TokenKind> kinds)
    {
        return kinds.Contains(_stream.Peek().Kind);
    }
}
