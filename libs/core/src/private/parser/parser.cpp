/**
 * @file parser.cpp
 * @author Francesco Corso
 * @date 6/24/2026
 * @brief
 */
module prism.core.parser;
import prism.core.ast.statement_syntax;
import prism.core.ast.common_syntax;
import prism.core.util;

namespace prism
{
    namespace
    {
        constexpr std::array built_in_types = {
            std::make_pair(TokenKind::kw_bool, BuiltInType::boolean),
            std::make_pair(TokenKind::kw_i8, BuiltInType::i8),
            std::make_pair(TokenKind::kw_i16, BuiltInType::i16),
            std::make_pair(TokenKind::kw_i32, BuiltInType::i32),
            std::make_pair(TokenKind::kw_i64, BuiltInType::i64),
            std::make_pair(TokenKind::kw_i128, BuiltInType::i128),
            std::make_pair(TokenKind::kw_u8, BuiltInType::u8),
            std::make_pair(TokenKind::kw_u16, BuiltInType::u16),
            std::make_pair(TokenKind::kw_u32, BuiltInType::u32),
            std::make_pair(TokenKind::kw_u64, BuiltInType::u64),
            std::make_pair(TokenKind::kw_u128, BuiltInType::u128),
            std::make_pair(TokenKind::kw_isize, BuiltInType::isize),
            std::make_pair(TokenKind::kw_usize, BuiltInType::usize),
            std::make_pair(TokenKind::kw_f16, BuiltInType::f16),
            std::make_pair(TokenKind::kw_f32, BuiltInType::f32),
            std::make_pair(TokenKind::kw_f64, BuiltInType::f64),
            std::make_pair(TokenKind::kw_char, BuiltInType::char8),
            std::make_pair(TokenKind::kw_char16, BuiltInType::char16),
            std::make_pair(TokenKind::kw_rune, BuiltInType::rune),
            std::make_pair(TokenKind::kw_str, BuiltInType::str),
        };

        constexpr std::array binary_operators = {
            std::make_pair(TokenKind::plus, BinaryOperator::add),
            std::make_pair(TokenKind::minus, BinaryOperator::sub),
            std::make_pair(TokenKind::star, BinaryOperator::mul),
            std::make_pair(TokenKind::slash, BinaryOperator::div),
            std::make_pair(TokenKind::percent, BinaryOperator::mod),

            std::make_pair(TokenKind::amp_amp, BinaryOperator::logical_and),
            std::make_pair(TokenKind::pipe_pipe, BinaryOperator::logical_or),

            std::make_pair(TokenKind::amp, BinaryOperator::bit_and),
            std::make_pair(TokenKind::pipe, BinaryOperator::bit_or),
            std::make_pair(TokenKind::caret, BinaryOperator::bit_xor),

            std::make_pair(TokenKind::equal_equal, BinaryOperator::equal),
            std::make_pair(TokenKind::exclaim_equal, BinaryOperator::not_equal),
            std::make_pair(TokenKind::less, BinaryOperator::less),
            std::make_pair(TokenKind::less_equal, BinaryOperator::less_equal),
            std::make_pair(TokenKind::greater, BinaryOperator::greater),
            std::make_pair(TokenKind::greater_equal, BinaryOperator::greater_equal),
            std::make_pair(TokenKind::spaceship, BinaryOperator::three_way),

            std::make_pair(TokenKind::less_less, BinaryOperator::shift_left),
            std::make_pair(TokenKind::greater_greater, BinaryOperator::shift_right),
            std::make_pair(TokenKind::greater_greater_greater, BinaryOperator::unsigned_shift_right),

            std::make_pair(TokenKind::equal, BinaryOperator::assign),
            std::make_pair(TokenKind::plus_equal, BinaryOperator::add_assign),
            std::make_pair(TokenKind::minus_equal, BinaryOperator::sub_assign),
            std::make_pair(TokenKind::star_equal, BinaryOperator::mul_assign),
            std::make_pair(TokenKind::slash_equal, BinaryOperator::div_assign),
            std::make_pair(TokenKind::percent_equal, BinaryOperator::mod_assign),
            std::make_pair(TokenKind::amp_equal, BinaryOperator::bit_and_assign),
            std::make_pair(TokenKind::pipe_equal, BinaryOperator::bit_or_assign),
            std::make_pair(TokenKind::caret_equal, BinaryOperator::bit_xor_assign),
            std::make_pair(TokenKind::less_less_equal, BinaryOperator::shift_left_assign),
            std::make_pair(TokenKind::greater_greater_equal, BinaryOperator::shift_right_assign),
            std::make_pair(TokenKind::greater_greater_greater_equal, BinaryOperator::unsigned_shift_right_assign),
        };

