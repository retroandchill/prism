/**
 * @file parser.cpp
 * @author Francesco Corso
 * @date 6/24/2026
 * @brief
 */
module prism.core.parser;
import prism.core.parser.token_cursor;
import prism.core.ast.statement_syntax;

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

    CompilationUnitSyntax Parser::parse_compilation_unit()
    {
        CompilationUnitSyntax compilation_unit;

        while (!cursor_.at_end())
        {
            compilation_unit.statements.push_back(parse_statement());
        }

        return compilation_unit;
    }

    StatementSyntax Parser::parse_statement()
    {
        if (cursor_.match(TokenKind::kw_var))
            return parse_variable_declaration();

        return parse_expression_statement();
    }

    VariableDeclarationSyntax Parser::parse_variable_declaration()
    {
        cursor_.advance();

        VariableDeclarationSyntax variable_declaration;

        if (cursor_.match(TokenKind::kw_mut))
        {
            cursor_.advance();
            variable_declaration.is_mutable = true;
        }

        variable_declaration.name = consume_identifier();

        if (cursor_.match(TokenKind::colon))
        {
            cursor_.advance();
            variable_declaration.type = parse_type();
        }

        if (cursor_.match(TokenKind::equal))
        {
            cursor_.advance();
            variable_declaration.initializer = parse_expression();
        }

        consume(TokenKind::semicolon);
        return variable_declaration;
    }

    ExpressionStatementSyntax Parser::parse_expression_statement()
    {
        ExpressionStatementSyntax result{parse_expression()};
        consume(TokenKind::semicolon);
        return result;
    }

    TypeSyntax Parser::parse_type()
    {
        const auto &[kind, range] = cursor_.current();
        if (kind == TokenKind::identifier)
        {
            return NamedTypeSyntax{consume_identifier()};
        }

        for (const auto &[keyword, type] : built_in_types)
        {
            if (kind == keyword)
            {
                cursor_.advance();
                return BuiltInTypeSyntax{type};
            }
        }

        throw std::runtime_error("Unexpected token");
    }

    std::unique_ptr<ExpressionSyntax> Parser::parse_expression()
    {
        return parse_binary_expression(0);
    }

    std::unique_ptr<ExpressionSyntax> Parser::parse_binary_expression(std::int32_t parent_precedence)
    {
        auto left = parse_primary();

        while (true)
        {
            const auto kind = cursor_.current().kind;

            const auto precedence = get_precedence(kind);

            if (precedence < parent_precedence)
            {
                break;
            }

            const auto op = get_binary_operator(cursor_.current().kind);
            cursor_.advance();

            auto right = parse_binary_expression(precedence + 1);

            left = std::make_unique<ExpressionSyntax>(BinaryExpressionSyntax{op, std::move(left), std::move(right)});
        }

        return left;
    }

    std::unique_ptr<ExpressionSyntax> Parser::parse_primary()
    {
        switch (const auto &[kind, range] = cursor_.current(); kind)
        {
            case TokenKind::kw_true:
                cursor_.advance();
                return std::make_unique<ExpressionSyntax>(LiteralSyntax{true});
            case TokenKind::kw_false:
                cursor_.advance();
                return std::make_unique<ExpressionSyntax>(LiteralSyntax{false});

            case TokenKind::number:
                cursor_.advance();
                // TODO: This always parses a double, we will change how numeric literals are parsed later
                return std::make_unique<ExpressionSyntax>(
                    LiteralSyntax{std::stod(source_file_.text().substr(range.start, range.length()))});

            case TokenKind::string_literal:
                cursor_.advance();
                return std::make_unique<ExpressionSyntax>(
                    LiteralSyntax{parse_string_literal(source_file_.text().substr(range.start, range.length()))});

            case TokenKind::identifier:
                return std::make_unique<ExpressionSyntax>(IdentifierSyntax{consume_identifier()});

            case TokenKind::lparen:
                {
                    cursor_.advance();

                    auto expr = parse_expression();

                    consume(TokenKind::rparen);
                    return expr;
                }
            default:
                throw std::runtime_error("Unexpected token");
        }
    }

    const Token &Parser::consume(const TokenKind kind)
    {
        auto &token = cursor_.current();
        if (token.kind != kind)
        {
            // TODO: We don't want to throw an exception since we can try to recover, but for now let's do that
            throw std::runtime_error("Unexpected token");
        }

        cursor_.advance();
        return token;
    }

    std::string Parser::consume_identifier()
    {
        const auto &[kind, range] = consume(TokenKind::identifier);
        return source_file_.text().substr(range.start, range.length());
    }
} // namespace prism
