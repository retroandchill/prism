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
import prism.core.ast.statement_syntax;
import prism.core.ast.expression_syntax;
import prism.core.ast.common_syntax;
import prism.core.ast.type_syntax;
import prism.core.source.source_file;
import prism.core.ast.statement_syntax;
import prism.core.diagnostic;
import prism.core.token_stream;

namespace prism
{
    export struct ParseResult
    {
        CompilationUnitSyntax compilation_unit;
        std::vector<Diagnostic> diagnostics;
    };

    export class NoProgressException : public std::runtime_error
    {
        using std::runtime_error::runtime_error;
    };

    export class PRISM_CORE_API Parser
    {
      public:
        constexpr explicit Parser(const SourceFile &source_file, DiagnosticSink &diagnostic_sink)
            : source_file_{source_file}, stream_{source_file}, diagnostics_{diagnostic_sink}
        {
        }

        CompilationUnitSyntax parse_compilation_unit();
        DeclarationSyntax parse_declaration();
        StatementSyntax parse_statement();
        ExpressionSyntax parse_expression();

      private:
        bool match(TokenKind kind);
        Token expect(TokenKind kind);

        void synchronize(bool include_semicolon = true);
        bool is_next(std::span<const TokenKind> kinds);

        std::pair<Modifiers, bool> parse_modifiers();

        VariableDeclarationSyntax parse_variable_declaration(Modifiers modifiers = {});
        FunctionDeclarationSyntax parse_function_declaration(Modifiers modifiers);
        std::vector<ParameterDeclarationSyntax> parse_parameter_list();

        IdentifierSyntax parse_identifier();
        TypeSyntax parse_type();

        BlockSyntax parse_block();

        ExpressionStatementSyntax parse_expression_statement();
        ReturnStatementSyntax parse_return_statement();

        ExpressionSyntax parse_expression(ExpressionSyntax lhs, std::int32_t min_precedence);

        ExpressionSyntax parse_ternary_expression(ExpressionSyntax lhs);

        ExpressionSyntax parse_primary_expression();
        ExpressionSyntax parse_prefix_expression();
        ExpressionSyntax parse_postfix_expression();
        std::vector<ExpressionSyntax> parse_argument_list();

        LiteralSyntax parse_string_literal();

        const SourceFile &source_file_;
        TokenStream stream_;
        DiagnosticSink &diagnostics_;
    };
} // namespace prism