        constexpr std::int32_t get_precedence(const TokenKind kind)
        {
            switch (kind)
            {
                case TokenKind::star:
                case TokenKind::slash:
                case TokenKind::percent:
                    return 120;
                case TokenKind::plus:
                case TokenKind::minus:
                    return 110;
                case TokenKind::less_less:
                case TokenKind::greater_greater:
                case TokenKind::greater_greater_greater:
                    return 100;
                case TokenKind::less:
                case TokenKind::less_equal:
                case TokenKind::greater:
                case TokenKind::greater_equal:
                    return 90;
                case TokenKind::spaceship:
                    return 80;
                case TokenKind::equal_equal:
                case TokenKind::exclaim_equal:
                    return 70;
                case TokenKind::amp:
                    return 60;
                case TokenKind::caret:
                    return 50;
                case TokenKind::pipe:
                    return 40;
                case TokenKind::question_question:
                    return 30;
                case TokenKind::question:
                    return 20;
                case TokenKind::equal:
                case TokenKind::plus_equal:
                case TokenKind::minus_equal:
                case TokenKind::star_equal:
                case TokenKind::slash_equal:
                case TokenKind::percent_equal:
                case TokenKind::amp_equal:
                case TokenKind::pipe_equal:
                case TokenKind::caret_equal:
                case TokenKind::less_less_equal:
                case TokenKind::greater_greater_equal:
                case TokenKind::greater_greater_greater_equal:
                    return 10;
                default:
                    return -1;
            }
        }

        constexpr BinaryOperator get_binary_operator(const TokenKind kind)
        {
            for (const auto &[keyword, op] : binary_operators)
            {
                if (kind == keyword)
                {
                    return op;
                }
            }

            throw std::invalid_argument("Unexpected token");
        }
    } // namespace

    CompilationUnitSyntax Parser::parse_compilation_unit()
    {
        CompilationUnitSyntax compilation_unit;

        while (!stream_.at_end())
        {
            compilation_unit.declarations.push_back(parse_declaration());
        }

        return compilation_unit;
    }

    bool Parser::match(const TokenKind kind)
    {
        if (const auto [k, f, r] = stream_.peek(); k != kind)
        {
            return false;
        }

        stream_.advance();
        return true;
    }

    Token Parser::expect(const TokenKind kind)
    {
        if (match(kind))
        {
            return stream_.previous();
        }

        const auto start = stream_.previous().range.end;
        const auto next = stream_.peek();
        const auto end = next.range.start;
        diagnostics_.report(Severity::error,
                            SourceRange{start, end},
                            UnexpectedToken{.actual = next.kind, .expected = {kind}});

        return Token{.kind = kind, .flags = TokenFlags::synthetic, .range = {start, end}};
    }

    void Parser::synchronize(const bool include_semicolon)
    {
        static constexpr std::array sync_tokens = {
            TokenKind::kw_var,
            TokenKind::kw_func,
            TokenKind::rbrace,
        };
        while (!stream_.at_end())
        {
            if (include_semicolon)
            {
                if (stream_.previous().kind == TokenKind::semicolon)
                    return;
            }
            else
            {
                if (stream_.peek().kind == TokenKind::semicolon)
                    return;
            }

            if (is_next(sync_tokens))
            {
                return;
            }

            stream_.advance();
        }
    }

    bool Parser::is_next(std::span<const TokenKind> kinds)
    {
        auto next = stream_.peek();
        return std::ranges::any_of(kinds, [next](const auto &kind) { return next.kind == kind; });
    }

