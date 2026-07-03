/**
 * @file parser.cpp
 * @author Francesco Corso
 * @date 6/24/2026
 * @brief
 */
module prism.core.parser;

import prism.core.parser.token_cursor;
import prism.core.ast;
import prism.core.util;
import prism.core.memory.buffer_pool;

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
        PooledVector<Ref<const DeclarationSyntax>> declarations;

        while (!stream_.at_end())
        {
            declarations.emplace_back(parse_declaration());
        }

        if (declarations.empty())
        {
            return CompilationUnitSyntax{{}, SourceRange{}};
        }

        auto span = SourceRange{.start = declarations.front()->range().start, .end = declarations.back()->range().end};

        return CompilationUnitSyntax{context_.copy(declarations), span};
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
        context_.report(unexpected_token, SourceRange{start, end}, kind, std::span{&kind, 1});

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

    const DeclarationSyntax &Parser::parse_declaration()
    {
        static constexpr std::array permitted_tokens = {TokenKind::kw_var, TokenKind::kw_func};
        const auto [modifiers, has_modifiers] = parse_modifiers();
        switch (const auto next_token = stream_.peek(); next_token.kind)
        {
            case TokenKind::kw_var: // NOLINT(*-branch-clone)
                return parse_variable_declaration(modifiers);
            case TokenKind::kw_func:
                return parse_function_declaration(modifiers);
            case TokenKind::semicolon:
                stream_.advance();
                context_.report(empty_statement, next_token.range);
                return context_.create<EmptyDeclarationSyntax>(next_token.range);
            default:
                context_.report(unexpected_token, next_token.range, next_token.kind, permitted_tokens);
                synchronize();
                return context_.create<EmptyDeclarationSyntax>(next_token.range, SyntaxFlags::unknown);
        }
    }

    const VariableDeclarationSyntax &Parser::parse_variable_declaration(const Modifiers modifiers)
    {
        const auto start = expect(TokenKind::kw_var).range.start;

        auto is_mutable = match(TokenKind::kw_mut);
        auto name = parse_identifier();

        const TypeSyntax *type = nullptr;
        if (match(TokenKind::colon))
        {
            type = &parse_type();
        }

        if (match(TokenKind::semicolon))
        {
            return context_.create<VariableDeclarationSyntax>(name,
                                                              type,
                                                              is_mutable,
                                                              modifiers,
                                                              nullptr,
                                                              SourceRange{start, stream_.previous().range.end});
        }

        expect(TokenKind::equal);

        auto &initializer = parse_expression();

        expect(TokenKind::semicolon);

        return context_.create<VariableDeclarationSyntax>(name,
                                                          type,
                                                          is_mutable,
                                                          modifiers,
                                                          &initializer,
                                                          SourceRange{start, stream_.previous().range.end});
    }

    const FunctionDeclarationSyntax &Parser::parse_function_declaration(const Modifiers modifiers)
    {
        const auto start = expect(TokenKind::kw_func).range.start;

        auto name = parse_identifier();

        std::span<const Ref<const ParameterDeclarationSyntax>> parameters;
        if (match(TokenKind::lparen))
        {
            parameters = parse_parameter_list();
            expect(TokenKind::rparen);
        }
        else
        {
            static constexpr std::array permitted_tokens = {TokenKind::lparen};
            auto next = stream_.peek();
            context_.report(unexpected_token, next.range, next.kind, permitted_tokens);
        }

        const TypeSyntax *return_type = nullptr;
        if (match(TokenKind::arrow))
        {
            return_type = &parse_type();
        }

        static constexpr std::array permitted_tokens = {TokenKind::lbrace, TokenKind::big_arrow, TokenKind::semicolon};
        FunctionBodySyntax body;
        auto next = stream_.peek();
        switch (next.kind)
        {
            case TokenKind::lbrace:
                body = parse_block();
                break;
            case TokenKind::big_arrow:
                stream_.advance();
                body = parse_expression();
                break;
            case TokenKind::semicolon:
                // We're already initialized in the empty state
                break;
            default:
                context_.report(unexpected_token, next.range, next.kind, permitted_tokens);
                synchronize(false);
                break;
        }

        return context_.create<FunctionDeclarationSyntax>(name,
                                                          return_type,
                                                          parameters,
                                                          modifiers,
                                                          body,
                                                          SourceRange{start, stream_.previous().range.end});
    }

    std::span<const Ref<const ParameterDeclarationSyntax>> Parser::parse_parameter_list()
    {
        std::vector<Ref<const ParameterDeclarationSyntax>> parameters;

        auto next = stream_.peek();
        while (next.kind != TokenKind::rparen)
        {
            if (!parameters.empty())
            {
                expect(TokenKind::comma);
            }

            auto is_mutable = match(TokenKind::kw_mut);
            auto name = parse_identifier();

            expect(TokenKind::colon);
            auto &type = parse_type();
            parameters.emplace_back(context_.create<ParameterDeclarationSyntax>(
                name,
                type,
                is_mutable,
                SourceRange{next.range.start, stream_.previous().range.end}));

            next = stream_.peek();
        }

        return context_.copy(parameters);
    }

    IdentifierSyntax Parser::parse_identifier()
    {
        const auto identifier = expect(TokenKind::identifier);
        auto flags = SyntaxFlags::none;
        if (has_any_flags(identifier.flags, TokenFlags::synthetic))
        {
            flags |= SyntaxFlags::missing;
        }

        return IdentifierSyntax{
            .name = context_.slice(identifier.range),
            .flags = flags,
            .range = identifier.range,
        };
    }

    const TypeSyntax &Parser::parse_type()
    {
        const auto next = stream_.peek();
        for (auto [token, type] : built_in_types)
        {
            if (token == next.kind)
            {
                stream_.advance();
                return context_.create<BuiltInTypeSyntax>(type, next.range);
            }
        }

        const auto identifier = expect(TokenKind::identifier);
        if (has_any_flags(identifier.flags, TokenFlags::synthetic))
        {
            return context_.create<BuiltInTypeSyntax>(BuiltInType::void_type, next.range, SyntaxFlags::missing);
        }

        return context_.create<NamedTypeSyntax>(
            IdentifierSyntax{
                .name = context_.slice(identifier.range),
                .range = identifier.range,
            },
            identifier.range);
    }

    const BlockStatementSyntax &Parser::parse_block()
    {
        PooledVector<Ref<const StatementSyntax>> statements;
        auto start = expect(TokenKind::lbrace).range.start;

        while (stream_.peek().kind != TokenKind::rbrace)
        {
            statements.emplace_back(parse_statement());
        }

        auto end = expect(TokenKind::rbrace).range.end;
        return context_.create<BlockStatementSyntax>(context_.copy(statements), SourceRange{start, end});
    }

    const StatementSyntax &Parser::parse_statement()
    {
        auto next = stream_.peek();
        switch (next.kind)
        {
            case TokenKind::kw_var:
                {
                    auto &declaration = parse_variable_declaration();
                    return context_.create<VariableDeclarationStatementSyntax>(declaration, declaration.range());
                }
            case TokenKind::kw_return:
                return parse_return_statement();
            case TokenKind::lbrace:
                return parse_block();
            case TokenKind::semicolon:
                stream_.advance();
                context_.report(empty_statement, next.range);
                return context_.create<EmptyStatementSyntax>(next.range);
            default:
                return parse_expression_statement();
        }
    }

    const ExpressionStatementSyntax &Parser::parse_expression_statement()
    {
        auto &expression = parse_expression();
        expect(TokenKind::semicolon);
        return context_.create<ExpressionStatementSyntax>(
            expression,
            SourceRange{expression.range().start, stream_.previous().range.end});
    }

    const ReturnStatementSyntax &Parser::parse_return_statement()
    {
        auto start = expect(TokenKind::kw_return).range.start;
        auto &expression = parse_expression();
        expect(TokenKind::semicolon);
        return context_.create<ReturnStatementSyntax>(expression, SourceRange{start, stream_.previous().range.end});
    }

    const ExpressionSyntax &Parser::parse_expression()
    {
        return parse_expression(parse_prefix_expression(), 0);
    }

    const ExpressionSyntax &Parser::parse_expression(const ExpressionSyntax &left, const std::int32_t min_precedence)
    {
        auto next = stream_.peek();
        auto precedence = get_precedence(next.kind);
        auto lhs = &left;
        while (precedence >= min_precedence)
        {
            if (next.kind == TokenKind::question)
            {
                lhs = &parse_ternary_expression(*lhs);
            }
            else
            {
                const auto op = get_binary_operator(next.kind);
                stream_.advance();
                auto rhs = &parse_prefix_expression();
                next = stream_.peek();
                auto inner_precedence = get_precedence(next.kind);
                while (inner_precedence >= precedence)
                {
                    rhs = &parse_expression(*rhs, inner_precedence > precedence ? precedence + 1 : precedence);

                    next = stream_.peek();
                    inner_precedence = get_precedence(next.kind);
                }

                lhs = &context_.create<BinaryExpressionSyntax>(op,
                                                               *lhs,
                                                               *rhs,
                                                               SourceRange{lhs->range().start, rhs->range().end});
            }

            next = stream_.peek();
            precedence = get_precedence(next.kind);
        }

        return *lhs;
    }

    const ExpressionSyntax &Parser::parse_ternary_expression(const ExpressionSyntax &lhs)
    {
        expect(TokenKind::question);
        auto &true_expression = parse_expression();

        expect(TokenKind::colon);

        auto &false_expression = parse_expression();

        return context_.create<TernaryExpressionSyntax>(lhs,
                                                        true_expression,
                                                        false_expression,
                                                        SourceRange{lhs.range().start, false_expression.range().end});
    }

    const ExpressionSyntax &Parser::parse_primary_expression()
    {
        constexpr static std::array permitted_tokens = {
            TokenKind::kw_false,
            TokenKind::kw_true,
            TokenKind::integer,
            TokenKind::string_literal,
            TokenKind::identifier,
            TokenKind::lparen,
        };
        switch (const auto [kind, flags, range] = stream_.peek(); kind)
        {
            case TokenKind::kw_false:
                stream_.advance();
                return context_.create<LiteralExpressionSyntax>(LiteralSyntaxKind::bool_false, range);
            case TokenKind::kw_true:
                stream_.advance();
                return context_.create<LiteralExpressionSyntax>(LiteralSyntaxKind::bool_true, range);
            case TokenKind::integer:
                stream_.advance();
                return context_.create<LiteralExpressionSyntax>(LiteralSyntaxKind::integer, range);
            case TokenKind::string_literal:
                stream_.advance();
                return context_.create<LiteralExpressionSyntax>(LiteralSyntaxKind::string, range);
            case TokenKind::identifier:
                {
                    auto [name, flags, r] = parse_identifier();
                    return context_.create<IdentifierExpressionSyntax>(name, r, flags);
                }
            case TokenKind::lparen:
                {
                    stream_.advance();
                    auto &expression = parse_expression();
                    expect(TokenKind::rparen);
                    return expression;
                }
            default:
                context_.report(unexpected_token, range, kind, permitted_tokens);
                synchronize(false);
                return context_.create<LiteralExpressionSyntax>(LiteralSyntaxKind::bool_false,
                                                                range,
                                                                SyntaxFlags::unknown);
        }
    }

    const ExpressionSyntax &Parser::parse_prefix_expression()
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
                const auto start = expect(kind).range.start;
                auto &operand = parse_prefix_expression();
                return context_.create<UnaryExpressionSyntax>(op, operand, SourceRange{start, operand.range().end});
            }
        }

        return parse_postfix_expression();
    }

    const ExpressionSyntax &Parser::parse_postfix_expression()
    {
        auto &expression = parse_primary_expression();

        switch (const auto [kind, flags, range] = stream_.peek(); kind)
        {
            case TokenKind::lparen:
                {
                    stream_.advance();
                    auto arguments = parse_argument_list();
                    expect(TokenKind::rparen);
                    return context_.create<InvocationExpressionSyntax>(
                        expression,
                        arguments,
                        SourceRange{expression.range().start, stream_.previous().range.end});
                }
            case TokenKind::plus_plus:
                stream_.advance();
                return context_.create<UnaryExpressionSyntax>(
                    UnaryOperator::post_increment,
                    expression,
                    SourceRange{expression.range().start, stream_.previous().range.end});
            case TokenKind::minus_minus:
                stream_.advance();
                return context_.create<UnaryExpressionSyntax>(
                    UnaryOperator::post_decrement,
                    expression,
                    SourceRange{expression.range().start, stream_.previous().range.end});
            default:
                // Do nothing
                break;
        }

        return expression;
    }

    std::span<const Ref<const ExpressionSyntax>> Parser::parse_argument_list()
    {
        PooledVector<Ref<const ExpressionSyntax>> arguments;
        while (stream_.peek().kind != TokenKind::rparen)
        {
            if (!arguments.empty())
            {
                expect(TokenKind::comma);
            }

            arguments.emplace_back(parse_expression());
        }

        return context_.copy(arguments);
    }
} // namespace prism
