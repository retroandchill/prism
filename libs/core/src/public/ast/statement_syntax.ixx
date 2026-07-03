/**
 * @file statement_syntax.ixx
 * @author Francesco Corso
 * @date 6/24/2026
 * @brief
 */
module;

#include "ast/macros.hpp"

export module prism.core.ast:statement_syntax;

import std;
import prism.core.util;
import prism.core.lexer.token;
import prism.core.source.source_file;
import :syntax_node;
import :identifier_syntax;
import :expression_syntax;

namespace prism
{
    export class StatementSyntax : public SyntaxNode
    {
      protected:
        StatementSyntax(SyntaxKind kind, const SourceRange &range, SyntaxFlags flags)
            : SyntaxNode(SyntaxCategory::statement, kind, range, flags)
        {
        }

        AST_NODE_BOILERPLATE(StatementSyntax)
    };

    export class VariableDeclarationStatementSyntax : public StatementSyntax
    {
      public:
        constexpr VariableDeclarationStatementSyntax(const class VariableDeclarationSyntax &declaration,
                                                     SourceRange range,
                                                     SyntaxFlags flags = SyntaxFlags::none)
            : StatementSyntax{SyntaxKind::variable_declaration, range, flags}, declaration_{declaration}
        {
        }

        UNOWNED_REF_PROPERTY(VariableDeclarationSyntax, declaration)
    };

    export class BlockStatementSyntax : public StatementSyntax
    {

      public:
        constexpr BlockStatementSyntax(std::span<const Ref<const StatementSyntax>> statements,
                                       const SourceRange range,
                                       const SyntaxFlags flags = SyntaxFlags::none)
            : StatementSyntax{SyntaxKind::block_statement, range, flags}, statements_{statements}
        {
        }

        VALUE_PROPERTY(std::span<const Ref<const StatementSyntax>>, statements)
    };

    export class ReturnStatementSyntax : public StatementSyntax
    {
      public:
        constexpr ReturnStatementSyntax(const ExpressionSyntax &expression,
                                        const SourceRange range,
                                        const SyntaxFlags flags = SyntaxFlags::none)
            : StatementSyntax{SyntaxKind::return_statement, range, flags}, expression_{expression}
        {
        }

        UNOWNED_REF_PROPERTY(ExpressionSyntax, expression)
    };

    export class ExpressionStatementSyntax : public StatementSyntax
    {
      public:
        constexpr ExpressionStatementSyntax(const ExpressionSyntax &expression,
                                            const SourceRange range,
                                            const SyntaxFlags flags = SyntaxFlags::none)
            : StatementSyntax{SyntaxKind::expression_statement, range, flags}, expression_{expression}
        {
        }

        UNOWNED_REF_PROPERTY(ExpressionSyntax, expression)
    };

    export class EmptyStatementSyntax : public StatementSyntax
    {
      public:
        constexpr explicit EmptyStatementSyntax(const SourceRange range, const SyntaxFlags flags = SyntaxFlags::none)
            : StatementSyntax{SyntaxKind::empty_statement, range, flags}
        {
        }
    };
} // namespace prism
