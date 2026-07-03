/**
 * @file parser.ixx
 * @author Francesco Corso
 * @date 6/24/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

export module prism.core.parser;

import std;
import prism.core.lexer.token;
import prism.core.parser.token_cursor;
import prism.core.ast;
import prism.core.source.source_file;
import prism.core.diagnostic;
import prism.core.token_stream;
import prism.core.context;
import prism.core.util;

namespace prism
{
    export class NoProgressException : public std::runtime_error
    {
        using std::runtime_error::runtime_error;
    };

    export class PRISM_CORE_API Parser
    {
      public:
        constexpr explicit Parser(CompilationContext &context) : context_{context}, stream_{context.source_file()}
        {
        }

        CompilationUnitSyntax parse_compilation_unit();

        const DeclarationSyntax &parse_declaration();

        const StatementSyntax &parse_statement();

        const ExpressionSyntax &parse_expression();

      private:
        bool match(TokenKind kind);
        Token expect(TokenKind kind);

        void synchronize(bool include_semicolon = true);
        bool is_next(std::span<const TokenKind> kinds);

        std::pair<Modifiers, bool> parse_modifiers();

        const VariableDeclarationSyntax &parse_variable_declaration(Modifiers modifiers = {});
        const FunctionDeclarationSyntax &parse_function_declaration(Modifiers modifiers);

        std::span<const Ref<const ParameterDeclarationSyntax>> parse_parameter_list();

        IdentifierSyntax parse_identifier();

        const TypeSyntax &parse_type();

        const BlockStatementSyntax &parse_block();

        const ExpressionStatementSyntax &parse_expression_statement();

        const ReturnStatementSyntax &parse_return_statement();

        const ExpressionSyntax &parse_expression(const ExpressionSyntax &left, std::int32_t min_precedence);

        const ExpressionSyntax &parse_ternary_expression(const ExpressionSyntax &lhs);

        const ExpressionSyntax &parse_primary_expression();

        const ExpressionSyntax &parse_prefix_expression();

        const ExpressionSyntax &parse_postfix_expression();

        std::span<const Ref<const ExpressionSyntax>> parse_argument_list();

        CompilationContext &context_;
        TokenStream stream_;
    };
} // namespace prism