    std::pair<Modifiers, bool> Parser::parse_modifiers()
    {
        Modifiers modifiers;
        auto modifiers_seen = false;
        while (true)
        {
            switch (const auto next = stream_.peek(); next.kind)
            {
                case TokenKind::kw_extern:
                    modifiers.is_extern = true;
                    modifiers_seen = true;
                    stream_.advance();
                    break;
                default:
                    return std::make_pair(modifiers, modifiers_seen);
            }
        }
    }

    DeclarationSyntax Parser::parse_declaration()
    {
        const auto [modifiers, has_modifiers] = parse_modifiers();
        switch (const auto next_token = stream_.peek(); next_token.kind)
        {
            case TokenKind::kw_var: // NOLINT(*-branch-clone)
                return parse_variable_declaration(modifiers);
            case TokenKind::kw_func:
                return parse_function_declaration(modifiers);
            case TokenKind::semicolon:
                {
                    stream_.advance();
                    diagnostics_.report(Severity::warning, next_token.range, empty_statement);
                    return EmptySyntax{
                        .range = {next_token.range.start, next_token.range.start},
                    };
                }
            default:
                {
                    diagnostics_.report(Severity::error,
                                        next_token.range,
                                        UnexpectedToken{
                                            .actual = next_token.kind,
                                            .expected = {TokenKind::kw_var, TokenKind::kw_func},
                                        });
                    synchronize();
                    const auto error_statement_end = stream_.peek().range.start;

                    return ErrorSyntax{.range = {
                                           .start = next_token.range.start,
                                           .end = error_statement_end,
                                       }};
                }
        }
    }

    VariableDeclarationSyntax Parser::parse_variable_declaration(const Modifiers modifiers)
    {
        VariableDeclarationSyntax syntax{
            .modifiers = modifiers,
        };
        expect(TokenKind::kw_var);

        syntax.is_mutable = match(TokenKind::kw_mut);
        syntax.name = parse_identifier();

        if (match(TokenKind::colon))
        {
            syntax.type = parse_type();
        }

        expect(TokenKind::equal);

        syntax.initializer = std::make_unique<ExpressionSyntax>(parse_expression());

        expect(TokenKind::semicolon);

        return syntax;
    }

    FunctionDeclarationSyntax Parser::parse_function_declaration(const Modifiers modifiers)
    {
        FunctionDeclarationSyntax syntax{
            .modifiers = modifiers,
        };
        expect(TokenKind::kw_func);

        syntax.name = parse_identifier();

        if (match(TokenKind::lparen))
        {
            syntax.parameters = parse_parameter_list();
            expect(TokenKind::rparen);
        }
        else
        {
            auto next = stream_.peek();
            diagnostics_.report(Severity::error,
                                next.range,
                                UnexpectedToken{
                                    .actual = next.kind,
                                    .expected = {TokenKind::lparen},
                                });
        }

        if (match(TokenKind::arrow))
        {
            syntax.return_type = parse_type();
        }

        switch (const auto next = stream_.peek(); next.kind)
        {
            case TokenKind::lbrace:
                syntax.body = parse_block();
                break;
            case TokenKind::big_arrow:
                stream_.advance();
                syntax.body = parse_expression();
                break;
            case TokenKind::semicolon:
                syntax.body = EmptySyntax{
                    .range = {next.range.start, next.range.start},
                };
                break;
            default:
                diagnostics_.report(Severity::error,
                                    next.range,
                                    UnexpectedToken{
                                        .actual = next.kind,
                                        .expected = {TokenKind::lbrace, TokenKind::big_arrow, TokenKind::semicolon},
                                    });
                synchronize(false);
                break;
        }

        return syntax;
    }

    std::vector<ParameterDeclarationSyntax> Parser::parse_parameter_list()
    {
        std::vector<ParameterDeclarationSyntax> parameters;

        auto next = stream_.peek();
        while (next.kind != TokenKind::rparen)
        {
            if (!parameters.empty())
            {
                expect(TokenKind::comma);
            }

            auto &[name, type, is_mutable] = parameters.emplace_back();

            is_mutable = match(TokenKind::kw_mut);
            name = parse_identifier();

            if (match(TokenKind::colon))
            {
                type = parse_type();
            }

            next = stream_.peek();
        }

        return parameters;
    }

