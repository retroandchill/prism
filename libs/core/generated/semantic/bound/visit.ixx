module;

#include <libassert/assert-macros.hpp>

export module prism.core:semantic.bound.visit;

import :semantic.bound.bound_node;
import :semantic.bound.bound_statement;
import :semantic.bound.bound_expression;

namespace prism
{
    template <typename Functor>
    concept VisitorForBoundNode = ExhaustiveVisitor<Functor,
                                                    BoundBlock,
                                                    BoundVariableDeclaration,
                                                    BoundExpressionStatement,
                                                    BoundReturnStatement,
                                                    BoundIfStatement,
                                                    BoundWhileStatement,
                                                    BoundLoopStatement,
                                                    BoundForStatement,
                                                    BoundBreakStatement,
                                                    BoundContinueStatement,
                                                    BoundBadExpression,
                                                    BoundLiteral,
                                                    BoundVariableAccess,
                                                    BoundParameterAccess,
                                                    BoundUnaryExpression,
                                                    BoundBinaryExpression,
                                                    BoundAssignmentExpression,
                                                    BoundConditionalExpression,
                                                    BoundCallExpression,
                                                    BoundConversionExpression>;

    template <VisitorForBoundNode Functor>
    constexpr decltype(auto) visit(const BoundNode &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case BoundNodeKind::block:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundBlock &>(node));
            case BoundNodeKind::variable_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundVariableDeclaration &>(node));
            case BoundNodeKind::expression_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundExpressionStatement &>(node));
            case BoundNodeKind::return_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundReturnStatement &>(node));
            case BoundNodeKind::if_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundIfStatement &>(node));
            case BoundNodeKind::while_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundWhileStatement &>(node));
            case BoundNodeKind::loop_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundLoopStatement &>(node));
            case BoundNodeKind::for_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundForStatement &>(node));
            case BoundNodeKind::break_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundBreakStatement &>(node));
            case BoundNodeKind::continue_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundContinueStatement &>(node));
            case BoundNodeKind::bad_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundBadExpression &>(node));
            case BoundNodeKind::literal:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundLiteral &>(node));
            case BoundNodeKind::variable_access:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundVariableAccess &>(node));
            case BoundNodeKind::parameter_access:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundParameterAccess &>(node));
            case BoundNodeKind::unary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundUnaryExpression &>(node));
            case BoundNodeKind::binary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundBinaryExpression &>(node));
            case BoundNodeKind::assignment_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const BoundAssignmentExpression &>(node));
            case BoundNodeKind::conditional_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const BoundConditionalExpression &>(node));
            case BoundNodeKind::call_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundCallExpression &>(node));
            case BoundNodeKind::conversion_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const BoundConversionExpression &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept VisitorForBoundNodeReturning = ConvertibleVisitor<Functor,
                                                              R,
                                                              BoundBlock,
                                                              BoundVariableDeclaration,
                                                              BoundExpressionStatement,
                                                              BoundReturnStatement,
                                                              BoundIfStatement,
                                                              BoundWhileStatement,
                                                              BoundLoopStatement,
                                                              BoundForStatement,
                                                              BoundBreakStatement,
                                                              BoundContinueStatement,
                                                              BoundBadExpression,
                                                              BoundLiteral,
                                                              BoundVariableAccess,
                                                              BoundParameterAccess,
                                                              BoundUnaryExpression,
                                                              BoundBinaryExpression,
                                                              BoundAssignmentExpression,
                                                              BoundConditionalExpression,
                                                              BoundCallExpression,
                                                              BoundConversionExpression>;

    template <typename R, VisitorForBoundNodeReturning<R> Functor>
    constexpr R visit(const BoundNode &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case BoundNodeKind::block:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundBlock &>(node));
            case BoundNodeKind::variable_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundVariableDeclaration &>(node));
            case BoundNodeKind::expression_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundExpressionStatement &>(node));
            case BoundNodeKind::return_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundReturnStatement &>(node));
            case BoundNodeKind::if_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundIfStatement &>(node));
            case BoundNodeKind::while_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundWhileStatement &>(node));
            case BoundNodeKind::loop_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundLoopStatement &>(node));
            case BoundNodeKind::for_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundForStatement &>(node));
            case BoundNodeKind::break_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundBreakStatement &>(node));
            case BoundNodeKind::continue_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundContinueStatement &>(node));
            case BoundNodeKind::bad_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundBadExpression &>(node));
            case BoundNodeKind::literal:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundLiteral &>(node));
            case BoundNodeKind::variable_access:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundVariableAccess &>(node));
            case BoundNodeKind::parameter_access:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundParameterAccess &>(node));
            case BoundNodeKind::unary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundUnaryExpression &>(node));
            case BoundNodeKind::binary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundBinaryExpression &>(node));
            case BoundNodeKind::assignment_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const BoundAssignmentExpression &>(node));
            case BoundNodeKind::conditional_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const BoundConditionalExpression &>(node));
            case BoundNodeKind::call_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundCallExpression &>(node));
            case BoundNodeKind::conversion_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const BoundConversionExpression &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor>
    concept VisitorForBoundStatement = ExhaustiveVisitor<Functor,
                                                         BoundBlock,
                                                         BoundVariableDeclaration,
                                                         BoundExpressionStatement,
                                                         BoundReturnStatement,
                                                         BoundIfStatement,
                                                         BoundWhileStatement,
                                                         BoundLoopStatement,
                                                         BoundForStatement,
                                                         BoundBreakStatement,
                                                         BoundContinueStatement>;

    template <VisitorForBoundStatement Functor>
    constexpr decltype(auto) visit(const BoundStatement &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case BoundNodeKind::block:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundBlock &>(node));
            case BoundNodeKind::variable_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundVariableDeclaration &>(node));
            case BoundNodeKind::expression_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundExpressionStatement &>(node));
            case BoundNodeKind::return_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundReturnStatement &>(node));
            case BoundNodeKind::if_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundIfStatement &>(node));
            case BoundNodeKind::while_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundWhileStatement &>(node));
            case BoundNodeKind::loop_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundLoopStatement &>(node));
            case BoundNodeKind::for_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundForStatement &>(node));
            case BoundNodeKind::break_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundBreakStatement &>(node));
            case BoundNodeKind::continue_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundContinueStatement &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept VisitorForBoundStatementReturning = ConvertibleVisitor<Functor,
                                                                   R,
                                                                   BoundBlock,
                                                                   BoundVariableDeclaration,
                                                                   BoundExpressionStatement,
                                                                   BoundReturnStatement,
                                                                   BoundIfStatement,
                                                                   BoundWhileStatement,
                                                                   BoundLoopStatement,
                                                                   BoundForStatement,
                                                                   BoundBreakStatement,
                                                                   BoundContinueStatement>;

    template <typename R, VisitorForBoundStatementReturning<R> Functor>
    constexpr R visit(const BoundStatement &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case BoundNodeKind::block:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundBlock &>(node));
            case BoundNodeKind::variable_declaration:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundVariableDeclaration &>(node));
            case BoundNodeKind::expression_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundExpressionStatement &>(node));
            case BoundNodeKind::return_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundReturnStatement &>(node));
            case BoundNodeKind::if_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundIfStatement &>(node));
            case BoundNodeKind::while_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundWhileStatement &>(node));
            case BoundNodeKind::loop_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundLoopStatement &>(node));
            case BoundNodeKind::for_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundForStatement &>(node));
            case BoundNodeKind::break_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundBreakStatement &>(node));
            case BoundNodeKind::continue_statement:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundContinueStatement &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor>
    concept VisitorForBoundExpression = ExhaustiveVisitor<Functor,
                                                          BoundBadExpression,
                                                          BoundLiteral,
                                                          BoundVariableAccess,
                                                          BoundParameterAccess,
                                                          BoundUnaryExpression,
                                                          BoundBinaryExpression,
                                                          BoundAssignmentExpression,
                                                          BoundConditionalExpression,
                                                          BoundCallExpression,
                                                          BoundConversionExpression>;

    template <VisitorForBoundExpression Functor>
    constexpr decltype(auto) visit(const BoundExpression &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case BoundNodeKind::bad_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundBadExpression &>(node));
            case BoundNodeKind::literal:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundLiteral &>(node));
            case BoundNodeKind::variable_access:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundVariableAccess &>(node));
            case BoundNodeKind::parameter_access:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundParameterAccess &>(node));
            case BoundNodeKind::unary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundUnaryExpression &>(node));
            case BoundNodeKind::binary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundBinaryExpression &>(node));
            case BoundNodeKind::assignment_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const BoundAssignmentExpression &>(node));
            case BoundNodeKind::conditional_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const BoundConditionalExpression &>(node));
            case BoundNodeKind::call_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundCallExpression &>(node));
            case BoundNodeKind::conversion_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const BoundConversionExpression &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }

    template <typename Functor, typename R>
    concept VisitorForBoundExpressionReturning = ConvertibleVisitor<Functor,
                                                                    R,
                                                                    BoundBadExpression,
                                                                    BoundLiteral,
                                                                    BoundVariableAccess,
                                                                    BoundParameterAccess,
                                                                    BoundUnaryExpression,
                                                                    BoundBinaryExpression,
                                                                    BoundAssignmentExpression,
                                                                    BoundConditionalExpression,
                                                                    BoundCallExpression,
                                                                    BoundConversionExpression>;

    template <typename R, VisitorForBoundExpressionReturning<R> Functor>
    constexpr R visit(const BoundExpression &node, Functor &&functor)
    {
        switch (node.kind())
        {
            case BoundNodeKind::bad_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundBadExpression &>(node));
            case BoundNodeKind::literal:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundLiteral &>(node));
            case BoundNodeKind::variable_access:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundVariableAccess &>(node));
            case BoundNodeKind::parameter_access:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundParameterAccess &>(node));
            case BoundNodeKind::unary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundUnaryExpression &>(node));
            case BoundNodeKind::binary_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundBinaryExpression &>(node));
            case BoundNodeKind::assignment_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const BoundAssignmentExpression &>(node));
            case BoundNodeKind::conditional_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const BoundConditionalExpression &>(node));
            case BoundNodeKind::call_expression:
                return std::invoke(std::forward<Functor>(functor), static_cast<const BoundCallExpression &>(node));
            case BoundNodeKind::conversion_expression:
                return std::invoke(std::forward<Functor>(functor),
                                   static_cast<const BoundConversionExpression &>(node));
            default:
                UNREACHABLE("Invalid node type passed into visit");
        }
    }
} // namespace prism
