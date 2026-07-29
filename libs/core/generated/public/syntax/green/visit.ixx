module;

#include <libassert/assert-macros.hpp>

export module prism.core:syntax.green.visit;

import libassert;
import :type_traits.visitor;
import :syntax.green.structured_trivia;
import :syntax.green.top_level;
import :syntax.green.types;
import :syntax.green.clauses;
import :syntax.green.expressions;
import :syntax.green.statements;
import :syntax.green.declarations;

namespace prism
{
    template <typename Functor>
    concept VisitorForGreenNode = ExhaustiveVisitor<Functor,
                                                    const GreenListNode &,
                                                    const GreenSkippedTokensTrivia &,
                                                    const GreenCompilationUnit &,
                                                    const GreenPredefinedType &,
                                                    const GreenIdentifierNamedType &,
                                                    const GreenInitializer &,
                                                    const GreenTypeSpecifier &,
                                                    const GreenNamedParameter &,
                                                    const GreenArgumentList &,
                                                    const GreenArgument &,
                                                    const GreenParameterList &,
                                                    const GreenParameter &,
                                                    const GreenExpressionBody &,
                                                    const GreenLiteralExpression &,
                                                    const GreenIdentifierExpression &,
                                                    const GreenParenthesizedExpression &,
                                                    const GreenBinaryExpression &,
                                                    const GreenAssignmentExpression &,
                                                    const GreenPrefixExpression &,
                                                    const GreenPostfixExpression &,
                                                    const GreenTernaryExpression &,
                                                    const GreenInvocationExpression &,
                                                    const GreenVariableDeclarationStatement &,
                                                    const GreenBlock &,
                                                    const GreenReturnStatement &,
                                                    const GreenExpressionStatement &,
                                                    const GreenEmptyStatement &,
                                                    const GreenIncompleteDeclaration &,
                                                    const GreenVariableDeclaration &,
                                                    const GreenFunctionDeclaration &>;