    IdentifierSyntax Parser::parse_identifier()
    {
        const auto identifier = expect(TokenKind::identifier);
        if (has_any_flags(identifier.flags, TokenFlags::synthetic))
        {
            return IdentifierSyntax{.range = identifier.range, .flags = IdentifierFlags::missing};
        }

        return IdentifierSyntax{
            .name = SharedString{source_file_.slice(identifier.range)},
            .range = identifier.range,
        };
    }

    TypeSyntax Parser::parse_type()
    {
        const auto next = stream_.peek();
        for (auto [token, type] : built_in_types)
        {
            if (token == next.kind)
            {
                stream_.advance();
                return BuiltInTypeSyntax{
                    .type = type,
                    .range = next.range,
                };
            }
        }

        const auto identifier = expect(TokenKind::identifier);
        if (has_any_flags(identifier.flags, TokenFlags::synthetic))
        {
            return ErrorSyntax{identifier.range};
        }

        return NamedTypeSyntax{.name = IdentifierSyntax{
                                   .name = SharedString{source_file_.slice(identifier.range)},
                                   .range = identifier.range,
                               }};
    }

    BlockSyntax Parser::parse_block()
    {
        BlockSyntax block;
        expect(TokenKind::lbrace);

        while (stream_.peek().kind != TokenKind::rbrace)
        {
            block.statements.push_back(parse_statement());
        }

        expect(TokenKind::rbrace);
        return block;
    }

    StatementSyntax Parser::parse_statement()
    {
        switch (auto next = stream_.peek(); next.kind)
        {
            case TokenKind::kw_var:
                return parse_variable_declaration();
            case TokenKind::kw_return:
                return parse_return_statement();
            case TokenKind::lbrace:
                return parse_block();
            case TokenKind::semicolon:
                stream_.advance();
                diagnostics_.report(Severity::warning, next.range, empty_statement);
                return EmptySyntax{
                    .range = {next.range.start, next.range.start},
                };
            default:
                return parse_expression_statement();
        }
    }

    ExpressionStatementSyntax Parser::parse_expression_statement()
    {
        ExpressionStatementSyntax statement;
        statement.expression = parse_expression();
        expect(TokenKind::semicolon);
        return statement;
    }

    ReturnStatementSyntax Parser::parse_return_statement()
    {
        ReturnStatementSyntax statement;
        expect(TokenKind::kw_return);
        statement.expression = parse_expression();
        expect(TokenKind::semicolon);
        return statement;
    }

    ExpressionSyntax Parser::parse_expression()
    {
        return parse_expression(parse_prefix_expression(), 0);
    }

    ExpressionSyntax Parser::parse_expression(ExpressionSyntax lhs, const std::int32_t min_precedence)
    {
        auto next = stream_.peek();
        auto precedence = get_precedence(next.kind);
        while (precedence >= min_precedence)
        {
            if (next.kind == TokenKind::question)
            {
                lhs = parse_ternary_expression(std::move(lhs));
            }
            else
            {
                const auto op = get_binary_operator(next.kind);
                stream_.advance();
                auto rhs = parse_prefix_expression();
                next = stream_.peek();
                auto inner_precedence = get_precedence(next.kind);
                while (inner_precedence >= precedence)
                {
                    rhs = parse_expression(std::move(rhs), inner_precedence > precedence ? precedence + 1 : precedence);

                    next = stream_.peek();
                    inner_precedence = get_precedence(next.kind);
                }

                lhs = BinaryExpressionSyntax{
                    .op = op,
                    .left = std::make_unique<ExpressionSyntax>(std::move(lhs)),
                    .right = std::make_unique<ExpressionSyntax>(std::move(rhs)),
                };
            }

            next = stream_.peek();
            precedence = get_precedence(next.kind);
        }

        return lhs;
    }

    ExpressionSyntax Parser::parse_ternary_expression(ExpressionSyntax lhs)
    {
        expect(TokenKind::question);
        auto true_expression = std::make_unique<ExpressionSyntax>(parse_expression());

        expect(TokenKind::colon);

        auto false_expression = std::make_unique<ExpressionSyntax>(parse_expression());

        return TernaryExpressionSyntax{
            .condition = std::make_unique<ExpressionSyntax>(std::move(lhs)),
            .if_true = std::move(true_expression),
            .if_false = std::move(false_expression),
        };
    }

