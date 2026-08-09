/**
 * @file expression_binder.cpp
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:binder.expression_binder.impl;

import :binder.expression_binder;
import :syntax.tree;
import :syntax.visit;
import :semantic.compilation;
import :symbols.function_symbol;
import :diagnostics.diagnostic_bag;

namespace prism
{
    void ExpressionBinder::bind()
    {
        auto &root = semantic_model_.tree().root();
        visit(root,
              Overload{[&](const CompilationUnitSyntax &syntax) { bind_declarations(syntax.members()); },
                       [](const SyntaxNode &)
                       {
                           throw InvalidStateException{"Unexpected root syntax kind. Can only bind a Compilation Unit"};
                       }});
    }

    void ExpressionBinder::bind_declarations(SyntaxList<DeclarationSyntax> syntax)
    {
        for (auto &declaration : syntax)
        {
            visit(declaration,
                  Overload{[&](const IncompleteDeclarationSyntax &)
                           {
                               // Do nothing
                           },
                           [&](const NamespaceDeclarationSyntax &ns) { bind_declarations(ns.members()); },
                           [&](const VariableDeclarationSyntax &variable) { bind_variable_declaration(variable); },
                           [&](const FunctionDeclarationSyntax &function)
                           {
                               bind_function_declaration(function);
                           }});
        }
    }

    void ExpressionBinder::bind_variable_declaration(const VariableDeclarationSyntax &syntax)
    {
        if (!syntax.initializer().has_value())
            return;

        auto &symbol = semantic_model_.get_declared_symbol(syntax).value();
        auto &initializer = bind_expression(syntax.initializer()->value());
        lookup_.add_variable_initializer(symbol, initializer);
    }

    void ExpressionBinder::bind_function_declaration(const FunctionDeclarationSyntax &syntax)
    {
        auto &symbol = semantic_model_.get_declared_symbol(syntax).value();
        if (syntax.expression_body().has_value())
        {
            auto *expression = &bind_expression(syntax.expression_body()->expression());
            if (symbol.returns_void())
            {
                const auto &body = lifetime_.create<BoundExpressionStatement>(*syntax.expression_body(), *expression);
                lookup_.add_function_body(symbol, body);
            }
            else
            {
                auto conversion =
                    semantic_model_.compilation().classify_conversion(expression->type(), symbol.return_type());
                if (!conversion.exists())
                {
                    diagnostics_.add(Diagnostic{
                        DiagnosticInfo::create<DiagnosticCode::no_conversion>(expression->type().to_display_string(),
                                                                              symbol.return_type().to_display_string()),
                        syntax.expression_body()->expression().location()});
                }
                else if (!conversion.is_identity())
                {
                    if (!conversion.is_implicit())
                    {
                        diagnostics_.add(Diagnostic{DiagnosticInfo::create<DiagnosticCode::conversion_is_explicit>(
                                                        expression->type().to_display_string(),
                                                        symbol.return_type().to_display_string()),
                                                    syntax.expression_body()->expression().location()});
                    }

                    expression = &lifetime_.create<BoundConversionExpression>(syntax.expression_body()->expression(),
                                                                              *expression,
                                                                              symbol.return_type(),
                                                                              conversion);
                }

                const auto &body = lifetime_.create<BoundReturnStatement>(*syntax.expression_body(), expression);
                lookup_.add_function_body(symbol, body);
            }
        }
        else if (syntax.body().has_value())
        {
            auto &body = bind_block(*syntax.body());
            // TODO: Control-flow analysis for the return statement
            lookup_.add_function_body(symbol, body);
        }
    }

    const BoundStatement &ExpressionBinder::bind_statement(const StatementSyntax &syntax)
    {
        return visit(syntax,
                     Overload{[&](const BlockSyntax &block) -> const BoundStatement & { return bind_block(block); },
                              [&](const ExpressionStatementSyntax &expression) -> const BoundStatement &
                              { return bind_expression_statement(expression); },
                              [&](const ReturnStatementSyntax &statement) -> const BoundStatement &
                              { return bind_return_statement(statement); },
                              [&](const VariableDeclarationStatementSyntax &variable) -> const BoundStatement &
                              { return bind_variable_declaration_statement(variable); },
                              [&](const EmptyStatementSyntax &) -> const BoundStatement &
                              {
                                  UNREACHABLE("We should guard against entering into this context");
                              }});
    }

    const BoundBlock &ExpressionBinder::bind_block(const BlockSyntax &syntax)
    {
        PooledVector<Ref<const BoundStatement>> statements;
        for (auto &statement : syntax.statements())
        {
            if (statement.is<EmptyStatementSyntax>())
                continue;

            statements.emplace_back(bind_statement(statement));
        }

        auto interned = lifetime_.copy_refs(statements);
        return lifetime_.create<BoundBlock>(syntax, interned);
    }

    const BoundExpressionStatement &ExpressionBinder::bind_expression_statement(const ExpressionStatementSyntax &syntax)
    {
        auto &expression = bind_expression(syntax.expression());
        return lifetime_.create<BoundExpressionStatement>(syntax, expression);
    }

    const BoundReturnStatement &ExpressionBinder::bind_return_statement(const ReturnStatementSyntax &syntax)
    {
        auto *expression = syntax.expression()
                               .transform([this](const ExpressionSyntax &e) -> auto & { return bind_expression(e); })
                               .value_ptr();

        return lifetime_.create<BoundReturnStatement>(syntax, expression);
    }

    const BoundExpression &ExpressionBinder::bind_expression(const ExpressionSyntax &syntax)
    {
        return visit(syntax,
                     Overload{[this](const LiteralExpressionSyntax &e) -> const BoundExpression &
                              { return bind_literal_expression(e); },
                              [this](const IdentifierExpressionSyntax &e) -> const BoundExpression &
                              { return bind_identifier_expression(e); },
                              [this](const ParenthesizedExpressionSyntax &e) -> const BoundExpression &
                              { return bind_expression(e.expression()); },
                              [this](const BinaryExpressionSyntax &e) -> const BoundExpression &
                              { return bind_binary_expression(e); },
                              [this](const AssignmentExpressionSyntax &e) -> const BoundExpression &
                              { return bind_assignment_expression(e); },
                              [this](const PrefixExpressionSyntax &e) -> const BoundExpression &
                              { return bind_prefix_expression(e); },
                              [this](const PostfixExpressionSyntax &e) -> const BoundExpression &
                              { return bind_postfix_expression(e); },
                              [this](const TernaryExpressionSyntax &e) -> const BoundExpression &
                              { return bind_ternary_expression(e); },
                              [this](const InvocationExpressionSyntax &e) -> const BoundExpression &
                              {
                                  return bind_invocation_expression(e);
                              }});
    }
} // namespace prism
