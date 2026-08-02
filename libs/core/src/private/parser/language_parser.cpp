/**
 * @file language_parser.cpp
 * @author Francesco Corso
 * @date 7/24/2026
 * @brief
 */
module prism.core:parser.language_parser.impl;

import :parser.language_parser;
import :util.enum_set;

namespace prism
{

    namespace
    {
        using ModifierKinds = EnumSet<SyntaxKind, SyntaxKind::extern_keyword>;

        using BuildInTypeKinds = EnumSet<SyntaxKind,
                                         SyntaxKind::void_keyword,
                                         SyntaxKind::bool_keyword,
                                         SyntaxKind::i8_keyword,
                                         SyntaxKind::i16_keyword,
                                         SyntaxKind::i32_keyword,
                                         SyntaxKind::i64_keyword,
                                         SyntaxKind::i128_keyword,
                                         SyntaxKind::isize_keyword,
                                         SyntaxKind::u8_keyword,
                                         SyntaxKind::u16_keyword,
                                         SyntaxKind::u32_keyword,
                                         SyntaxKind::u64_keyword,
                                         SyntaxKind::u128_keyword,
                                         SyntaxKind::usize_keyword,
                                         SyntaxKind::f16_keyword,
                                         SyntaxKind::f32_keyword,
                                         SyntaxKind::f64_keyword,
                                         SyntaxKind::char_keyword,
                                         SyntaxKind::char_keyword,
                                         SyntaxKind::char16_keyword,
                                         SyntaxKind::rune_keyword,
                                         SyntaxKind::str_keyword>;

        using AssignmentOperatorKinds = EnumSet<SyntaxKind,
                                                SyntaxKind::equal_token,
                                                SyntaxKind::plus_equal_token,
                                                SyntaxKind::minus_equal_token,
                                                SyntaxKind::star_equal_token,
                                                SyntaxKind::slash_equal_token,
                                                SyntaxKind::percent_equal_token,
                                                SyntaxKind::less_less_equal_token,
                                                SyntaxKind::greater_greater_equal_token,
                                                SyntaxKind::greater_greater_greater_equal_token,
                                                SyntaxKind::amp_equal_token,
                                                SyntaxKind::pipe_equal_token,
                                                SyntaxKind::caret_equal_token>;

        using PrefixOperatorKinds = EnumSet<SyntaxKind,
                                            SyntaxKind::plus_token,
                                            SyntaxKind::minus_token,
                                            SyntaxKind::bang_token,
                                            SyntaxKind::tilde_token,
                                            SyntaxKind::plus_plus_token,
                                            SyntaxKind::minus_minus_token,
                                            SyntaxKind::amp_token,
                                            SyntaxKind::star_token>;

        using PostfixOperatorKinds = EnumSet<SyntaxKind, SyntaxKind::plus_plus_token, SyntaxKind::minus_minus_token>;

        [[nodiscard]] constexpr bool is_modifier(const SyntaxKind kind)
        {
            return ModifierKinds::contains(kind);
        }

        [[nodiscard]] constexpr bool is_built_in_type(const SyntaxKind kind)
        {

            return BuildInTypeKinds::contains(kind);
        }

        [[nodiscard]] constexpr std::int32_t get_operator_precedence(const SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind::star_token:
                case SyntaxKind::slash_token:
                case SyntaxKind::percent_token:
                    return 140;
                case SyntaxKind::plus_token:
                case SyntaxKind::minus_token:
                    return 130;
                case SyntaxKind::less_less_token:
                case SyntaxKind::greater_greater_token:
                case SyntaxKind::greater_greater_greater_token:
                    return 120;
                case SyntaxKind::less_token:
                case SyntaxKind::greater_token:
                case SyntaxKind::less_equal_token:
                case SyntaxKind::greater_equal_token:
                    return 110;
                case SyntaxKind::spaceship_token:
                    return 100;
                case SyntaxKind::equal_equal_token:
                case SyntaxKind::bang_equal_token:
                    return 90;
                case SyntaxKind::amp_token:
                    return 80;
                case SyntaxKind::caret_token:
                    return 70;
                case SyntaxKind::pipe_token:
                    return 60;
                case SyntaxKind::amp_amp_token:
                    return 50;
                case SyntaxKind::pipe_pipe_token:
                    return 40;
                case SyntaxKind::question_question_token:
                    return 30;
                case SyntaxKind::question_token:
                    return 20;
                case SyntaxKind::equal_token:
                case SyntaxKind::plus_equal_token:
                case SyntaxKind::minus_equal_token:
                case SyntaxKind::star_equal_token:
                case SyntaxKind::slash_equal_token:
                case SyntaxKind::percent_equal_token:
                case SyntaxKind::less_less_equal_token:
                case SyntaxKind::greater_greater_equal_token:
                case SyntaxKind::greater_greater_greater_equal_token:
                case SyntaxKind::amp_equal_token:
                case SyntaxKind::pipe_equal_token:
                case SyntaxKind::caret_equal_token:
                    return 10;
                default:
                    return -1;
            }
        }

