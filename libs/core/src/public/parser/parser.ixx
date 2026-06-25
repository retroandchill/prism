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
import prism.core.ast.compilation_syntax;
import prism.core.lexer.token;
import prism.core.parser.token_cursor;
import prism.core.ast.statement_syntax;
import prism.core.ast.expression_syntax;
import prism.core.ast.type_syntax;
import prism.core.source.source_file;

namespace prism
{
    export class PRISM_CORE_API Parser
    {
      public:
        constexpr explicit Parser(const SourceFile &source_file, const std::span<const Token> tokens)
            : source_file_{source_file}, cursor_{tokens}
        {
        }

        CompilationUnitSyntax parse_compilation_unit();

      private:
        StatementSyntax parse_statement();
        VariableDeclarationSyntax parse_variable_declaration();
        ExpressionStatementSyntax parse_expression_statement();

        TypeSyntax parse_type();

        std::unique_ptr<ExpressionSyntax> parse_expression();
        std::unique_ptr<ExpressionSyntax> parse_binary_expression(std::int32_t parent_precedence = 0);

        std::unique_ptr<ExpressionSyntax> parse_primary();

        const Token &consume(TokenKind kind);
        std::string consume_identifier();

        const SourceFile &source_file_;
        TokenCursor cursor_;
    };
} // namespace prism