    ExpressionSyntax Parser::parse_primary_expression()
    {
        switch (const auto [kind, flags, range] = stream_.peek(); kind)
        {
            case TokenKind::kw_false:
                stream_.advance();
                return LiteralSyntax{
                    .value = false,
                    .range = range,
                };
            case TokenKind::kw_true:
                stream_.advance();
                return LiteralSyntax{
                    .value = true,
                    .range = range,
                };
            case TokenKind::integer:
                stream_.advance();
                return LiteralSyntax{
                    .value = std::stoull(std::string{source_file_.slice(range)}),
                    .range = range,
                };
            case TokenKind::string_literal:
                return parse_string_literal();
            case TokenKind::identifier:
                {
                    return parse_identifier();
                }
            case TokenKind::lparen:
                {
                    stream_.advance();
                    auto expression = parse_expression();
                    expect(TokenKind::rparen);
                    return expression;
                }
            default:
                diagnostics_.report(Severity::error,
                                    range,
                                    UnexpectedToken{
                                        .actual = kind,
                                    });
                synchronize(false);
                return ErrorSyntax{
                    .range = range,
                };
        }
    }

    ExpressionSyntax Parser::parse_prefix_expression()
    {
        static constexpr std::array unary_operators = {
            std::make_pair(TokenKind::plus, UnaryOperator::positive),
            std::make_pair(TokenKind::minus, UnaryOperator::negate),
            std::make_pair(TokenKind::exclaim, UnaryOperator::logical_not),
            std::make_pair(TokenKind::tilde, UnaryOperator::bit_not),
            std::make_pair(TokenKind::plus_plus, UnaryOperator::pre_increment),
            std::make_pair(TokenKind::minus_minus, UnaryOperator::pre_decrement),
            std::make_pair(TokenKind::amp, UnaryOperator::address_of),
            std::make_pair(TokenKind::star, UnaryOperator::dereference),
        };

        for (const auto &[kind, op] : unary_operators)
        {
            if (match(kind))
            {
                return UnaryExpressionSyntax{
                    .operand = std::make_unique<ExpressionSyntax>(parse_prefix_expression()),
                    .op = op,
                };
            }
        }

        return parse_postfix_expression();
    }

    ExpressionSyntax Parser::parse_postfix_expression()
    {
        auto expression = parse_primary_expression();

        switch (const auto [kind, flags, range] = stream_.peek(); kind)
        {
            case TokenKind::lparen:
                {
                    stream_.advance();
                    expression = InvocationSyntax{.callee = std::make_unique<ExpressionSyntax>(std::move(expression)),
                                                  .arguments = parse_argument_list()};
                    expect(TokenKind::rparen);
                    break;
                }
            case TokenKind::plus_plus:
                stream_.advance();
                expression = UnaryExpressionSyntax{
                    .operand = std::make_unique<ExpressionSyntax>(std::move(expression)),
                    .op = UnaryOperator::post_increment,
                };
                break;
            case TokenKind::minus_minus:
                stream_.advance();
                expression = UnaryExpressionSyntax{
                    .operand = std::make_unique<ExpressionSyntax>(std::move(expression)),
                    .op = UnaryOperator::post_decrement,
                };
                break;
            default:
                // Do nothing
                break;
        }

        return expression;
    }

    std::vector<ExpressionSyntax> Parser::parse_argument_list()
    {
        std::vector<ExpressionSyntax> arguments;
        while (stream_.peek().kind != TokenKind::rparen)
        {
            if (!arguments.empty())
            {
                expect(TokenKind::comma);
            }

            arguments.push_back(parse_expression());
        }

        return arguments;
    }

    LiteralSyntax Parser::parse_string_literal()
    {
        const auto token = expect(TokenKind::string_literal);
        if (has_any_flags(token.flags, TokenFlags::synthetic))
        {
            return LiteralSyntax{
                .value = std::string{},
                .range = token.range,
            };
        }

        const auto text = source_file_.slice(token.range);
        // TODO: Escape sequences
        return LiteralSyntax{
            .value = std::string(text.substr(1, text.length() - 2)),
            .range = token.range,
        };
    }
} // namespace prism
