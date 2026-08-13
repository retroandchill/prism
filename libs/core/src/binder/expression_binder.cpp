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
import :symbols.source;
import :symbols.error;
import :binder.binding_helpers;

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
                expression = &add_conversion_if_necessary(syntax.expression_body()->expression(),
                                                          *expression,
                                                          symbol.return_type());
                const auto &body = lifetime_.create<BoundReturnStatement>(*syntax.expression_body(), expression);
                lookup_.add_function_body(symbol, body);
            }
        }
        else if (syntax.body().has_value())
        {
            auto &body = bind_block(*syntax.body(), symbol.return_type());
            // TODO: Control-flow analysis for the return statement
            lookup_.add_function_body(symbol, body);
        }
    }

    const BoundStatement &ExpressionBinder::bind_statement(const StatementSyntax &syntax, const TypeSymbol &return_type)
    {
        return visit(syntax,
                     Overload{[&](const BlockSyntax &block) -> const BoundStatement &
                              { return bind_block(block, return_type); },
                              [&](const ExpressionStatementSyntax &expression) -> const BoundStatement &
                              { return bind_expression_statement(expression); },
                              [&](const ReturnStatementSyntax &statement) -> const BoundStatement &
                              { return bind_return_statement(statement, return_type); },
                              [&](const VariableDeclarationStatementSyntax &variable) -> const BoundStatement &
                              { return bind_variable_declaration_statement(variable); },
                              [&](const EmptyStatementSyntax &) -> const BoundStatement &
                              {
                                  UNREACHABLE("We should guard against entering into this context");
                              }});
    }

    const BoundBlock &ExpressionBinder::bind_block(const BlockSyntax &syntax, const TypeSymbol &return_type)
    {
        PooledVector<Ref<const BoundStatement>> statements;
        for (auto &statement : syntax.statements())
        {
            if (statement.is<EmptyStatementSyntax>())
                continue;

            statements.emplace_back(bind_statement(statement, return_type));
        }

        auto interned = lifetime_.copy_refs(statements);
        return lifetime_.create<BoundBlock>(syntax, interned);
    }

    const BoundVariableDeclaration &ExpressionBinder::bind_variable_declaration_statement(
        const VariableDeclarationStatementSyntax &syntax)
    {
        auto &declaration = syntax.declaration();
        auto &symbol = semantic_model_.get_local_variable(syntax);

        auto initializer = declaration.initializer().transform([this](const InitializerSyntax &i) -> auto &
                                                               { return bind_expression(i.value()); });
        if (initializer.has_value())
        {
            if (declaration.type().has_value())
            {
                symbol.type_ = &resolve_type(declaration.type()->type(), semantic_model_.compilation(), diagnostics_);
                initializer =
                    add_conversion_if_necessary(declaration.initializer()->value(), *initializer, symbol.type());
            }
            else
            {
                symbol.type_ = &initializer->type();
            }
        }
        else
        {
            if (!declaration.type().has_value())
            {
                diagnostics_.add(
                    Diagnostic{DiagnosticInfo::create<DiagnosticCode::expected_type_specifier>(), syntax.location()});
                symbol.type_ = &unnamed_error_type;
            }
            else
            {
                symbol.type_ = &resolve_type(declaration.type()->type(), semantic_model_.compilation(), diagnostics_);
            }
        }

        return lifetime_.create<BoundVariableDeclaration>(syntax, symbol, initializer.value_ptr());
    }

    const BoundExpressionStatement &ExpressionBinder::bind_expression_statement(const ExpressionStatementSyntax &syntax)
    {
        auto &expression = bind_expression(syntax.expression());
        return lifetime_.create<BoundExpressionStatement>(syntax, expression);
    }

    const BoundReturnStatement &ExpressionBinder::bind_return_statement(const ReturnStatementSyntax &syntax,
                                                                        const TypeSymbol &return_type)
    {
        auto *expression = syntax.expression()
                               .transform([&](const ExpressionSyntax &e) -> auto &
                                          { return add_conversion_if_necessary(e, bind_expression(e), return_type); })
                               .value_ptr();

        return lifetime_.create<BoundReturnStatement>(syntax, expression);
    }

    const BoundExpression &ExpressionBinder::bind_expression(const ExpressionSyntax &syntax)
    {
        return visit(syntax,
                     Overload{[&](const LiteralExpressionSyntax &e) -> const BoundExpression &
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

    const BoundLiteralExpression &ExpressionBinder::bind_literal_expression(const LiteralExpressionSyntax &syntax)
    {
        const auto token = syntax.value();
        auto value = evaluate_constant_expression(token);
        auto &type = semantic_model_.compilation().get_special_type(value.special_type());
        return lifetime_.create<BoundLiteralExpression>(syntax, value, type);
    }

    const BoundExpression &ExpressionBinder::add_conversion_if_necessary(const ExpressionSyntax &syntax,
                                                                         const BoundExpression &expression,
                                                                         const TypeSymbol &type) const
    {
        auto conversion = semantic_model_.compilation().classify_conversion(expression.type(), type);
        if (!conversion.exists())
        {
            diagnostics_.add(
                Diagnostic{DiagnosticInfo::create<DiagnosticCode::no_conversion>(expression.type().to_display_string(),
                                                                                 type.to_display_string()),
                           syntax.location()});
        }
        else if (!conversion.is_identity())
        {
            if (!conversion.is_implicit())
            {
                diagnostics_.add(Diagnostic{DiagnosticInfo::create<DiagnosticCode::conversion_is_explicit>(
                                                expression.type().to_display_string(),
                                                type.to_display_string()),
                                            syntax.location()});
            }

            return lifetime_.create<BoundConversionExpression>(syntax, expression, type, conversion);
        }

        return expression;
    }

    ConstantValue ExpressionBinder::evaluate_constant_expression(const SyntaxToken &token) const
    {
        if (auto bool_value = token.try_get_value<bool>(); bool_value.has_value())
        {
            return ConstantValue::boolean(*bool_value);
        }

        if (const auto numeric_value = token.try_get_value<IntegerLiteralData>(); numeric_value.has_value())
        {
            return evaluate_numeric_expression(*numeric_value);
        }

        if (const auto floating_point_value = token.try_get_value<FloatLiteralData>(); floating_point_value.has_value())
        {
            return evaluate_numeric_expression(*floating_point_value);
        }

        if (const auto character_value = token.try_get_value<CharacterLiteralData>(); character_value.has_value())
        {
            switch (character_value->encoding)
            {
                case CharacterEncoding::utf8:
                    DEBUG_ASSERT(character_value->value <= std::numeric_limits<char8_t>::max());
                    return ConstantValue::character(static_cast<char8_t>(character_value->value));
                    break;
                case CharacterEncoding::utf16:
                    DEBUG_ASSERT(character_value->value <= std::numeric_limits<char16_t>::max());
                    return ConstantValue::character16(static_cast<char16_t>(character_value->value));
                    break;
                case CharacterEncoding::utf32:
                    return ConstantValue::rune(character_value->value);
            }
        }

        if (const auto string_value = token.try_get_value<StringLiteralData>(); string_value.has_value())
        {
            return ConstantValue::str(string_value->value);
        }

        UNREACHABLE("Invalid literal type");
    }
} // namespace prism
