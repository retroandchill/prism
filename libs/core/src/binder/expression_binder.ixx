/**
 * @file expression_binder.ixx
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
export module prism.core:binder.expression_binder;

import :syntax.list;
import :semantic.semantic_model;
import :semantic.bound.bound_expression;
import :semantic.bound.bound_statement;

namespace prism
{
    class BoundNodeLookup;
    class SemanticLifetime;
    class DeclarationSyntax;
    class CompilationUnitSyntax;
    class DiagnosticBag;

    class ExpressionBinder final
    {
      public:
        explicit constexpr ExpressionBinder(const SemanticModel &semantic_model,
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

        const BoundStatement &bind_statement(const StatementSyntax &syntax, const TypeSymbol &return_type);
        const BoundBlock &bind_block(const BlockSyntax &syntax, const TypeSymbol &return_type);
        const BoundVariableDeclaration &bind_variable_declaration_statement(
            const VariableDeclarationStatementSyntax &syntax);
        const BoundExpressionStatement &bind_expression_statement(const ExpressionStatementSyntax &syntax);
        const BoundReturnStatement &bind_return_statement(const ReturnStatementSyntax &syntax,
                                                          const TypeSymbol &return_type);

        const BoundExpression &bind_expression(const ExpressionSyntax &syntax);
        const BoundLiteralExpression &bind_literal_expression(const LiteralExpressionSyntax &syntax);
        const BoundExpression &bind_identifier_expression(const IdentifierExpressionSyntax &syntax);
        const BoundBinaryExpression &bind_binary_expression(const BinaryExpressionSyntax &syntax);
        const BoundAssignmentExpression &bind_assignment_expression(const AssignmentExpressionSyntax &syntax);
        const BoundExpression &bind_prefix_expression(const PrefixExpressionSyntax &syntax);
        const BoundUnaryExpression &bind_postfix_expression(const PostfixExpressionSyntax &syntax);
        const BoundConditionalExpression &bind_ternary_expression(const TernaryExpressionSyntax &syntax);
        const BoundInvocationExpression &bind_invocation_expression(const InvocationExpressionSyntax &syntax);

        [[nodiscard]] const BoundExpression &add_conversion_if_necessary(const ExpressionSyntax &syntax,
                                                                         const BoundExpression &expression,
                                                                         const TypeSymbol &type) const;

        [[nodiscard]] ConstantValue evaluate_constant_expression(const SyntaxToken &token) const;
        [[nodiscard]] ConstantValue evaluate_numeric_expression(const IntegerLiteralData &data) const;
        [[nodiscard]] ConstantValue evaluate_numeric_expression(const FloatLiteralData &data) const;

        const SemanticModel &semantic_model_;
        DiagnosticBag &diagnostics_;
        BoundNodeLookup &lookup_;
        SemanticLifetime &lifetime_;
    };
} // namespace prism
