module prism.core:syntax.expressions.impl;

import :syntax.expressions;
import :syntax.clauses;

namespace prism
{
    SyntaxToken LiteralExpressionSyntax::value() const
    {
        return SyntaxToken{static_cast<const GreenLiteralExpression &>(green()).value(), this, position()};
    }

    Optional<const SyntaxNode &> LiteralExpressionSyntax::get_node_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    Optional<const SyntaxNode &> LiteralExpressionSyntax::get_cached_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    SyntaxToken IdentifierExpressionSyntax::value() const
    {
        return SyntaxToken{static_cast<const GreenIdentifierExpression &>(green()).value(), this, position()};
    }

    Optional<const SyntaxNode &> IdentifierExpressionSyntax::get_node_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    Optional<const SyntaxNode &> IdentifierExpressionSyntax::get_cached_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    SyntaxToken ParenthesizedExpressionSyntax::open() const
    {
        return SyntaxToken{static_cast<const GreenParenthesizedExpression &>(green()).open(), this, position()};
    }

    const ExpressionSyntax &ParenthesizedExpressionSyntax::expression() const
    {
        return *get_red(expression_, 1);
    }

    SyntaxToken ParenthesizedExpressionSyntax::close() const
    {
        return SyntaxToken{static_cast<const GreenParenthesizedExpression &>(green()).close(),
                           this,
                           get_child_position(2)};
    }

    Optional<const SyntaxNode &> ParenthesizedExpressionSyntax::get_node_slot(const std::size_t index) const
    {
        return index == 1 ? get_red(expression_) : std::nullopt;
    }

    Optional<const SyntaxNode &> ParenthesizedExpressionSyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 1 ? Optional<const SyntaxNode &>{expression_.load(std::memory_order_acquire)} : std::nullopt;
    }

    const ExpressionSyntax &BinaryExpressionSyntax::left() const
    {
        return *get_red(left_);
    }

    SyntaxToken BinaryExpressionSyntax::op() const
    {
        return SyntaxToken{static_cast<const GreenBinaryExpression &>(green()).op(), this, get_child_position(1)};
    }

    const ExpressionSyntax &BinaryExpressionSyntax::right() const
    {
        return *get_red(right_, 2);
    }

    Optional<const SyntaxNode &> BinaryExpressionSyntax::get_node_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return get_red(left_);
            case 2:
                return get_red(right_, 2);
            default:
                return std::nullopt;
        }
    }

    Optional<const SyntaxNode &> BinaryExpressionSyntax::get_cached_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return left_.load(std::memory_order_acquire);
            case 2:
                return right_.load(std::memory_order_acquire);
            default:
                return std::nullopt;
        }
    }

    const ExpressionSyntax &AssignmentExpressionSyntax::left() const
    {
        return *get_red(left_);
    }

    SyntaxToken AssignmentExpressionSyntax::op() const
    {
        return SyntaxToken{static_cast<const GreenAssignmentExpression &>(green()).op(), this, get_child_position(1)};
    }

    const ExpressionSyntax &AssignmentExpressionSyntax::right() const
    {
        return *get_red(right_, 2);
    }

    Optional<const SyntaxNode &> AssignmentExpressionSyntax::get_node_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return get_red(left_);
            case 2:
                return get_red(right_, 2);
            default:
                return std::nullopt;
        }
    }

    Optional<const SyntaxNode &> AssignmentExpressionSyntax::get_cached_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return left_.load(std::memory_order_acquire);
            case 2:
                return right_.load(std::memory_order_acquire);
            default:
                return std::nullopt;
        }
    }

    SyntaxToken PrefixExpressionSyntax::op() const
    {
        return SyntaxToken{static_cast<const GreenPrefixExpression &>(green()).op(), this, position()};
    }

    const ExpressionSyntax &PrefixExpressionSyntax::operand() const
    {
        return *get_red(operand_, 1);
    }

    Optional<const SyntaxNode &> PrefixExpressionSyntax::get_node_slot(const std::size_t index) const
    {
        return index == 1 ? get_red(operand_) : std::nullopt;
    }

    Optional<const SyntaxNode &> PrefixExpressionSyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 1 ? Optional<const SyntaxNode &>{operand_.load(std::memory_order_acquire)} : std::nullopt;
    }

    const ExpressionSyntax &PostfixExpressionSyntax::operand() const
    {
        return *get_red(operand_);
    }

    SyntaxToken PostfixExpressionSyntax::op() const
    {
        return SyntaxToken{static_cast<const GreenPostfixExpression &>(green()).op(), this, get_child_position(1)};
    }

    Optional<const SyntaxNode &> PostfixExpressionSyntax::get_node_slot(const std::size_t index) const
    {
        return index == 0 ? get_red(operand_) : std::nullopt;
    }

    Optional<const SyntaxNode &> PostfixExpressionSyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 0 ? Optional<const SyntaxNode &>{operand_.load(std::memory_order_acquire)} : std::nullopt;
    }

    const ExpressionSyntax &TernaryExpressionSyntax::condition() const
    {
        return *get_red(condition_);
    }

    SyntaxToken TernaryExpressionSyntax::question_mark() const
    {
        return SyntaxToken{static_cast<const GreenTernaryExpression &>(green()).question_mark(),
                           this,
                           get_child_position(1)};
    }

    const ExpressionSyntax &TernaryExpressionSyntax::when_true() const
    {
        return *get_red(when_true_, 2);
    }

    SyntaxToken TernaryExpressionSyntax::colon() const
    {
        return SyntaxToken{static_cast<const GreenTernaryExpression &>(green()).colon(), this, get_child_position(3)};
    }

    const ExpressionSyntax &TernaryExpressionSyntax::when_false() const
    {
        return *get_red(when_false_, 4);
    }

    Optional<const SyntaxNode &> TernaryExpressionSyntax::get_node_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return get_red(condition_);
            case 2:
                return get_red(when_true_, 2);
            case 4:
                return get_red(when_false_, 4);
            default:
                return std::nullopt;
        }
    }

    Optional<const SyntaxNode &> TernaryExpressionSyntax::get_cached_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return condition_.load(std::memory_order_acquire);
            case 2:
                return when_true_.load(std::memory_order_acquire);
            case 4:
                return when_false_.load(std::memory_order_acquire);
            default:
                return std::nullopt;
        }
    }

    const ExpressionSyntax &InvocationExpressionSyntax::callee() const
    {
        return *get_red(callee_);
    }

    const ArgumentListSyntax &InvocationExpressionSyntax::arguments() const
    {
        return *get_red(arguments_, 1);
    }

    Optional<const SyntaxNode &> InvocationExpressionSyntax::get_node_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return get_red(callee_);
            case 1:
                return get_red(arguments_, 1);
            default:
                return std::nullopt;
        }
    }

    Optional<const SyntaxNode &> InvocationExpressionSyntax::get_cached_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return callee_.load(std::memory_order_acquire);
            case 1:
                return arguments_.load(std::memory_order_acquire);
            default:
                return std::nullopt;
        }
    }
} // namespace prism
