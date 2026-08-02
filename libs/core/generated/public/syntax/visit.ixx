module;

#include <libassert/assert-macros.hpp>

export module prism.core:syntax.visit;

import libassert;
import :type_traits.visitor;
import :syntax.structured_trivia;
import :syntax.top_level;
import :syntax.types;
import :syntax.names;
import :syntax.clauses;
import :syntax.expressions;
import :syntax.statements;
import :syntax.declarations;
import :syntax.directives;

namespace prism
{
    export template <typename Functor>
    concept VisitorForSyntaxNode = ExhaustiveVisitor<Functor,
                                                     const SkippedTokensTriviaSyntax &,
                                                     const CompilationUnitSyntax &,
                                                     const PredefinedTypeSyntax &,
                                                     const NamedTypeSyntax &,
                                                     const SimpleNameSyntax &,
                                                     const QualifiedNameSyntax &,
                                                     const InitializerSyntax &,
                                                     const TypeSpecifierSyntax &,
                                                     const NamedParameterSyntax &,
                                                     const ArgumentListSyntax &,
                                                     const ArgumentSyntax &,
                                                     const ParameterListSyntax &,
                                                     const ParameterSyntax &,
                                                     const ExpressionBodySyntax &,
                                                     const LiteralExpressionSyntax &,
                                                     const IdentifierExpressionSyntax &,
                                                     const ParenthesizedExpressionSyntax &,
                                                     const BinaryExpressionSyntax &,
                                                     const AssignmentExpressionSyntax &,
                                                     const PrefixExpressionSyntax &,
                                                     const PostfixExpressionSyntax &,
                                                     const TernaryExpressionSyntax &,
                                                     const InvocationExpressionSyntax &,
                                                     const VariableDeclarationStatementSyntax &,
                                                     const BlockSyntax &,
                                                     const ReturnStatementSyntax &,
                                                     const ExpressionStatementSyntax &,
                                                     const EmptyStatementSyntax &,
                                                     const IncompleteDeclarationSyntax &,
                                                     const BlockNamespaceDeclarationSyntax &,
                                                     const FileScopedNamespaceDeclarationSyntax &,
                                                     const VariableDeclarationSyntax &,
                                                     const FunctionDeclarationSyntax &,
                                                     const UsingDirectiveSyntax &>;

