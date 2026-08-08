/**
 * @file expression_binder.ixx
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
export module prism.core:binder.expression_binder;

import :syntax.list;
import :semantic.semantic_model;

namespace prism
{
    class BlockSyntax;
    class ReturnStatementSyntax;
    class ExpressionStatementSyntax;
    class VariableDeclarationStatementSyntax;
    class BoundReturnStatement;
    class BoundExpressionStatement;
    class BoundVariableDeclaration;
    class BoundBlock;
    class SemanticLifetime;
    class BoundNodeLookup;
    class BoundStatement;
    class StatementSyntax;
    class ExpressionSyntax;
    class BoundExpression;
    class DeclarationSyntax;
    class FunctionDeclarationSyntax;
    class VariableDeclarationSyntax;
    class NamespaceDeclarationSyntax;
    class CompilationUnitSyntax;
    class DiagnosticBag;

    class ExpressionBinder final
    {
      public:
        explicit constexpr ExpressionBinder(const SemanticModel semantic_model,
                                            DiagnosticBag &diagnostics,
                                            BoundNodeLookup &lookup,
                                            SemanticLifetime &lifetime)
            : semantic_model_{semantic_model}, diagnostics_{diagnostics}, lookup_{lookup}, lifetime_{lifetime}
        {
        }

        void bind();

      private:
        void bind_declarations(SyntaxList<DeclarationSyntax> syntax);
        void bind_variable_declaration(const VariableDeclarationSyntax &syntax);
        void bind_function_declaration(const FunctionDeclarationSyntax &syntax);

        const BoundStatement &bind_statement(const StatementSyntax &syntax);
        const BoundBlock &bind_block(const BlockSyntax &syntax);
        const BoundVariableDeclaration &bind_variable_declaration_statement(
            const VariableDeclarationStatementSyntax &syntax);
        const BoundExpressionStatement &bind_expression_statement(const ExpressionStatementSyntax &syntax);
        const BoundReturnStatement &bind_return_statement(const ReturnStatementSyntax &syntax);

        const BoundExpression &bind_expression(const ExpressionSyntax &syntax);

        SemanticModel semantic_model_;
        DiagnosticBag &diagnostics_;
        BoundNodeLookup &lookup_;
        SemanticLifetime &lifetime_;
    };
} // namespace prism
