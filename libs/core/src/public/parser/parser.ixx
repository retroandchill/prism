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
        constexpr explicit Parser(const SourceFile &source_file) : source_file_{source_file}, stream_{source_file}
        {
        }

        ParseResult parse();

      private:
        bool match(TokenKind kind);
        Token expect(TokenKind kind);

        void synchronize(bool include_semicolon = true);
        bool is_next(std::span<const TokenKind> kinds);

        DeclarationSyntax parse_declaration();
        VariableDeclarationSyntax parse_variable_declaration();
        FunctionDeclarationSyntax parse_function_declaration();

        TypeSyntax parse_type();

        ExpressionSyntax parse_expression();

        const SourceFile &source_file_;
        TokenStream stream_;
        std::vector<Diagnostic> diagnostics_;
    };
} // namespace prism