    export template <VisitorForSyntaxNode Functor>
    constexpr decltype(auto) visit(const SyntaxNode &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::skipped_tokens_trivia:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const SkippedTokensTriviaSyntax &>(node));
            case SyntaxKind::compilation_unit:
                return std::invoke(std::forward<Functor>(functor), static_cast<const CompilationUnitSyntax &>(node));
            case SyntaxKind::predefined_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const PredefinedTypeSyntax &>(node));
            case SyntaxKind::named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const NamedTypeSyntax &>(node));
            case SyntaxKind::simple_name:
                return std::invoke(std::forward<Functor>(functor), static_cast<const SimpleNameSyntax &>(node));
            case SyntaxKind::qualified_name:
                return std::invoke(std::forward<Functor>(functor), static_cast<const QualifiedNameSyntax &>(node));
            case SyntaxKind::initializer:
                return std::invoke(std::forward<Functor>(functor), static_cast<const InitializerSyntax &>(node));
            case SyntaxKind::type_specifier:
                return std::invoke(std::forward<Functor>(functor), static_cast<const TypeSpecifierSyntax &>(node));
            case SyntaxKind::named_parameter:
                return std::invoke(std::forward<Functor>(functor), static_cast<const NamedParameterSyntax &>(node));
            case SyntaxKind::argument_list:
                return std::invoke(std::forward<Functor>(functor), static_cast<const ArgumentListSyntax &>(node));
            case SyntaxKind::argument:
                return std::invoke(std::forward<Functor>(functor), static_cast<const ArgumentSyntax &>(node));
            case SyntaxKind::parameter_list:
                return std::invoke(std::forward<Functor>(functor), static_cast<const ParameterListSyntax &>(node));
            case SyntaxKind::parameter:
                return std::invoke(std::forward<Functor>(functor), static_cast<const ParameterSyntax &>(node));
            case SyntaxKind::expression_body:
                return std::invoke(std::forward<Functor>(functor), static_cast<const ExpressionBodySyntax &>(node));
            case SyntaxKind::literal_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const LiteralExpressionSyntax &>(node));
            case SyntaxKind::identifier_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const IdentifierExpressionSyntax &>(node));
            case SyntaxKind::parenthesized_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const ParenthesizedExpressionSyntax &>(node));
            case SyntaxKind::binary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BinaryExpressionSyntax &>(node));
            case SyntaxKind::assignment_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const AssignmentExpressionSyntax &>(node));
            case SyntaxKind::prefix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const PrefixExpressionSyntax &>(node));
            case SyntaxKind::postfix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const PostfixExpressionSyntax &>(node));
            case SyntaxKind::ternary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const TernaryExpressionSyntax &>(node));
            case SyntaxKind::invocation_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const InvocationExpressionSyntax &>(node));
            case SyntaxKind::variable_declaration_statement:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const VariableDeclarationStatementSyntax &>(node));
            case SyntaxKind::block:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BlockSyntax &>(node));
            case SyntaxKind::return_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const ReturnStatementSyntax &>(node));
            case SyntaxKind::expression_statement:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const ExpressionStatementSyntax &>(node));
            case SyntaxKind::empty_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const EmptyStatementSyntax &>(node));
            case SyntaxKind::incomplete_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const IncompleteDeclarationSyntax &>(node));
            case SyntaxKind::block_namespace_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const BlockNamespaceDeclarationSyntax &>(node));
            case SyntaxKind::file_scoped_namespace_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const FileScopedNamespaceDeclarationSyntax &>(node));
            case SyntaxKind::variable_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const VariableDeclarationSyntax &>(node));
            case SyntaxKind::function_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const FunctionDeclarationSyntax &>(node));
            case SyntaxKind::using_directive:
                return std::invoke(std::forward<Functor>(functor), static_cast<const UsingDirectiveSyntax &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    export template <typename Functor, typename R>
    concept VisitorForSyntaxNodeReturning = ConvertibleVisitor<Functor,
                                                               R,
                                                               const SkippedTokensTriviaSyntax &,
                                                               const CompilationUnitSyntax &,
                                                               const PredefinedTypeSyntax &,
                                                               const NamedTypeSyntax &,
                                                               const SimpleNameSyntax &,
                                                               const QualifiedNameSyntax &,
                                                               const InitializerSyntax &,
                                                               const TypeSpecifierSyntax &,
                                                               const NamedParameterSyntax &,
                                                               const ArgumentListSyntax &,
                                                               const ArgumentSyntax &,
                                                               const ParameterListSyntax &,
                                                               const ParameterSyntax &,
                                                               const ExpressionBodySyntax &,
                                                               const LiteralExpressionSyntax &,
                                                               const IdentifierExpressionSyntax &,
                                                               const ParenthesizedExpressionSyntax &,
                                                               const BinaryExpressionSyntax &,
                                                               const AssignmentExpressionSyntax &,
                                                               const PrefixExpressionSyntax &,
                                                               const PostfixExpressionSyntax &,
                                                               const TernaryExpressionSyntax &,
                                                               const InvocationExpressionSyntax &,
                                                               const VariableDeclarationStatementSyntax &,
                                                               const BlockSyntax &,
                                                               const ReturnStatementSyntax &,
                                                               const ExpressionStatementSyntax &,
                                                               const EmptyStatementSyntax &,
                                                               const IncompleteDeclarationSyntax &,
                                                               const BlockNamespaceDeclarationSyntax &,
                                                               const FileScopedNamespaceDeclarationSyntax &,
                                                               const VariableDeclarationSyntax &,
                                                               const FunctionDeclarationSyntax &,
                                                               const UsingDirectiveSyntax &>;

    export template <typename R, VisitorForSyntaxNodeReturning Functor>
    constexpr R visit(const SyntaxNode &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::skipped_tokens_trivia:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const SkippedTokensTriviaSyntax &>(node));
            case SyntaxKind::compilation_unit:
                return std::invoke(std::forward<Functor>(functor), static_cast<const CompilationUnitSyntax &>(node));
            case SyntaxKind::predefined_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const PredefinedTypeSyntax &>(node));
            case SyntaxKind::named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const NamedTypeSyntax &>(node));
            case SyntaxKind::simple_name:
                return std::invoke(std::forward<Functor>(functor), static_cast<const SimpleNameSyntax &>(node));
            case SyntaxKind::qualified_name:
                return std::invoke(std::forward<Functor>(functor), static_cast<const QualifiedNameSyntax &>(node));
            case SyntaxKind::initializer:
                return std::invoke(std::forward<Functor>(functor), static_cast<const InitializerSyntax &>(node));
            case SyntaxKind::type_specifier:
                return std::invoke(std::forward<Functor>(functor), static_cast<const TypeSpecifierSyntax &>(node));
            case SyntaxKind::named_parameter:
                return std::invoke(std::forward<Functor>(functor), static_cast<const NamedParameterSyntax &>(node));
            case SyntaxKind::argument_list:
                return std::invoke(std::forward<Functor>(functor), static_cast<const ArgumentListSyntax &>(node));
            case SyntaxKind::argument:
                return std::invoke(std::forward<Functor>(functor), static_cast<const ArgumentSyntax &>(node));
            case SyntaxKind::parameter_list:
                return std::invoke(std::forward<Functor>(functor), static_cast<const ParameterListSyntax &>(node));
            case SyntaxKind::parameter:
                return std::invoke(std::forward<Functor>(functor), static_cast<const ParameterSyntax &>(node));
            case SyntaxKind::expression_body:
                return std::invoke(std::forward<Functor>(functor), static_cast<const ExpressionBodySyntax &>(node));
            case SyntaxKind::literal_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const LiteralExpressionSyntax &>(node));
            case SyntaxKind::identifier_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const IdentifierExpressionSyntax &>(node));
            case SyntaxKind::parenthesized_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const ParenthesizedExpressionSyntax &>(node));
            case SyntaxKind::binary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BinaryExpressionSyntax &>(node));
            case SyntaxKind::assignment_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const AssignmentExpressionSyntax &>(node));
            case SyntaxKind::prefix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const PrefixExpressionSyntax &>(node));
            case SyntaxKind::postfix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const PostfixExpressionSyntax &>(node));
            case SyntaxKind::ternary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const TernaryExpressionSyntax &>(node));
            case SyntaxKind::invocation_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const InvocationExpressionSyntax &>(node));
            case SyntaxKind::variable_declaration_statement:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const VariableDeclarationStatementSyntax &>(node));
            case SyntaxKind::block:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BlockSyntax &>(node));
            case SyntaxKind::return_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const ReturnStatementSyntax &>(node));
            case SyntaxKind::expression_statement:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const ExpressionStatementSyntax &>(node));
            case SyntaxKind::empty_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const EmptyStatementSyntax &>(node));
            case SyntaxKind::incomplete_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const IncompleteDeclarationSyntax &>(node));
            case SyntaxKind::block_namespace_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const BlockNamespaceDeclarationSyntax &>(node));
            case SyntaxKind::file_scoped_namespace_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const FileScopedNamespaceDeclarationSyntax &>(node));
            case SyntaxKind::variable_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const VariableDeclarationSyntax &>(node));
            case SyntaxKind::function_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const FunctionDeclarationSyntax &>(node));
            case SyntaxKind::using_directive:
                return std::invoke(std::forward<Functor>(functor), static_cast<const UsingDirectiveSyntax &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    export template <typename Functor>
    concept VisitorForStructuredTriviaSyntax = ExhaustiveVisitor<Functor, const SkippedTokensTriviaSyntax &>;

    export template <VisitorForStructuredTriviaSyntax Functor>
    constexpr decltype(auto) visit(const StructuredTriviaSyntax &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::skipped_tokens_trivia:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const SkippedTokensTriviaSyntax &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    export template <typename Functor, typename R>
    concept VisitorForStructuredTriviaSyntaxReturning =
        ConvertibleVisitor<Functor, R, const SkippedTokensTriviaSyntax &>;

    export template <typename R, VisitorForStructuredTriviaSyntaxReturning Functor>
    constexpr R visit(const StructuredTriviaSyntax &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::skipped_tokens_trivia:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const SkippedTokensTriviaSyntax &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    export template <typename Functor>
    concept VisitorForTypeSyntax = ExhaustiveVisitor<Functor, const PredefinedTypeSyntax &, const NamedTypeSyntax &>;

    export template <VisitorForTypeSyntax Functor>
    constexpr decltype(auto) visit(const TypeSyntax &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::predefined_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const PredefinedTypeSyntax &>(node));
            case SyntaxKind::named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const NamedTypeSyntax &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    export template <typename Functor, typename R>
    concept VisitorForTypeSyntaxReturning =
        ConvertibleVisitor<Functor, R, const PredefinedTypeSyntax &, const NamedTypeSyntax &>;

    export template <typename R, VisitorForTypeSyntaxReturning Functor>
    constexpr R visit(const TypeSyntax &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::predefined_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const PredefinedTypeSyntax &>(node));
            case SyntaxKind::named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const NamedTypeSyntax &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    export template <typename Functor>
    concept VisitorForNameSyntax = ExhaustiveVisitor<Functor, const SimpleNameSyntax &, const QualifiedNameSyntax &>;

    export template <VisitorForNameSyntax Functor>
    constexpr decltype(auto) visit(const NameSyntax &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::simple_name:
                return std::invoke(std::forward<Functor>(functor), static_cast<const SimpleNameSyntax &>(node));
            case SyntaxKind::qualified_name:
                return std::invoke(std::forward<Functor>(functor), static_cast<const QualifiedNameSyntax &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    export template <typename Functor, typename R>
    concept VisitorForNameSyntaxReturning =
        ConvertibleVisitor<Functor, R, const SimpleNameSyntax &, const QualifiedNameSyntax &>;

    export template <typename R, VisitorForNameSyntaxReturning Functor>
    constexpr R visit(const NameSyntax &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::simple_name:
                return std::invoke(std::forward<Functor>(functor), static_cast<const SimpleNameSyntax &>(node));
            case SyntaxKind::qualified_name:
                return std::invoke(std::forward<Functor>(functor), static_cast<const QualifiedNameSyntax &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    export template <typename Functor>
    concept VisitorForExpressionSyntax = ExhaustiveVisitor<Functor,
                                                           const LiteralExpressionSyntax &,
                                                           const IdentifierExpressionSyntax &,
                                                           const ParenthesizedExpressionSyntax &,
                                                           const BinaryExpressionSyntax &,
                                                           const AssignmentExpressionSyntax &,
                                                           const PrefixExpressionSyntax &,
                                                           const PostfixExpressionSyntax &,
                                                           const TernaryExpressionSyntax &,
                                                           const InvocationExpressionSyntax &>;

    export template <VisitorForExpressionSyntax Functor>
    constexpr decltype(auto) visit(const ExpressionSyntax &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::literal_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const LiteralExpressionSyntax &>(node));
            case SyntaxKind::identifier_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const IdentifierExpressionSyntax &>(node));
            case SyntaxKind::parenthesized_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const ParenthesizedExpressionSyntax &>(node));
            case SyntaxKind::binary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BinaryExpressionSyntax &>(node));
            case SyntaxKind::assignment_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const AssignmentExpressionSyntax &>(node));
            case SyntaxKind::prefix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const PrefixExpressionSyntax &>(node));
            case SyntaxKind::postfix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const PostfixExpressionSyntax &>(node));
            case SyntaxKind::ternary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const TernaryExpressionSyntax &>(node));
            case SyntaxKind::invocation_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const InvocationExpressionSyntax &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    export template <typename Functor, typename R>
    concept VisitorForExpressionSyntaxReturning = ConvertibleVisitor<Functor,
                                                                     R,
                                                                     const LiteralExpressionSyntax &,
                                                                     const IdentifierExpressionSyntax &,
                                                                     const ParenthesizedExpressionSyntax &,
                                                                     const BinaryExpressionSyntax &,
                                                                     const AssignmentExpressionSyntax &,
                                                                     const PrefixExpressionSyntax &,
                                                                     const PostfixExpressionSyntax &,
                                                                     const TernaryExpressionSyntax &,
                                                                     const InvocationExpressionSyntax &>;

    export template <typename R, VisitorForExpressionSyntaxReturning Functor>
    constexpr R visit(const ExpressionSyntax &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::literal_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const LiteralExpressionSyntax &>(node));
            case SyntaxKind::identifier_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const IdentifierExpressionSyntax &>(node));
            case SyntaxKind::parenthesized_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const ParenthesizedExpressionSyntax &>(node));
            case SyntaxKind::binary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BinaryExpressionSyntax &>(node));
            case SyntaxKind::assignment_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const AssignmentExpressionSyntax &>(node));
            case SyntaxKind::prefix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const PrefixExpressionSyntax &>(node));
            case SyntaxKind::postfix_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const PostfixExpressionSyntax &>(node));
            case SyntaxKind::ternary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const TernaryExpressionSyntax &>(node));
            case SyntaxKind::invocation_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const InvocationExpressionSyntax &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    export template <typename Functor>
    concept VisitorForStatementSyntax = ExhaustiveVisitor<Functor,
                                                          const VariableDeclarationStatementSyntax &,
                                                          const BlockSyntax &,
                                                          const ReturnStatementSyntax &,
                                                          const ExpressionStatementSyntax &,
                                                          const EmptyStatementSyntax &>;

    export template <VisitorForStatementSyntax Functor>
    constexpr decltype(auto) visit(const StatementSyntax &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::variable_declaration_statement:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const VariableDeclarationStatementSyntax &>(node));
            case SyntaxKind::block:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BlockSyntax &>(node));
            case SyntaxKind::return_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const ReturnStatementSyntax &>(node));
            case SyntaxKind::expression_statement:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const ExpressionStatementSyntax &>(node));
            case SyntaxKind::empty_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const EmptyStatementSyntax &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    export template <typename Functor, typename R>
    concept VisitorForStatementSyntaxReturning = ConvertibleVisitor<Functor,
                                                                    R,
                                                                    const VariableDeclarationStatementSyntax &,
                                                                    const BlockSyntax &,
                                                                    const ReturnStatementSyntax &,
                                                                    const ExpressionStatementSyntax &,
                                                                    const EmptyStatementSyntax &>;

    export template <typename R, VisitorForStatementSyntaxReturning Functor>
    constexpr R visit(const StatementSyntax &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::variable_declaration_statement:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const VariableDeclarationStatementSyntax &>(node));
            case SyntaxKind::block:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BlockSyntax &>(node));
            case SyntaxKind::return_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const ReturnStatementSyntax &>(node));
            case SyntaxKind::expression_statement:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const ExpressionStatementSyntax &>(node));
            case SyntaxKind::empty_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const EmptyStatementSyntax &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    export template <typename Functor>
    concept VisitorForDeclarationSyntax = ExhaustiveVisitor<Functor,
                                                            const IncompleteDeclarationSyntax &,
                                                            const BlockNamespaceDeclarationSyntax &,
                                                            const FileScopedNamespaceDeclarationSyntax &,
                                                            const VariableDeclarationSyntax &,
                                                            const FunctionDeclarationSyntax &>;

    export template <VisitorForDeclarationSyntax Functor>
    constexpr decltype(auto) visit(const DeclarationSyntax &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::incomplete_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const IncompleteDeclarationSyntax &>(node));
            case SyntaxKind::block_namespace_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const BlockNamespaceDeclarationSyntax &>(node));
            case SyntaxKind::file_scoped_namespace_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const FileScopedNamespaceDeclarationSyntax &>(node));
            case SyntaxKind::variable_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const VariableDeclarationSyntax &>(node));
            case SyntaxKind::function_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const FunctionDeclarationSyntax &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    export template <typename Functor, typename R>
    concept VisitorForDeclarationSyntaxReturning = ConvertibleVisitor<Functor,
                                                                      R,
                                                                      const IncompleteDeclarationSyntax &,
                                                                      const BlockNamespaceDeclarationSyntax &,
                                                                      const FileScopedNamespaceDeclarationSyntax &,
                                                                      const VariableDeclarationSyntax &,
                                                                      const FunctionDeclarationSyntax &>;

    export template <typename R, VisitorForDeclarationSyntaxReturning Functor>
    constexpr R visit(const DeclarationSyntax &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::incomplete_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const IncompleteDeclarationSyntax &>(node));
            case SyntaxKind::block_namespace_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const BlockNamespaceDeclarationSyntax &>(node));
            case SyntaxKind::file_scoped_namespace_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const FileScopedNamespaceDeclarationSyntax &>(node));
            case SyntaxKind::variable_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const VariableDeclarationSyntax &>(node));
            case SyntaxKind::function_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const FunctionDeclarationSyntax &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    export template <typename Functor>
    concept VisitorForNamespaceDeclarationSyntax = ExhaustiveVisitor<Functor,
                                                                     const BlockNamespaceDeclarationSyntax &,
                                                                     const FileScopedNamespaceDeclarationSyntax &>;

    export template <VisitorForNamespaceDeclarationSyntax Functor>
    constexpr decltype(auto) visit(const NamespaceDeclarationSyntax &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::block_namespace_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const BlockNamespaceDeclarationSyntax &>(node));
            case SyntaxKind::file_scoped_namespace_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const FileScopedNamespaceDeclarationSyntax &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    export template <typename Functor, typename R>
    concept VisitorForNamespaceDeclarationSyntaxReturning =
        ConvertibleVisitor<Functor,
                           R,
                           const BlockNamespaceDeclarationSyntax &,
                           const FileScopedNamespaceDeclarationSyntax &>;

    export template <typename R, VisitorForNamespaceDeclarationSyntaxReturning Functor>
    constexpr R visit(const NamespaceDeclarationSyntax &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case SyntaxKind::block_namespace_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const BlockNamespaceDeclarationSyntax &>(node));
            case SyntaxKind::file_scoped_namespace_declaration:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const FileScopedNamespaceDeclarationSyntax &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <std::invocable<const SkippedTokensTriviaSyntax &> Functor>
    constexpr decltype(auto) visit(const SkippedTokensTriviaSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const CompilationUnitSyntax &> Functor>
    constexpr decltype(auto) visit(const CompilationUnitSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const PredefinedTypeSyntax &> Functor>
    constexpr decltype(auto) visit(const PredefinedTypeSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const NamedTypeSyntax &> Functor>
    constexpr decltype(auto) visit(const NamedTypeSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const SimpleNameSyntax &> Functor>
    constexpr decltype(auto) visit(const SimpleNameSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const QualifiedNameSyntax &> Functor>
    constexpr decltype(auto) visit(const QualifiedNameSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const InitializerSyntax &> Functor>
    constexpr decltype(auto) visit(const InitializerSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const TypeSpecifierSyntax &> Functor>
    constexpr decltype(auto) visit(const TypeSpecifierSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const NamedParameterSyntax &> Functor>
    constexpr decltype(auto) visit(const NamedParameterSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const ArgumentListSyntax &> Functor>
    constexpr decltype(auto) visit(const ArgumentListSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const ArgumentSyntax &> Functor>
    constexpr decltype(auto) visit(const ArgumentSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const ParameterListSyntax &> Functor>
    constexpr decltype(auto) visit(const ParameterListSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const ParameterSyntax &> Functor>
    constexpr decltype(auto) visit(const ParameterSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const ExpressionBodySyntax &> Functor>
    constexpr decltype(auto) visit(const ExpressionBodySyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const LiteralExpressionSyntax &> Functor>
    constexpr decltype(auto) visit(const LiteralExpressionSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const IdentifierExpressionSyntax &> Functor>
    constexpr decltype(auto) visit(const IdentifierExpressionSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const ParenthesizedExpressionSyntax &> Functor>
    constexpr decltype(auto) visit(const ParenthesizedExpressionSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const BinaryExpressionSyntax &> Functor>
    constexpr decltype(auto) visit(const BinaryExpressionSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const AssignmentExpressionSyntax &> Functor>
    constexpr decltype(auto) visit(const AssignmentExpressionSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const PrefixExpressionSyntax &> Functor>
    constexpr decltype(auto) visit(const PrefixExpressionSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const PostfixExpressionSyntax &> Functor>
    constexpr decltype(auto) visit(const PostfixExpressionSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const TernaryExpressionSyntax &> Functor>
    constexpr decltype(auto) visit(const TernaryExpressionSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const InvocationExpressionSyntax &> Functor>
    constexpr decltype(auto) visit(const InvocationExpressionSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const VariableDeclarationStatementSyntax &> Functor>
    constexpr decltype(auto) visit(const VariableDeclarationStatementSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const BlockSyntax &> Functor>
    constexpr decltype(auto) visit(const BlockSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const ReturnStatementSyntax &> Functor>
    constexpr decltype(auto) visit(const ReturnStatementSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const ExpressionStatementSyntax &> Functor>
    constexpr decltype(auto) visit(const ExpressionStatementSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const EmptyStatementSyntax &> Functor>
    constexpr decltype(auto) visit(const EmptyStatementSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const IncompleteDeclarationSyntax &> Functor>
    constexpr decltype(auto) visit(const IncompleteDeclarationSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const BlockNamespaceDeclarationSyntax &> Functor>
    constexpr decltype(auto) visit(const BlockNamespaceDeclarationSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const FileScopedNamespaceDeclarationSyntax &> Functor>
    constexpr decltype(auto) visit(const FileScopedNamespaceDeclarationSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const VariableDeclarationSyntax &> Functor>
    constexpr decltype(auto) visit(const VariableDeclarationSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const FunctionDeclarationSyntax &> Functor>
    constexpr decltype(auto) visit(const FunctionDeclarationSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    template <std::invocable<const UsingDirectiveSyntax &> Functor>
    constexpr decltype(auto) visit(const UsingDirectiveSyntax &node, Functor &&functor)
    {
        return std::invoke(std::forward<Functor>(functor), node);
    }

    export template <typename R = void>
    class SyntaxVisitor
    {
      public:
        virtual ~SyntaxVisitor() = default;

        template <std::derived_from<SyntaxNode> T, typename Self>
        constexpr R visit(this Self &&self, const T &node)
        {
            return prism::visit(node, std::forward<Self>(self));
        }

        virtual R operator()(const SkippedTokensTriviaSyntax &node) const = 0;
        virtual R operator()(const CompilationUnitSyntax &node) const = 0;
        virtual R operator()(const PredefinedTypeSyntax &node) const = 0;
        virtual R operator()(const NamedTypeSyntax &node) const = 0;
        virtual R operator()(const SimpleNameSyntax &node) const = 0;
        virtual R operator()(const QualifiedNameSyntax &node) const = 0;
        virtual R operator()(const InitializerSyntax &node) const = 0;
        virtual R operator()(const TypeSpecifierSyntax &node) const = 0;
        virtual R operator()(const NamedParameterSyntax &node) const = 0;
        virtual R operator()(const ArgumentListSyntax &node) const = 0;
        virtual R operator()(const ArgumentSyntax &node) const = 0;
        virtual R operator()(const ParameterListSyntax &node) const = 0;
        virtual R operator()(const ParameterSyntax &node) const = 0;
        virtual R operator()(const ExpressionBodySyntax &node) const = 0;
        virtual R operator()(const LiteralExpressionSyntax &node) const = 0;
        virtual R operator()(const IdentifierExpressionSyntax &node) const = 0;
        virtual R operator()(const ParenthesizedExpressionSyntax &node) const = 0;
        virtual R operator()(const BinaryExpressionSyntax &node) const = 0;
        virtual R operator()(const AssignmentExpressionSyntax &node) const = 0;
        virtual R operator()(const PrefixExpressionSyntax &node) const = 0;
        virtual R operator()(const PostfixExpressionSyntax &node) const = 0;
        virtual R operator()(const TernaryExpressionSyntax &node) const = 0;
        virtual R operator()(const InvocationExpressionSyntax &node) const = 0;
        virtual R operator()(const VariableDeclarationStatementSyntax &node) const = 0;
        virtual R operator()(const BlockSyntax &node) const = 0;
        virtual R operator()(const ReturnStatementSyntax &node) const = 0;
        virtual R operator()(const ExpressionStatementSyntax &node) const = 0;
        virtual R operator()(const EmptyStatementSyntax &node) const = 0;
        virtual R operator()(const IncompleteDeclarationSyntax &node) const = 0;
        virtual R operator()(const BlockNamespaceDeclarationSyntax &node) const = 0;
        virtual R operator()(const FileScopedNamespaceDeclarationSyntax &node) const = 0;
        virtual R operator()(const VariableDeclarationSyntax &node) const = 0;
        virtual R operator()(const FunctionDeclarationSyntax &node) const = 0;
        virtual R operator()(const UsingDirectiveSyntax &node) const = 0;
    };

    export template <typename R = void>
        requires(std::same_as<R, void> || std::is_default_constructible_v<R>)
    class SyntaxVisitorBase : public SyntaxVisitor<R>
    {
      public:
        constexpr R operator()(const SkippedTokensTriviaSyntax &node) const override
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

        constexpr R operator()(const CompilationUnitSyntax &node) const override
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

        constexpr R operator()(const PredefinedTypeSyntax &node) const override
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

        constexpr R operator()(const NamedTypeSyntax &node) const override
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

        constexpr R operator()(const SimpleNameSyntax &node) const override
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

        constexpr R operator()(const QualifiedNameSyntax &node) const override
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

        constexpr R operator()(const InitializerSyntax &node) const override
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

        constexpr R operator()(const TypeSpecifierSyntax &node) const override
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

        constexpr R operator()(const NamedParameterSyntax &node) const override
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

        constexpr R operator()(const ArgumentListSyntax &node) const override
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

        constexpr R operator()(const ArgumentSyntax &node) const override
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

        constexpr R operator()(const ParameterListSyntax &node) const override
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

        constexpr R operator()(const ParameterSyntax &node) const override
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

        constexpr R operator()(const ExpressionBodySyntax &node) const override
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

        constexpr R operator()(const LiteralExpressionSyntax &node) const override
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

        constexpr R operator()(const IdentifierExpressionSyntax &node) const override
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

        constexpr R operator()(const ParenthesizedExpressionSyntax &node) const override
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

        constexpr R operator()(const BinaryExpressionSyntax &node) const override
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

        constexpr R operator()(const AssignmentExpressionSyntax &node) const override
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

        constexpr R operator()(const PrefixExpressionSyntax &node) const override
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

        constexpr R operator()(const PostfixExpressionSyntax &node) const override
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

        constexpr R operator()(const TernaryExpressionSyntax &node) const override
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

        constexpr R operator()(const InvocationExpressionSyntax &node) const override
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

        constexpr R operator()(const VariableDeclarationStatementSyntax &node) const override
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

        constexpr R operator()(const BlockSyntax &node) const override
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

        constexpr R operator()(const ReturnStatementSyntax &node) const override
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

        constexpr R operator()(const ExpressionStatementSyntax &node) const override
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

        constexpr R operator()(const EmptyStatementSyntax &node) const override
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

        constexpr R operator()(const IncompleteDeclarationSyntax &node) const override
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

        constexpr R operator()(const BlockNamespaceDeclarationSyntax &node) const override
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

        constexpr R operator()(const FileScopedNamespaceDeclarationSyntax &node) const override
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

        constexpr R operator()(const VariableDeclarationSyntax &node) const override
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

        constexpr R operator()(const FunctionDeclarationSyntax &node) const override
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

        constexpr R operator()(const UsingDirectiveSyntax &node) const override
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
