/**
 * @file parser.cpp
 * @author Francesco Corso
 * @date 6/24/2026
 * @brief
 */
module prism.core.parser;
import prism.core.parser.token_cursor;
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
        };

        constexpr std::int32_t get_precedence(const TokenKind kind)
        {
            switch (kind)
            {
                case TokenKind::star:
                case TokenKind::slash:
                    return 20;
                case TokenKind::plus:
                case TokenKind::minus:
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

            throw std::runtime_error("Unexpected token");
        }

        std::string parse_string_literal(const std::string_view text)
        {
            // TODO: Escape sequences
            return std::string(text.substr(1, text.length() - 2));
        }
    } // namespace

    ParseResult Parser::parse()
    {
        ParseResult result;

        while (!stream_.at_end())
        {
            result.compilation_unit.declarations.push_back(parse_declaration());
        }

        result.diagnostics = std::move(diagnostics_);
        return result;
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
        diagnostics_.emplace_back(Severity::error,
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

    DeclarationSyntax Parser::parse_declaration()
    {
        switch (const auto next_token = stream_.peek(); next_token.kind)
        {
            case TokenKind::kw_var: // NOLINT(*-branch-clone)
                return parse_variable_declaration();
            case TokenKind::kw_func:
                return parse_function_declaration();
            case TokenKind::semicolon:
                {
                    stream_.advance();
                    diagnostics_.emplace_back(Severity::warning, next_token.range, empty_statement);
                    return empty_syntax;
                }
            default:
                {
                    diagnostics_.emplace_back(Severity::error,
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

    VariableDeclarationSyntax Parser::parse_variable_declaration()
    {
        VariableDeclarationSyntax syntax;
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

    FunctionDeclarationSyntax Parser::parse_function_declaration()
    {
        FunctionDeclarationSyntax syntax;
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
            diagnostics_.emplace_back(Severity::error,
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

        switch (auto next = stream_.peek(); next.kind)
        {
            case TokenKind::lbrace:
                syntax.body = parse_block();
                break;
            case TokenKind::big_arrow:
                stream_.advance();
                syntax.body = parse_expression();
                break;
            case TokenKind::semicolon:
                syntax.body = empty_syntax;
                break;
            default:
                diagnostics_.emplace_back(
                    Severity::error,
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
            return ErrorSyntax{identifier.range};
        }

        return ValidIdentifierSyntax{
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

        return NamedTypeSyntax{.name = ValidIdentifierSyntax{
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
                diagnostics_.emplace_back(Severity::warning, next.range, empty_statement);
                return empty_syntax;
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
        switch (const auto [kind, flags, range] = stream_.peek(); kind)
        {
            case TokenKind::semicolon:
                diagnostics_.emplace_back(Severity::error, stream_.previous().range, empty_expression);
                return ErrorSyntax{
                    .range = stream_.previous().range,
                };
            case TokenKind::number:
                stream_.advance();
                return LiteralSyntax{
                    .value = std::stoull(std::string{source_file_.slice(range)}),
                    .range = range,
                };
            default:
                diagnostics_.emplace_back(Severity::error,
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
} // namespace prism