    template <VisitorForGreenNode Functor>
    constexpr decltype(auto) visit(const GreenNode &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::list:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenListNode &>(node));
            case SyntaxKind::skipped_tokens_trivia:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenSkippedTokensTrivia &>(node));
            case SyntaxKind::compilation_unit:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenCompilationUnit &>(node));
            case SyntaxKind::predefined_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenPredefinedType &>(node));
            case SyntaxKind::identifier_named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenIdentifierNamedType &>(node));
            case SyntaxKind::initializer:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenInitializer &>(node));
            case SyntaxKind::type_specifier:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenTypeSpecifier &>(node));
            case SyntaxKind::named_parameter:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenNamedParameter &>(node));
            case SyntaxKind::argument_list:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenArgumentList &>(node));
            case SyntaxKind::argument:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenArgument &>(node));
            case SyntaxKind::parameter_list:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenParameterList &>(node));
            case SyntaxKind::parameter:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenParameter &>(node));
            case SyntaxKind::expression_body:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenExpressionBody &>(node));
            case SyntaxKind::literal_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenLiteralExpression &>(node));
            case SyntaxKind::identifier_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenIdentifierExpression &>(node));
            case SyntaxKind::parenthesized_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenParenthesizedExpression &>(node));
            case SyntaxKind::binary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenBinaryExpression &>(node));
            case SyntaxKind::assignment_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenAssignmentExpression &>(node));
            case SyntaxKind::prefix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenPrefixExpression &>(node));
            case SyntaxKind::postfix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenPostfixExpression &>(node));
            case SyntaxKind::ternary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenTernaryExpression &>(node));
            case SyntaxKind::invocation_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenInvocationExpression &>(node));
            case SyntaxKind::variable_declaration_statement:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenVariableDeclarationStatement &>(node));
            case SyntaxKind::block:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenBlock &>(node));
            case SyntaxKind::return_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenReturnStatement &>(node));
            case SyntaxKind::expression_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenExpressionStatement &>(node));
            case SyntaxKind::empty_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenEmptyStatement &>(node));
            case SyntaxKind::incomplete_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenIncompleteDeclaration &>(node));
            case SyntaxKind::variable_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenVariableDeclaration &>(node));
            case SyntaxKind::function_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenFunctionDeclaration &>(node));
            default:
                if (node.is_token())
                {
                    return std::invoke(std::forward<Functor>(functor), static_cast<const GreenToken &>(node));
                }

                if (node.is_trivia())
                {
                    return std::invoke(std::forward<Functor>(functor), static_cast<const GreenTrivia &>(node));
                }

                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor>
    concept MutableVisitorForGreenNode = ExhaustiveVisitor<Functor,
                                                           GreenListNode &,
                                                           GreenSkippedTokensTrivia &,
                                                           GreenCompilationUnit &,
                                                           GreenPredefinedType &,
                                                           GreenIdentifierNamedType &,
                                                           GreenInitializer &,
                                                           GreenTypeSpecifier &,
                                                           GreenNamedParameter &,
                                                           GreenArgumentList &,
                                                           GreenArgument &,
                                                           GreenParameterList &,
                                                           GreenParameter &,
                                                           GreenExpressionBody &,
                                                           GreenLiteralExpression &,
                                                           GreenIdentifierExpression &,
                                                           GreenParenthesizedExpression &,
                                                           GreenBinaryExpression &,
                                                           GreenAssignmentExpression &,
                                                           GreenPrefixExpression &,
                                                           GreenPostfixExpression &,
                                                           GreenTernaryExpression &,
                                                           GreenInvocationExpression &,
                                                           GreenVariableDeclarationStatement &,
                                                           GreenBlock &,
                                                           GreenReturnStatement &,
                                                           GreenExpressionStatement &,
                                                           GreenEmptyStatement &,
                                                           GreenIncompleteDeclaration &,
                                                           GreenVariableDeclaration &,
                                                           GreenFunctionDeclaration &>;

    template <MutableVisitorForGreenNode Functor>
    constexpr decltype(auto) visit(GreenNode &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::list:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenListNode &>(node));
            case SyntaxKind::skipped_tokens_trivia:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenSkippedTokensTrivia &>(node));
            case SyntaxKind::compilation_unit:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenCompilationUnit &>(node));
            case SyntaxKind::predefined_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenPredefinedType &>(node));
            case SyntaxKind::identifier_named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenIdentifierNamedType &>(node));
            case SyntaxKind::initializer:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenInitializer &>(node));
            case SyntaxKind::type_specifier:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenTypeSpecifier &>(node));
            case SyntaxKind::named_parameter:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenNamedParameter &>(node));
            case SyntaxKind::argument_list:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenArgumentList &>(node));
            case SyntaxKind::argument:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenArgument &>(node));
            case SyntaxKind::parameter_list:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenParameterList &>(node));
            case SyntaxKind::parameter:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenParameter &>(node));
            case SyntaxKind::expression_body:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenExpressionBody &>(node));
            case SyntaxKind::literal_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenLiteralExpression &>(node));
            case SyntaxKind::identifier_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenIdentifierExpression &>(node));
            case SyntaxKind::parenthesized_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenParenthesizedExpression &>(node));
            case SyntaxKind::binary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenBinaryExpression &>(node));
            case SyntaxKind::assignment_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenAssignmentExpression &>(node));
            case SyntaxKind::prefix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenPrefixExpression &>(node));
            case SyntaxKind::postfix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenPostfixExpression &>(node));
            case SyntaxKind::ternary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenTernaryExpression &>(node));
            case SyntaxKind::invocation_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenInvocationExpression &>(node));
            case SyntaxKind::variable_declaration_statement:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<GreenVariableDeclarationStatement &>(node));
            case SyntaxKind::block:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenBlock &>(node));
            case SyntaxKind::return_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenReturnStatement &>(node));
            case SyntaxKind::expression_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenExpressionStatement &>(node));
            case SyntaxKind::empty_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenEmptyStatement &>(node));
            case SyntaxKind::incomplete_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenIncompleteDeclaration &>(node));
            case SyntaxKind::variable_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenVariableDeclaration &>(node));
            case SyntaxKind::function_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenFunctionDeclaration &>(node));
            default:
                if (node.is_token())
                {
                    return std::invoke(std::forward<Functor>(functor), static_cast<GreenToken &>(node));
                }

                if (node.is_trivia())
                {
                    return std::invoke(std::forward<Functor>(functor), static_cast<GreenTrivia &>(node));
                }

                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept VisitorForGreenNodeReturning = ConvertibleVisitor<Functor,
                                                              R,
                                                              const GreenListNode &,
                                                              const GreenSkippedTokensTrivia &,
                                                              const GreenCompilationUnit &,
                                                              const GreenPredefinedType &,
                                                              const GreenIdentifierNamedType &,
                                                              const GreenInitializer &,
                                                              const GreenTypeSpecifier &,
                                                              const GreenNamedParameter &,
                                                              const GreenArgumentList &,
                                                              const GreenArgument &,
                                                              const GreenParameterList &,
                                                              const GreenParameter &,
                                                              const GreenExpressionBody &,
                                                              const GreenLiteralExpression &,
                                                              const GreenIdentifierExpression &,
                                                              const GreenParenthesizedExpression &,
                                                              const GreenBinaryExpression &,
                                                              const GreenAssignmentExpression &,
                                                              const GreenPrefixExpression &,
                                                              const GreenPostfixExpression &,
                                                              const GreenTernaryExpression &,
                                                              const GreenInvocationExpression &,
                                                              const GreenVariableDeclarationStatement &,
                                                              const GreenBlock &,
                                                              const GreenReturnStatement &,
                                                              const GreenExpressionStatement &,
                                                              const GreenEmptyStatement &,
                                                              const GreenIncompleteDeclaration &,
                                                              const GreenVariableDeclaration &,
                                                              const GreenFunctionDeclaration &>;

    template <typename R, VisitorForGreenNodeReturning Functor>
    constexpr R visit(const GreenNode &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::list:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenListNode &>(node));
            case SyntaxKind::skipped_tokens_trivia:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenSkippedTokensTrivia &>(node));
            case SyntaxKind::compilation_unit:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenCompilationUnit &>(node));
            case SyntaxKind::predefined_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenPredefinedType &>(node));
            case SyntaxKind::identifier_named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenIdentifierNamedType &>(node));
            case SyntaxKind::initializer:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenInitializer &>(node));
            case SyntaxKind::type_specifier:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenTypeSpecifier &>(node));
            case SyntaxKind::named_parameter:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenNamedParameter &>(node));
            case SyntaxKind::argument_list:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenArgumentList &>(node));
            case SyntaxKind::argument:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenArgument &>(node));
            case SyntaxKind::parameter_list:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenParameterList &>(node));
            case SyntaxKind::parameter:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenParameter &>(node));
            case SyntaxKind::expression_body:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenExpressionBody &>(node));
            case SyntaxKind::literal_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenLiteralExpression &>(node));
            case SyntaxKind::identifier_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenIdentifierExpression &>(node));
            case SyntaxKind::parenthesized_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenParenthesizedExpression &>(node));
            case SyntaxKind::binary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenBinaryExpression &>(node));
            case SyntaxKind::assignment_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenAssignmentExpression &>(node));
            case SyntaxKind::prefix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenPrefixExpression &>(node));
            case SyntaxKind::postfix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenPostfixExpression &>(node));
            case SyntaxKind::ternary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenTernaryExpression &>(node));
            case SyntaxKind::invocation_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenInvocationExpression &>(node));
            case SyntaxKind::variable_declaration_statement:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenVariableDeclarationStatement &>(node));
            case SyntaxKind::block:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenBlock &>(node));
            case SyntaxKind::return_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenReturnStatement &>(node));
            case SyntaxKind::expression_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenExpressionStatement &>(node));
            case SyntaxKind::empty_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenEmptyStatement &>(node));
            case SyntaxKind::incomplete_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenIncompleteDeclaration &>(node));
            case SyntaxKind::variable_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenVariableDeclaration &>(node));
            case SyntaxKind::function_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenFunctionDeclaration &>(node));
            default:
                if (node.is_token())
                {
                    return std::invoke(std::forward<Functor>(functor), static_cast<const GreenToken &>(node));
                }

                if (node.is_trivia())
                {
                    return std::invoke(std::forward<Functor>(functor), static_cast<const GreenTrivia &>(node));
                }

                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept MutableVisitorForGreenNodeReturning = ConvertibleVisitor<Functor,
                                                                     R,
                                                                     GreenListNode &,
                                                                     GreenSkippedTokensTrivia &,
                                                                     GreenCompilationUnit &,
                                                                     GreenPredefinedType &,
                                                                     GreenIdentifierNamedType &,
                                                                     GreenInitializer &,
                                                                     GreenTypeSpecifier &,
                                                                     GreenNamedParameter &,
                                                                     GreenArgumentList &,
                                                                     GreenArgument &,
                                                                     GreenParameterList &,
                                                                     GreenParameter &,
                                                                     GreenExpressionBody &,
                                                                     GreenLiteralExpression &,
                                                                     GreenIdentifierExpression &,
                                                                     GreenParenthesizedExpression &,
                                                                     GreenBinaryExpression &,
                                                                     GreenAssignmentExpression &,
                                                                     GreenPrefixExpression &,
                                                                     GreenPostfixExpression &,
                                                                     GreenTernaryExpression &,
                                                                     GreenInvocationExpression &,
                                                                     GreenVariableDeclarationStatement &,
                                                                     GreenBlock &,
                                                                     GreenReturnStatement &,
                                                                     GreenExpressionStatement &,
                                                                     GreenEmptyStatement &,
                                                                     GreenIncompleteDeclaration &,
                                                                     GreenVariableDeclaration &,
                                                                     GreenFunctionDeclaration &>;

    template <typename R, MutableVisitorForGreenNodeReturning Functor>
    constexpr R visit(GreenNode &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::list:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenListNode &>(node));
            case SyntaxKind::skipped_tokens_trivia:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenSkippedTokensTrivia &>(node));
            case SyntaxKind::compilation_unit:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenCompilationUnit &>(node));
            case SyntaxKind::predefined_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenPredefinedType &>(node));
            case SyntaxKind::identifier_named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenIdentifierNamedType &>(node));
            case SyntaxKind::initializer:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenInitializer &>(node));
            case SyntaxKind::type_specifier:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenTypeSpecifier &>(node));
            case SyntaxKind::named_parameter:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenNamedParameter &>(node));
            case SyntaxKind::argument_list:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenArgumentList &>(node));
            case SyntaxKind::argument:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenArgument &>(node));
            case SyntaxKind::parameter_list:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenParameterList &>(node));
            case SyntaxKind::parameter:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenParameter &>(node));
            case SyntaxKind::expression_body:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenExpressionBody &>(node));
            case SyntaxKind::literal_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenLiteralExpression &>(node));
            case SyntaxKind::identifier_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenIdentifierExpression &>(node));
            case SyntaxKind::parenthesized_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenParenthesizedExpression &>(node));
            case SyntaxKind::binary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenBinaryExpression &>(node));
            case SyntaxKind::assignment_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenAssignmentExpression &>(node));
            case SyntaxKind::prefix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenPrefixExpression &>(node));
            case SyntaxKind::postfix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenPostfixExpression &>(node));
            case SyntaxKind::ternary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenTernaryExpression &>(node));
            case SyntaxKind::invocation_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenInvocationExpression &>(node));
            case SyntaxKind::variable_declaration_statement:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<GreenVariableDeclarationStatement &>(node));
            case SyntaxKind::block:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenBlock &>(node));
            case SyntaxKind::return_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenReturnStatement &>(node));
            case SyntaxKind::expression_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenExpressionStatement &>(node));
            case SyntaxKind::empty_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenEmptyStatement &>(node));
            case SyntaxKind::incomplete_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenIncompleteDeclaration &>(node));
            case SyntaxKind::variable_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenVariableDeclaration &>(node));
            case SyntaxKind::function_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenFunctionDeclaration &>(node));
            default:
                if (node.is_token())
                {
                    return std::invoke(std::forward<Functor>(functor), static_cast<GreenToken &>(node));
                }

                if (node.is_trivia())
                {
                    return std::invoke(std::forward<Functor>(functor), static_cast<GreenTrivia &>(node));
                }

                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor>
    concept VisitorForGreenStructuredTrivia = ExhaustiveVisitor<Functor, const GreenSkippedTokensTrivia &>;

    template <VisitorForGreenStructuredTrivia Functor>
    constexpr decltype(auto) visit(const GreenStructuredTrivia &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::skipped_tokens_trivia:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenSkippedTokensTrivia &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor>
    concept MutableVisitorForGreenStructuredTrivia = ExhaustiveVisitor<Functor, GreenSkippedTokensTrivia &>;

    template <MutableVisitorForGreenStructuredTrivia Functor>
    constexpr decltype(auto) visit(GreenStructuredTrivia &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::skipped_tokens_trivia:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenSkippedTokensTrivia &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept VisitorForGreenStructuredTriviaReturning = ConvertibleVisitor<Functor, R, const GreenSkippedTokensTrivia &>;

    template <typename R, VisitorForGreenStructuredTriviaReturning Functor>
    constexpr R visit(const GreenStructuredTrivia &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::skipped_tokens_trivia:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenSkippedTokensTrivia &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept MutableVisitorForGreenStructuredTriviaReturning =
        ConvertibleVisitor<Functor, R, GreenSkippedTokensTrivia &>;

    template <typename R, MutableVisitorForGreenStructuredTriviaReturning Functor>
    constexpr R visit(GreenStructuredTrivia &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::skipped_tokens_trivia:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenSkippedTokensTrivia &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor>
    concept VisitorForGreenType = ExhaustiveVisitor<Functor, const GreenPredefinedType &, const GreenNamedType &>;

    template <VisitorForGreenType Functor>
    constexpr decltype(auto) visit(const GreenType &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::predefined_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenPredefinedType &>(node));
            case SyntaxKind::named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenNamedType &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor>
    concept MutableVisitorForGreenType = ExhaustiveVisitor<Functor, GreenPredefinedType &, GreenNamedType &>;

    template <MutableVisitorForGreenType Functor>
    constexpr decltype(auto) visit(GreenType &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::predefined_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenPredefinedType &>(node));
            case SyntaxKind::named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenNamedType &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept VisitorForGreenTypeReturning =
        ConvertibleVisitor<Functor, R, const GreenPredefinedType &, const GreenNamedType &>;

    template <typename R, VisitorForGreenTypeReturning Functor>
    constexpr R visit(const GreenType &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::predefined_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenPredefinedType &>(node));
            case SyntaxKind::named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenNamedType &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept MutableVisitorForGreenTypeReturning =
        ConvertibleVisitor<Functor, R, GreenPredefinedType &, GreenNamedType &>;

    template <typename R, MutableVisitorForGreenTypeReturning Functor>
    constexpr R visit(GreenType &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::predefined_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenPredefinedType &>(node));
            case SyntaxKind::named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenNamedType &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor>
    concept VisitorForGreenNamedType = ExhaustiveVisitor<Functor, const GreenSimpleNamedType &>;

    template <VisitorForGreenNamedType Functor>
    constexpr decltype(auto) visit(const GreenNamedType &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::simple_named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenSimpleNamedType &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor>
    concept MutableVisitorForGreenNamedType = ExhaustiveVisitor<Functor, GreenSimpleNamedType &>;

    template <MutableVisitorForGreenNamedType Functor>
    constexpr decltype(auto) visit(GreenNamedType &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::simple_named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenSimpleNamedType &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept VisitorForGreenNamedTypeReturning = ConvertibleVisitor<Functor, R, const GreenSimpleNamedType &>;

    template <typename R, VisitorForGreenNamedTypeReturning Functor>
    constexpr R visit(const GreenNamedType &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::simple_named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenSimpleNamedType &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept MutableVisitorForGreenNamedTypeReturning = ConvertibleVisitor<Functor, R, GreenSimpleNamedType &>;

    template <typename R, MutableVisitorForGreenNamedTypeReturning Functor>
    constexpr R visit(GreenNamedType &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::simple_named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenSimpleNamedType &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor>
    concept VisitorForGreenSimpleNamedType = ExhaustiveVisitor<Functor, const GreenIdentifierNamedType &>;

    template <VisitorForGreenSimpleNamedType Functor>
    constexpr decltype(auto) visit(const GreenSimpleNamedType &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::identifier_named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenIdentifierNamedType &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor>
    concept MutableVisitorForGreenSimpleNamedType = ExhaustiveVisitor<Functor, GreenIdentifierNamedType &>;

    template <MutableVisitorForGreenSimpleNamedType Functor>
    constexpr decltype(auto) visit(GreenSimpleNamedType &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::identifier_named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenIdentifierNamedType &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept VisitorForGreenSimpleNamedTypeReturning = ConvertibleVisitor<Functor, R, const GreenIdentifierNamedType &>;

    template <typename R, VisitorForGreenSimpleNamedTypeReturning Functor>
    constexpr R visit(const GreenSimpleNamedType &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::identifier_named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenIdentifierNamedType &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept MutableVisitorForGreenSimpleNamedTypeReturning = ConvertibleVisitor<Functor, R, GreenIdentifierNamedType &>;

    template <typename R, MutableVisitorForGreenSimpleNamedTypeReturning Functor>
    constexpr R visit(GreenSimpleNamedType &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::identifier_named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenIdentifierNamedType &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor>
    concept VisitorForGreenExpression = ExhaustiveVisitor<Functor,
                                                          const GreenLiteralExpression &,
                                                          const GreenIdentifierExpression &,
                                                          const GreenParenthesizedExpression &,
                                                          const GreenBinaryExpression &,
                                                          const GreenAssignmentExpression &,
                                                          const GreenPrefixExpression &,
                                                          const GreenPostfixExpression &,
                                                          const GreenTernaryExpression &,
                                                          const GreenInvocationExpression &>;

    template <VisitorForGreenExpression Functor>
    constexpr decltype(auto) visit(const GreenExpression &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::literal_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenLiteralExpression &>(node));
            case SyntaxKind::identifier_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenIdentifierExpression &>(node));
            case SyntaxKind::parenthesized_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenParenthesizedExpression &>(node));
            case SyntaxKind::binary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenBinaryExpression &>(node));
            case SyntaxKind::assignment_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenAssignmentExpression &>(node));
            case SyntaxKind::prefix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenPrefixExpression &>(node));
            case SyntaxKind::postfix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenPostfixExpression &>(node));
            case SyntaxKind::ternary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenTernaryExpression &>(node));
            case SyntaxKind::invocation_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenInvocationExpression &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor>
    concept MutableVisitorForGreenExpression = ExhaustiveVisitor<Functor,
                                                                 GreenLiteralExpression &,
                                                                 GreenIdentifierExpression &,
                                                                 GreenParenthesizedExpression &,
                                                                 GreenBinaryExpression &,
                                                                 GreenAssignmentExpression &,
                                                                 GreenPrefixExpression &,
                                                                 GreenPostfixExpression &,
                                                                 GreenTernaryExpression &,
                                                                 GreenInvocationExpression &>;

    template <MutableVisitorForGreenExpression Functor>
    constexpr decltype(auto) visit(GreenExpression &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::literal_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenLiteralExpression &>(node));
            case SyntaxKind::identifier_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenIdentifierExpression &>(node));
            case SyntaxKind::parenthesized_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenParenthesizedExpression &>(node));
            case SyntaxKind::binary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenBinaryExpression &>(node));
            case SyntaxKind::assignment_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenAssignmentExpression &>(node));
            case SyntaxKind::prefix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenPrefixExpression &>(node));
            case SyntaxKind::postfix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenPostfixExpression &>(node));
            case SyntaxKind::ternary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenTernaryExpression &>(node));
            case SyntaxKind::invocation_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenInvocationExpression &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept VisitorForGreenExpressionReturning = ConvertibleVisitor<Functor,
                                                                    R,
                                                                    const GreenLiteralExpression &,
                                                                    const GreenIdentifierExpression &,
                                                                    const GreenParenthesizedExpression &,
                                                                    const GreenBinaryExpression &,
                                                                    const GreenAssignmentExpression &,
                                                                    const GreenPrefixExpression &,
                                                                    const GreenPostfixExpression &,
                                                                    const GreenTernaryExpression &,
                                                                    const GreenInvocationExpression &>;

    template <typename R, VisitorForGreenExpressionReturning Functor>
    constexpr R visit(const GreenExpression &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::literal_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenLiteralExpression &>(node));
            case SyntaxKind::identifier_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenIdentifierExpression &>(node));
            case SyntaxKind::parenthesized_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenParenthesizedExpression &>(node));
            case SyntaxKind::binary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenBinaryExpression &>(node));
            case SyntaxKind::assignment_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenAssignmentExpression &>(node));
            case SyntaxKind::prefix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenPrefixExpression &>(node));
            case SyntaxKind::postfix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenPostfixExpression &>(node));
            case SyntaxKind::ternary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenTernaryExpression &>(node));
            case SyntaxKind::invocation_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenInvocationExpression &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept MutableVisitorForGreenExpressionReturning = ConvertibleVisitor<Functor,
                                                                           R,
                                                                           GreenLiteralExpression &,
                                                                           GreenIdentifierExpression &,
                                                                           GreenParenthesizedExpression &,
                                                                           GreenBinaryExpression &,
                                                                           GreenAssignmentExpression &,
                                                                           GreenPrefixExpression &,
                                                                           GreenPostfixExpression &,
                                                                           GreenTernaryExpression &,
                                                                           GreenInvocationExpression &>;

    template <typename R, MutableVisitorForGreenExpressionReturning Functor>
    constexpr R visit(GreenExpression &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::literal_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenLiteralExpression &>(node));
            case SyntaxKind::identifier_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenIdentifierExpression &>(node));
            case SyntaxKind::parenthesized_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenParenthesizedExpression &>(node));
            case SyntaxKind::binary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenBinaryExpression &>(node));
            case SyntaxKind::assignment_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenAssignmentExpression &>(node));
            case SyntaxKind::prefix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenPrefixExpression &>(node));
            case SyntaxKind::postfix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenPostfixExpression &>(node));
            case SyntaxKind::ternary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenTernaryExpression &>(node));
            case SyntaxKind::invocation_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenInvocationExpression &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor>
    concept VisitorForGreenStatement = ExhaustiveVisitor<Functor,
                                                         const GreenVariableDeclarationStatement &,
                                                         const GreenBlock &,
                                                         const GreenReturnStatement &,
                                                         const GreenExpressionStatement &,
                                                         const GreenEmptyStatement &>;

    template <VisitorForGreenStatement Functor>
    constexpr decltype(auto) visit(const GreenStatement &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::variable_declaration_statement:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenVariableDeclarationStatement &>(node));
            case SyntaxKind::block:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenBlock &>(node));
            case SyntaxKind::return_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenReturnStatement &>(node));
            case SyntaxKind::expression_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenExpressionStatement &>(node));
            case SyntaxKind::empty_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenEmptyStatement &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor>
    concept MutableVisitorForGreenStatement = ExhaustiveVisitor<Functor,
                                                                GreenVariableDeclarationStatement &,
                                                                GreenBlock &,
                                                                GreenReturnStatement &,
                                                                GreenExpressionStatement &,
                                                                GreenEmptyStatement &>;

    template <MutableVisitorForGreenStatement Functor>
    constexpr decltype(auto) visit(GreenStatement &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::variable_declaration_statement:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<GreenVariableDeclarationStatement &>(node));
            case SyntaxKind::block:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenBlock &>(node));
            case SyntaxKind::return_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenReturnStatement &>(node));
            case SyntaxKind::expression_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenExpressionStatement &>(node));
            case SyntaxKind::empty_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenEmptyStatement &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept VisitorForGreenStatementReturning = ConvertibleVisitor<Functor,
                                                                   R,
                                                                   const GreenVariableDeclarationStatement &,
                                                                   const GreenBlock &,
                                                                   const GreenReturnStatement &,
                                                                   const GreenExpressionStatement &,
                                                                   const GreenEmptyStatement &>;

    template <typename R, VisitorForGreenStatementReturning Functor>
    constexpr R visit(const GreenStatement &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::variable_declaration_statement:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenVariableDeclarationStatement &>(node));
            case SyntaxKind::block:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenBlock &>(node));
            case SyntaxKind::return_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenReturnStatement &>(node));
            case SyntaxKind::expression_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenExpressionStatement &>(node));
            case SyntaxKind::empty_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenEmptyStatement &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept MutableVisitorForGreenStatementReturning = ConvertibleVisitor<Functor,
                                                                          R,
                                                                          GreenVariableDeclarationStatement &,
                                                                          GreenBlock &,
                                                                          GreenReturnStatement &,
                                                                          GreenExpressionStatement &,
                                                                          GreenEmptyStatement &>;

    template <typename R, MutableVisitorForGreenStatementReturning Functor>
    constexpr R visit(GreenStatement &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::variable_declaration_statement:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<GreenVariableDeclarationStatement &>(node));
            case SyntaxKind::block:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenBlock &>(node));
            case SyntaxKind::return_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenReturnStatement &>(node));
            case SyntaxKind::expression_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenExpressionStatement &>(node));
            case SyntaxKind::empty_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenEmptyStatement &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor>
    concept VisitorForGreenDeclaration = ExhaustiveVisitor<Functor,
                                                           const GreenIncompleteDeclaration &,
                                                           const GreenVariableDeclaration &,
                                                           const GreenFunctionDeclaration &>;

    template <VisitorForGreenDeclaration Functor>
    constexpr decltype(auto) visit(const GreenDeclaration &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::incomplete_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenIncompleteDeclaration &>(node));
            case SyntaxKind::variable_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenVariableDeclaration &>(node));
            case SyntaxKind::function_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenFunctionDeclaration &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor>
    concept MutableVisitorForGreenDeclaration = ExhaustiveVisitor<Functor,
                                                                  GreenIncompleteDeclaration &,
                                                                  GreenVariableDeclaration &,
                                                                  GreenFunctionDeclaration &>;

    template <MutableVisitorForGreenDeclaration Functor>
    constexpr decltype(auto) visit(GreenDeclaration &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::incomplete_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenIncompleteDeclaration &>(node));
            case SyntaxKind::variable_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenVariableDeclaration &>(node));
            case SyntaxKind::function_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenFunctionDeclaration &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept VisitorForGreenDeclarationReturning = ConvertibleVisitor<Functor,
                                                                     R,
                                                                     const GreenIncompleteDeclaration &,
                                                                     const GreenVariableDeclaration &,
                                                                     const GreenFunctionDeclaration &>;

    template <typename R, VisitorForGreenDeclarationReturning Functor>
    constexpr R visit(const GreenDeclaration &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::incomplete_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const GreenIncompleteDeclaration &>(node));
            case SyntaxKind::variable_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenVariableDeclaration &>(node));
            case SyntaxKind::function_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<const GreenFunctionDeclaration &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept MutableVisitorForGreenDeclarationReturning = ConvertibleVisitor<Functor,
                                                                            R,
                                                                            GreenIncompleteDeclaration &,
                                                                            GreenVariableDeclaration &,
                                                                            GreenFunctionDeclaration &>;

    template <typename R, MutableVisitorForGreenDeclarationReturning Functor>
    constexpr R visit(GreenDeclaration &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::incomplete_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenIncompleteDeclaration &>(node));
            case SyntaxKind::variable_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenVariableDeclaration &>(node));
            case SyntaxKind::function_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<GreenFunctionDeclaration &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <std::invocable<const GreenSkippedTokensTrivia &> Functor>
    constexpr decltype(auto) visit(const GreenSkippedTokensTrivia &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenSkippedTokensTrivia &> Functor>
    constexpr decltype(auto) visit(GreenSkippedTokensTrivia &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenCompilationUnit &> Functor>
    constexpr decltype(auto) visit(const GreenCompilationUnit &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenCompilationUnit &> Functor>
    constexpr decltype(auto) visit(GreenCompilationUnit &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenPredefinedType &> Functor>
    constexpr decltype(auto) visit(const GreenPredefinedType &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenPredefinedType &> Functor>
    constexpr decltype(auto) visit(GreenPredefinedType &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenIdentifierNamedType &> Functor>
    constexpr decltype(auto) visit(const GreenIdentifierNamedType &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenIdentifierNamedType &> Functor>
    constexpr decltype(auto) visit(GreenIdentifierNamedType &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenInitializer &> Functor>
    constexpr decltype(auto) visit(const GreenInitializer &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenInitializer &> Functor>
    constexpr decltype(auto) visit(GreenInitializer &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenTypeSpecifier &> Functor>
    constexpr decltype(auto) visit(const GreenTypeSpecifier &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenTypeSpecifier &> Functor>
    constexpr decltype(auto) visit(GreenTypeSpecifier &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenNamedParameter &> Functor>
    constexpr decltype(auto) visit(const GreenNamedParameter &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenNamedParameter &> Functor>
    constexpr decltype(auto) visit(GreenNamedParameter &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenArgumentList &> Functor>
    constexpr decltype(auto) visit(const GreenArgumentList &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenArgumentList &> Functor>
    constexpr decltype(auto) visit(GreenArgumentList &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenArgument &> Functor>
    constexpr decltype(auto) visit(const GreenArgument &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenArgument &> Functor>
    constexpr decltype(auto) visit(GreenArgument &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenParameterList &> Functor>
    constexpr decltype(auto) visit(const GreenParameterList &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenParameterList &> Functor>
    constexpr decltype(auto) visit(GreenParameterList &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenParameter &> Functor>
    constexpr decltype(auto) visit(const GreenParameter &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenParameter &> Functor>
    constexpr decltype(auto) visit(GreenParameter &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenExpressionBody &> Functor>
    constexpr decltype(auto) visit(const GreenExpressionBody &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenExpressionBody &> Functor>
    constexpr decltype(auto) visit(GreenExpressionBody &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenLiteralExpression &> Functor>
    constexpr decltype(auto) visit(const GreenLiteralExpression &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenLiteralExpression &> Functor>
    constexpr decltype(auto) visit(GreenLiteralExpression &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenIdentifierExpression &> Functor>
    constexpr decltype(auto) visit(const GreenIdentifierExpression &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenIdentifierExpression &> Functor>
    constexpr decltype(auto) visit(GreenIdentifierExpression &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenParenthesizedExpression &> Functor>
    constexpr decltype(auto) visit(const GreenParenthesizedExpression &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenParenthesizedExpression &> Functor>
    constexpr decltype(auto) visit(GreenParenthesizedExpression &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenBinaryExpression &> Functor>
    constexpr decltype(auto) visit(const GreenBinaryExpression &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenBinaryExpression &> Functor>
    constexpr decltype(auto) visit(GreenBinaryExpression &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenAssignmentExpression &> Functor>
    constexpr decltype(auto) visit(const GreenAssignmentExpression &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenAssignmentExpression &> Functor>
    constexpr decltype(auto) visit(GreenAssignmentExpression &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenPrefixExpression &> Functor>
    constexpr decltype(auto) visit(const GreenPrefixExpression &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenPrefixExpression &> Functor>
    constexpr decltype(auto) visit(GreenPrefixExpression &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenPostfixExpression &> Functor>
    constexpr decltype(auto) visit(const GreenPostfixExpression &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenPostfixExpression &> Functor>
    constexpr decltype(auto) visit(GreenPostfixExpression &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenTernaryExpression &> Functor>
    constexpr decltype(auto) visit(const GreenTernaryExpression &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenTernaryExpression &> Functor>
    constexpr decltype(auto) visit(GreenTernaryExpression &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenInvocationExpression &> Functor>
    constexpr decltype(auto) visit(const GreenInvocationExpression &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenInvocationExpression &> Functor>
    constexpr decltype(auto) visit(GreenInvocationExpression &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenVariableDeclarationStatement &> Functor>
    constexpr decltype(auto) visit(const GreenVariableDeclarationStatement &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenVariableDeclarationStatement &> Functor>
    constexpr decltype(auto) visit(GreenVariableDeclarationStatement &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenBlock &> Functor>
    constexpr decltype(auto) visit(const GreenBlock &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenBlock &> Functor>
    constexpr decltype(auto) visit(GreenBlock &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenReturnStatement &> Functor>
    constexpr decltype(auto) visit(const GreenReturnStatement &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenReturnStatement &> Functor>
    constexpr decltype(auto) visit(GreenReturnStatement &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenExpressionStatement &> Functor>
    constexpr decltype(auto) visit(const GreenExpressionStatement &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenExpressionStatement &> Functor>
    constexpr decltype(auto) visit(GreenExpressionStatement &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenEmptyStatement &> Functor>
    constexpr decltype(auto) visit(const GreenEmptyStatement &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenEmptyStatement &> Functor>
    constexpr decltype(auto) visit(GreenEmptyStatement &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenIncompleteDeclaration &> Functor>
    constexpr decltype(auto) visit(const GreenIncompleteDeclaration &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenIncompleteDeclaration &> Functor>
    constexpr decltype(auto) visit(GreenIncompleteDeclaration &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenVariableDeclaration &> Functor>
    constexpr decltype(auto) visit(const GreenVariableDeclaration &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenVariableDeclaration &> Functor>
    constexpr decltype(auto) visit(GreenVariableDeclaration &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const GreenFunctionDeclaration &> Functor>
    constexpr decltype(auto) visit(const GreenFunctionDeclaration &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<GreenFunctionDeclaration &> Functor>
    constexpr decltype(auto) visit(GreenFunctionDeclaration &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <typename R = void>
    class GreenSyntaxVisitor
    {
      public:
        virtual ~GreenSyntaxVisitor() = default;

        template <std::derived_from<GreenNode> T, typename Self>
        constexpr R visit(this Self &&self, const T &node)
        {
            return prism::visit(node, std::forward<Self>(self));
        }

        virtual R operator()(const GreenListNode &node) const = 0;
        virtual R operator()(const GreenToken &node) const = 0;
        virtual R operator()(const GreenTrivia &node) const = 0;
        virtual R operator()(const GreenSkippedTokensTrivia &node) const = 0;
        virtual R operator()(const GreenCompilationUnit &node) const = 0;
        virtual R operator()(const GreenPredefinedType &node) const = 0;
        virtual R operator()(const GreenIdentifierNamedType &node) const = 0;
        virtual R operator()(const GreenInitializer &node) const = 0;
        virtual R operator()(const GreenTypeSpecifier &node) const = 0;
        virtual R operator()(const GreenNamedParameter &node) const = 0;
        virtual R operator()(const GreenArgumentList &node) const = 0;
        virtual R operator()(const GreenArgument &node) const = 0;
        virtual R operator()(const GreenParameterList &node) const = 0;
        virtual R operator()(const GreenParameter &node) const = 0;
        virtual R operator()(const GreenExpressionBody &node) const = 0;
        virtual R operator()(const GreenLiteralExpression &node) const = 0;
        virtual R operator()(const GreenIdentifierExpression &node) const = 0;
        virtual R operator()(const GreenParenthesizedExpression &node) const = 0;
        virtual R operator()(const GreenBinaryExpression &node) const = 0;
        virtual R operator()(const GreenAssignmentExpression &node) const = 0;
        virtual R operator()(const GreenPrefixExpression &node) const = 0;
        virtual R operator()(const GreenPostfixExpression &node) const = 0;
        virtual R operator()(const GreenTernaryExpression &node) const = 0;
        virtual R operator()(const GreenInvocationExpression &node) const = 0;
        virtual R operator()(const GreenVariableDeclarationStatement &node) const = 0;
        virtual R operator()(const GreenBlock &node) const = 0;
        virtual R operator()(const GreenReturnStatement &node) const = 0;
        virtual R operator()(const GreenExpressionStatement &node) const = 0;
        virtual R operator()(const GreenEmptyStatement &node) const = 0;
        virtual R operator()(const GreenIncompleteDeclaration &node) const = 0;
        virtual R operator()(const GreenVariableDeclaration &node) const = 0;
        virtual R operator()(const GreenFunctionDeclaration &node) const = 0;
    };

    template <typename R = void>
        requires(std::same_as<R, void> || std::is_default_constructible_v<R>)
    class GreenSyntaxVisitorBase : public GreenSyntaxVisitor<R>
    {
      public:
        constexpr R operator()(const GreenListNode &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenToken &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenTrivia &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenSkippedTokensTrivia &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenCompilationUnit &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenPredefinedType &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenIdentifierNamedType &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenInitializer &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenTypeSpecifier &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenNamedParameter &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenArgumentList &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenArgument &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenParameterList &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenParameter &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenExpressionBody &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenLiteralExpression &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenIdentifierExpression &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenParenthesizedExpression &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenBinaryExpression &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenAssignmentExpression &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenPrefixExpression &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenPostfixExpression &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenTernaryExpression &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenInvocationExpression &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenVariableDeclarationStatement &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenBlock &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenReturnStatement &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenExpressionStatement &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenEmptyStatement &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenIncompleteDeclaration &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenVariableDeclaration &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(const GreenFunctionDeclaration &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }
    };

    template <typename R = void>
    class MutableGreenSyntaxVisitor
    {
      public:
        virtual ~MutableGreenSyntaxVisitor() = default;

        template <std::derived_from<GreenNode> T, typename Self>
        constexpr R visit(this Self &&self, T &node)
        {
            return prism::visit(node, std::forward<Self>(self));
        }

        virtual R operator()(GreenListNode &node) const = 0;
        virtual R operator()(GreenToken &node) const = 0;
        virtual R operator()(GreenTrivia &node) const = 0;
        virtual R operator()(GreenSkippedTokensTrivia &node) const = 0;
        virtual R operator()(GreenCompilationUnit &node) const = 0;
        virtual R operator()(GreenPredefinedType &node) const = 0;
        virtual R operator()(GreenIdentifierNamedType &node) const = 0;
        virtual R operator()(GreenInitializer &node) const = 0;
        virtual R operator()(GreenTypeSpecifier &node) const = 0;
        virtual R operator()(GreenNamedParameter &node) const = 0;
        virtual R operator()(GreenArgumentList &node) const = 0;
        virtual R operator()(GreenArgument &node) const = 0;
        virtual R operator()(GreenParameterList &node) const = 0;
        virtual R operator()(GreenParameter &node) const = 0;
        virtual R operator()(GreenExpressionBody &node) const = 0;
        virtual R operator()(GreenLiteralExpression &node) const = 0;
        virtual R operator()(GreenIdentifierExpression &node) const = 0;
        virtual R operator()(GreenParenthesizedExpression &node) const = 0;
        virtual R operator()(GreenBinaryExpression &node) const = 0;
        virtual R operator()(GreenAssignmentExpression &node) const = 0;
        virtual R operator()(GreenPrefixExpression &node) const = 0;
        virtual R operator()(GreenPostfixExpression &node) const = 0;
        virtual R operator()(GreenTernaryExpression &node) const = 0;
        virtual R operator()(GreenInvocationExpression &node) const = 0;
        virtual R operator()(GreenVariableDeclarationStatement &node) const = 0;
        virtual R operator()(GreenBlock &node) const = 0;
        virtual R operator()(GreenReturnStatement &node) const = 0;
        virtual R operator()(GreenExpressionStatement &node) const = 0;
        virtual R operator()(GreenEmptyStatement &node) const = 0;
        virtual R operator()(GreenIncompleteDeclaration &node) const = 0;
        virtual R operator()(GreenVariableDeclaration &node) const = 0;
        virtual R operator()(GreenFunctionDeclaration &node) const = 0;
    };

    template <typename R = void>
        requires(std::same_as<R, void> || std::is_default_constructible_v<R>)
    class MutableGreenSyntaxVisitorBase : public MutableGreenSyntaxVisitor<R>
    {
      public:
        constexpr R operator()(GreenListNode &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenToken &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenTrivia &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenSkippedTokensTrivia &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenCompilationUnit &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenPredefinedType &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenIdentifierNamedType &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenInitializer &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenTypeSpecifier &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenNamedParameter &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenArgumentList &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenArgument &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenParameterList &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenParameter &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenExpressionBody &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenLiteralExpression &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenIdentifierExpression &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenParenthesizedExpression &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenBinaryExpression &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenAssignmentExpression &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenPrefixExpression &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenPostfixExpression &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenTernaryExpression &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenInvocationExpression &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenVariableDeclarationStatement &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenBlock &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenReturnStatement &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenExpressionStatement &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenEmptyStatement &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenIncompleteDeclaration &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenVariableDeclaration &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }

        constexpr R operator()(GreenFunctionDeclaration &node) const override
        {
            if constexpr (!std::same_as<R, void>)
            {
                return R{};
            }
            else
            {
                return;
            }
        }
    };
} // namespace prism