        [[nodiscard]] constexpr bool is_assignment_operator(const SyntaxKind kind) noexcept
        {
            return AssignmentOperatorKinds::contains(kind);
        }

        [[nodiscard]] constexpr bool is_prefix_operator(const SyntaxKind kind) noexcept
        {
            return PrefixOperatorKinds::contains(kind);
        }

        [[nodiscard]] constexpr bool is_postfix_operator(const SyntaxKind kind) noexcept
        {
            return PostfixOperatorKinds::contains(kind);
        }
    } // namespace

    GreenPtr<GreenCompilationUnit> LanguageParser::parse_compilation_unit()
    {
        auto [usings, members] = parse_namespace_body();
        return make_ref_counted<const GreenCompilationUnit>(std::move(usings), std::move(members));
    }

    GreenPtr<GreenDeclaration> LanguageParser::parse_declaration()
    {
        if (peek_token().kind() == SyntaxKind::namespace_keyword)
        {
            return parse_namespace_declaration();
        }

        auto modifiers = parse_modifiers();
        switch (auto &next = peek_token(); next.kind())
        {
            case SyntaxKind::var_keyword:
                return parse_variable_declaration(std::move(modifiers));
            case SyntaxKind::func_keyword:
                return parse_function_declaration(std::move(modifiers));
            default:
                {
                    // TODO: Add a diagnostic
                    return make_ref_counted<GreenIncompleteDeclaration>(std::move(modifiers));
                }
        }
    }

    GreenPtr<GreenStatement> LanguageParser::parse_statement()
    {
        switch (auto &next = peek_token(); next.kind())
        {
            case SyntaxKind::var_keyword:
                {
                    auto declaration = parse_variable_declaration();
                    return make_ref_counted<GreenVariableDeclarationStatement>(std::move(declaration));
                }
            case SyntaxKind::return_keyword:
                return parse_return_statement();
            case SyntaxKind::open_brace_token:
                return parse_block();
            case SyntaxKind::semicolon_token:
                {
                    auto semicolon = consume_token();
                    return make_ref_counted<GreenEmptyStatement>(std::move(semicolon));
                }
            default:
                return parse_expression_statement();
        }
    }

    GreenPtr<GreenExpression> LanguageParser::parse_expression()
    {
        return parse_expression(parse_prefix_expression(), 0);
    }

    NamespaceBody LanguageParser::parse_namespace_body()
    {
        return parse_namespace_body([](auto &&) { return true; });
    }

    template <std::predicate<const GreenToken &> Predicate>
    NamespaceBody LanguageParser::parse_namespace_body(Predicate &&predicate)
    {
        GreenListBuilder<GreenUsingDirective> using_directives;
        while (!at_end() && peek_token().kind() == SyntaxKind::using_keyword)
        {
            using_directives.add(parse_using_directive());
        }

        GreenListBuilder<GreenDeclaration> members;
        while (!at_end() && std::invoke(predicate, peek_token()))
        {
            members.add(parse_declaration());
        }

        return NamespaceBody{.using_directives = std::move(using_directives).build(),
                             .members = std::move(members).build()};
    }

    GreenPtr<GreenUsingDirective> LanguageParser::parse_using_directive()
    {
        auto using_keyword = expect_token(SyntaxKind::using_keyword);
        auto name = parse_name();
        auto semicolon = expect_token(SyntaxKind::semicolon_token);
        return make_ref_counted<const GreenUsingDirective>(std::move(using_keyword),
                                                           std::move(name),
                                                           std::move(semicolon));
    }

    GreenSyntaxList<GreenToken> LanguageParser::parse_modifiers()
    {
        GreenListBuilder<GreenToken> builder;
        while (!at_end())
        {
            if (auto &next = peek_token(); is_modifier(next.kind()))
            {
                builder.add(consume_token());
            }
            else
            {
                break;
            }
        }

        return std::move(builder).build();
    }

    GreenPtr<GreenNamespaceDeclaration> LanguageParser::parse_namespace_declaration()
    {
        auto namespace_keyword = expect_token(SyntaxKind::namespace_keyword);
        auto identifier = parse_name();

        if (auto semicolon = match_token(SyntaxKind::semicolon_token); semicolon.has_value())
        {
            auto [usings, members] = parse_namespace_body();
            return make_ref_counted<GreenFileScopedNamespaceDeclaration>(GreenSyntaxList<GreenToken>{},
                                                                         std::move(namespace_keyword),
                                                                         std::move(identifier),
                                                                         *std::move(semicolon),
                                                                         std::move(usings),
                                                                         std::move(members));
        }
        else
        {
            auto open_brace = expect_token(SyntaxKind::open_brace_token);
            auto [usings, members] = parse_namespace_body([](const GreenToken &token)
                                                          { return token.kind() != SyntaxKind::close_brace_token; });
            auto close_brace = expect_token(SyntaxKind::close_brace_token);
            return make_ref_counted<GreenBlockNamespaceDeclaration>(GreenSyntaxList<GreenToken>{},
                                                                    std::move(namespace_keyword),
                                                                    std::move(identifier),
                                                                    std::move(open_brace),
                                                                    std::move(usings),
                                                                    std::move(members),
                                                                    std::move(close_brace));
        }
    }

    GreenPtr<GreenVariableDeclaration> LanguageParser::parse_variable_declaration(GreenSyntaxList<GreenToken> modifiers)
    {
        auto var_keyword = expect_token(SyntaxKind::var_keyword);

        auto mut_keyword = match_token(SyntaxKind::mut_keyword);
        auto identifier = expect_token(SyntaxKind::identifier_token);
        auto type_specifier = parse_type_specifier();
        auto initializer = parse_initializer();
        auto semicolon = expect_token(SyntaxKind::semicolon_token);

        return make_ref_counted<const GreenVariableDeclaration>(std::move(modifiers),
                                                                std::move(var_keyword),
                                                                std::move(mut_keyword).value_or_default(),
                                                                std::move(identifier),
                                                                std::move(type_specifier).value_or_default(),
                                                                std::move(initializer).value_or_default(),
                                                                std::move(semicolon));
    }

    GreenPtr<GreenFunctionDeclaration> LanguageParser::parse_function_declaration(GreenSyntaxList<GreenToken> modifiers)
    {
        auto func_keyword = expect_token(SyntaxKind::func_keyword);
        auto name = expect_token(SyntaxKind::identifier_token);
        auto parameters = parse_parameter_list();
        auto return_type = parse_type_specifier();

        GreenPtr<GreenBlock> block;
        GreenPtr<GreenExpressionBody> expression_body;
        GreenPtr<GreenToken> semicolon;
        auto &next = peek_token();
        switch (next.kind())
        {
            case SyntaxKind::open_brace_token:
                block = parse_block();
                break;
            case SyntaxKind::arrow_token:
                expression_body = parse_expression_body();
                semicolon = expect_token(SyntaxKind::semicolon_token);
                break;
            default:
                semicolon = expect_token(SyntaxKind::semicolon_token);
                break;
        }

        return make_ref_counted<const GreenFunctionDeclaration>(std::move(modifiers),
                                                                std::move(func_keyword),
                                                                std::move(name),
                                                                std::move(parameters),
                                                                std::move(return_type).value_or_default(),
                                                                std::move(block),
                                                                std::move(expression_body),
                                                                std::move(semicolon));
    }

    Optional<GreenPtr<GreenTypeSpecifier>> LanguageParser::parse_type_specifier()
    {
        auto colon = match_token(SyntaxKind::colon_token);
        if (!colon.has_value())
            return std::nullopt;

        auto type = parse_type();
        return make_ref_counted<GreenTypeSpecifier>(*std::move(colon), std::move(type));
    }

    GreenPtr<GreenTypeSpecifier> LanguageParser::parse_required_type_specifier()
    {
        auto colon = expect_token(SyntaxKind::colon_token);
        auto type = parse_type();
        return make_ref_counted<const GreenTypeSpecifier>(std::move(colon), std::move(type));
    }

    GreenPtr<GreenType> LanguageParser::parse_type()
    {
        if (auto &next = peek_token(); is_built_in_type(next.kind()))
        {
            return make_ref_counted<GreenPredefinedType>(consume_token());
        }

        return make_ref_counted<GreenNamedType>(parse_name());
    }

    GreenPtr<GreenName> LanguageParser::parse_name()
    {
        GreenPtr<GreenName> name = parse_simple_name();

        while (!at_end())
        {
            auto separator = match_token(SyntaxKind::double_colon_token);
            if (!separator.has_value())
                break;

            auto right = parse_simple_name();
            name = make_ref_counted<GreenQualifiedName>(std::move(name), *std::move(separator), std::move(right));
        }

        return name;
    }

    GreenPtr<GreenSimpleName> LanguageParser::parse_simple_name()
    {
        auto identifier = expect_token(SyntaxKind::identifier_token);
        return make_ref_counted<GreenSimpleName>(std::move(identifier));
    }

    Optional<GreenPtr<GreenInitializer>> LanguageParser::parse_initializer()
    {
        auto equal = match_token(SyntaxKind::equal_token);
        if (!equal.has_value())
            return std::nullopt;

        auto expression = parse_expression();
        return make_ref_counted<GreenInitializer>(*std::move(equal), std::move(expression));
    }

    GreenPtr<GreenParameterList> LanguageParser::parse_parameter_list()
    {
        auto open_paren = expect_token(SyntaxKind::open_paren_token);
        auto param_list = parse_parameters();
        auto close_paren = expect_token(SyntaxKind::close_paren_token);
        return make_ref_counted<const GreenParameterList>(std::move(open_paren),
                                                          std::move(param_list),
                                                          std::move(close_paren));
    }

    GreenSeparatedList<GreenParameter> LanguageParser::parse_parameters()
    {
        GreenSeparatedListBuilder<GreenParameter> builder;

        auto *next = &peek_token();
        while (next->kind() != SyntaxKind::close_paren_token && !at_end())
        {
            if (builder.size() > 0)
            {
                builder.add_separator(expect_token(SyntaxKind::comma_token));
            }

            auto mut_keyword = match_token(SyntaxKind::mut_keyword);
            auto name = expect_token(SyntaxKind::identifier_token);
            auto type = parse_required_type_specifier();
            auto default_value = parse_initializer();
            builder.add_item(make_ref_counted<const GreenParameter>(std::move(mut_keyword).value_or_default(),
                                                                    std::move(name),
                                                                    std::move(type),
                                                                    std::move(default_value).value_or_default()));

            next = &peek_token();
        }

        return std::move(builder).build();
    }

    GreenPtr<GreenExpressionBody> LanguageParser::parse_expression_body()
    {
        auto arrow = expect_token(SyntaxKind::arrow_token);
        auto expression = parse_expression();
        return make_ref_counted<const GreenExpressionBody>(std::move(arrow), std::move(expression));
    }

    GreenPtr<GreenReturnStatement> LanguageParser::parse_return_statement()
    {
        auto return_keyword = expect_token(SyntaxKind::return_keyword);
        auto expression = parse_expression();
        auto semicolon = expect_token(SyntaxKind::semicolon_token);
        return make_ref_counted<const GreenReturnStatement>(std::move(return_keyword),
                                                            std::move(expression),
                                                            std::move(semicolon));
    }

    GreenPtr<GreenExpressionStatement> LanguageParser::parse_expression_statement()
    {
        auto expression = parse_expression();
        auto semicolon = expect_token(SyntaxKind::semicolon_token);
        return make_ref_counted<const GreenExpressionStatement>(std::move(expression), std::move(semicolon));
    }

    GreenPtr<GreenBlock> LanguageParser::parse_block()
    {
        GreenListBuilder<GreenStatement> builder;

        auto start = expect_token(SyntaxKind::open_brace_token);

        while (!at_end() && peek_token().kind() != SyntaxKind::close_brace_token)
        {
            builder.add(parse_statement());
        }

        auto end = expect_token(SyntaxKind::close_brace_token);
        return make_ref_counted<const GreenBlock>(std::move(start), std::move(builder).build(), std::move(end));
    }

    GreenPtr<GreenExpression> LanguageParser::parse_expression(GreenPtr<GreenExpression> left,
                                                               std::int32_t min_precedence)
    {
        auto *next = &peek_token();
        auto precedence = get_operator_precedence(next->kind());
        while (precedence >= min_precedence)
        {
            if (next->kind() == SyntaxKind::question_token)
            {
                left = parse_ternary_expression(std::move(left));
            }
            else
            {
                auto op = consume_token();
                auto right = parse_prefix_expression();
                next = &peek_token();
                auto inner_precedence = get_operator_precedence(next->kind());
                while (inner_precedence >= precedence)
                {
                    right =
                        parse_expression(std::move(right), inner_precedence > precedence ? precedence + 1 : precedence);

                    next = &peek_token();
                    inner_precedence = get_operator_precedence(next->kind());
                }

                if (is_assignment_operator(op->kind()))
                {
                    left =
                        make_ref_counted<GreenAssignmentExpression>(std::move(left), std::move(op), std::move(right));
                }
                else
                {
                    left = make_ref_counted<GreenBinaryExpression>(std::move(left), std::move(op), std::move(right));
                }
            }

            next = &peek_token();
            precedence = get_operator_precedence(next->kind());
        }

        return std::move(left);
    }
    GreenPtr<GreenTernaryExpression> LanguageParser::parse_ternary_expression(GreenPtr<GreenExpression> condition)
    {
        auto question = expect_token(SyntaxKind::question_token);
        auto true_expression = parse_expression();
        auto colon = expect_token(SyntaxKind::colon_token);
        auto false_expression = parse_expression();
        return make_ref_counted<const GreenTernaryExpression>(std::move(condition),
                                                              std::move(question),
                                                              std::move(true_expression),
                                                              std::move(colon),
                                                              std::move(false_expression));
    }

    GreenPtr<GreenExpression> LanguageParser::parse_primary_expression()
    {
        switch (auto &next = peek_token(); next.kind())
        {
            case SyntaxKind::false_keyword:
            case SyntaxKind::true_keyword:
            case SyntaxKind::numeric_literal_token:
            case SyntaxKind::character_literal_token:
            case SyntaxKind::string_literal_token:
                return make_ref_counted<GreenLiteralExpression>(consume_token());
            case SyntaxKind::open_paren_token:
                return parse_parenthesized_expression();
            default:
                {
                    auto identifier = parse_name();
                    return make_ref_counted<GreenIdentifierExpression>(std::move(identifier));
                }
        }
    }

    GreenPtr<GreenExpression> LanguageParser::parse_prefix_expression()
    {
        auto &next = peek_token();
        if (!is_prefix_operator(next.kind()))
            return parse_postfix_expression();

        auto op = consume_token();
        auto operand = parse_prefix_expression();
        return make_ref_counted<GreenPrefixExpression>(std::move(op), std::move(operand));
    }

    GreenPtr<GreenExpression> LanguageParser::parse_postfix_expression()
    {
        auto expression = parse_primary_expression();
        auto &next = peek_token();
        const auto kind = next.kind();
        if (kind == SyntaxKind::open_paren_token)
        {
            auto arguments = parse_argument_list();
            return make_ref_counted<GreenInvocationExpression>(std::move(expression), std::move(arguments));
        }

        if (is_postfix_operator(kind))
        {
            auto op = consume_token();
            return make_ref_counted<GreenPostfixExpression>(std::move(expression), std::move(op));
        }

        return std::move(expression);
    }

    GreenPtr<GreenExpression> LanguageParser::parse_parenthesized_expression()
    {
        auto open_paren = expect_token(SyntaxKind::open_paren_token);
        auto expression = parse_expression();
        auto close_paren = expect_token(SyntaxKind::close_paren_token);
        return make_ref_counted<GreenParenthesizedExpression>(std::move(open_paren),
                                                              std::move(expression),
                                                              std::move(close_paren));
    }

    GreenPtr<GreenArgumentList> LanguageParser::parse_argument_list()
    {
        auto open_paren = expect_token(SyntaxKind::open_paren_token);

        GreenSeparatedListBuilder<GreenArgument> builder;
        auto *next = &peek_token();
        while (!at_end() && next->kind() != SyntaxKind::close_paren_token)
        {
            if (builder.size() > 0)
            {
                builder.add_separator(expect_token(SyntaxKind::comma_token));
            }

            builder.add_item(parse_argument());
        }

        auto close_paren = expect_token(SyntaxKind::close_paren_token);
        return make_ref_counted<const GreenArgumentList>(std::move(open_paren),
                                                         std::move(builder).build(),
                                                         std::move(close_paren));
    }

    GreenPtr<GreenArgument> LanguageParser::parse_argument()
    {
        auto named_param = parse_named_parameter();
        auto expression = parse_expression();
        return make_ref_counted<const GreenArgument>(std::move(named_param).value_or_default(), std::move(expression));
    }

    Optional<GreenPtr<GreenNamedParameter>> LanguageParser::parse_named_parameter()
    {
        auto &next = peek_token();
        GreenPtr<GreenNamedParameter> named_param = nullptr;
        if (next.kind() != SyntaxKind::identifier_token || peek_token(2).kind() != SyntaxKind::colon_token)
        {
            return std::nullopt;
        }

        auto identifier = consume_token();
        auto colon = consume_token();
        return make_ref_counted<GreenNamedParameter>(std::move(identifier), std::move(colon));
    }
} // namespace prism
