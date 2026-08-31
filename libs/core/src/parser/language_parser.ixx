/**
 * @file language_parser.ixx
 * @author Francesco Corso
 * @date 7/24/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:parser.language_parser;

import :parser.syntax_parser;
import :syntax.green.top_level;
import :diagnostics.syntax_info;
import :diagnostics.factories;

namespace prism
{
    struct NamespaceBody
    {
        GreenSyntaxList<GreenUsingDirective> using_directives;
        GreenSyntaxList<GreenDeclaration> members;
    };

    class LanguageParser final : public SyntaxParser
    {
      public:
        explicit constexpr LanguageParser(const std::string_view text) : SyntaxParser{text}
        {
        }

        template <std::derived_from<GreenNode> T>
        GreenPtr<T> consume_unexpected_tokens(GreenPtr<T> node)
        {
            auto &current_token = peek_token();
            if (current_token.kind() == SyntaxKind::eof_token)
                return std::move(node);

            GreenListBuilder<GreenToken> builder;
            while (current_token.kind() != SyntaxKind::eof_token)
            {
                builder.add(consume_token());
            }

            const auto list = std::move(builder).build();

            auto mutable_copy = node->clone();
            mutable_copy->add_diagnostic(
                SyntaxDiagnosticInfo{diagnostics::info::make_unexpected_token(list[0].to_string())});
            add_trailing_skipped_syntax(*mutable_copy, list.node()->shared_from_this());
            return mutable_copy;
        }

        [[nodiscard]] GreenPtr<GreenCompilationUnit> parse_compilation_unit();
        [[nodiscard]] GreenPtr<GreenDeclaration> parse_declaration();
        [[nodiscard]] GreenPtr<GreenStatement> parse_statement();
        [[nodiscard]] GreenPtr<GreenExpression> parse_expression();

      private:
        [[nodiscard]] NamespaceBody parse_namespace_body();
        template <std::predicate<const GreenToken &> Predicate>
        [[nodiscard]] NamespaceBody parse_namespace_body(Predicate &&predicate);
        [[nodiscard]] GreenPtr<GreenUsingDirective> parse_using_directive();
        [[nodiscard]] GreenSyntaxList<GreenToken> parse_modifiers();
        [[nodiscard]] GreenPtr<GreenNamespaceDeclaration> parse_namespace_declaration(
            GreenSyntaxList<GreenToken> modifiers);
        [[nodiscard]] GreenPtr<GreenVariableDeclaration> parse_variable_declaration(
            GreenSyntaxList<GreenToken> modifiers = {});
        [[nodiscard]] GreenPtr<GreenFunctionDeclaration> parse_function_declaration(
            GreenSyntaxList<GreenToken> modifiers);

        [[nodiscard]] Optional<GreenPtr<GreenTypeSpecifier>> parse_type_specifier();
        [[nodiscard]] GreenPtr<GreenTypeSpecifier> parse_required_type_specifier();
        [[nodiscard]] GreenPtr<GreenType> parse_type();
        [[nodiscard]] GreenPtr<GreenName> parse_name();
        [[nodiscard]] GreenPtr<GreenSimpleName> parse_simple_name();
        [[nodiscard]] Optional<GreenPtr<GreenInitializer>> parse_initializer();
        [[nodiscard]] GreenPtr<GreenParameterList> parse_parameter_list();

        [[nodiscard]] GreenSeparatedList<GreenParameter> parse_parameters();
        [[nodiscard]] GreenPtr<GreenExpressionBody> parse_expression_body();

        [[nodiscard]] GreenPtr<GreenReturnStatement> parse_return_statement();
        [[nodiscard]] Optional<GreenPtr<GreenVariableDeclarationStatement>> parse_variable_declaration_statement();
        [[nodiscard]] GreenPtr<GreenExpressionStatement> parse_expression_statement();
        [[nodiscard]] GreenPtr<GreenBlock> parse_block();
        [[nodiscard]] GreenPtr<GreenIfStatement> parse_if_statement();
        [[nodiscard]] GreenPtr<GreenWhileStatement> parse_while_statement();
        [[nodiscard]] GreenPtr<GreenLoopStatement> parse_loop_statement();
        [[nodiscard]] GreenPtr<GreenForStatement> parse_for_statement();
        [[nodiscard]] GreenPtr<GreenBreakStatement> parse_break_statement();
        [[nodiscard]] GreenPtr<GreenContinueStatement> parse_continue_statement();

        [[nodiscard]] GreenPtr<GreenExpression> parse_expression(GreenPtr<GreenExpression> left,
                                                                 std::int32_t min_precedence);
        [[nodiscard]] GreenPtr<GreenTernaryExpression> parse_ternary_expression(GreenPtr<GreenExpression> condition);
        [[nodiscard]] GreenPtr<GreenExpression> parse_primary_expression();
        [[nodiscard]] GreenPtr<GreenExpression> parse_prefix_expression();
        [[nodiscard]] GreenPtr<GreenExpression> parse_postfix_expression();
        [[nodiscard]] GreenPtr<GreenExpression> parse_parenthesized_expression();

        [[nodiscard]] GreenPtr<GreenArgumentList> parse_argument_list();

        [[nodiscard]] GreenPtr<GreenArgument> parse_argument();
        [[nodiscard]] Optional<GreenPtr<GreenNamedParameter>> parse_named_parameter();

        [[nodiscard]] Optional<GreenPtr<GreenElseClause>> parse_else_clause();
    };
} // namespace prism
