module prism.core:syntax.expressions.impl;

import :syntax.expressions;
import :syntax.clauses;
import :syntax.names;

namespace prism
{
    SyntaxToken LiteralExpressionSyntax::value() const
    {
        return SyntaxToken{static_cast<const GreenLiteralExpression &>(SyntaxNodeInternal::get_green(*this)).value(),
                           this,
                           position()};
    }

    Optional<const SyntaxNode &> LiteralExpressionSyntax::get_node_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    Optional<const SyntaxNode &> LiteralExpressionSyntax::get_cached_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    const NameSyntax &IdentifierExpressionSyntax::value() const
    {
        return *get_red(value_);
    }

    Optional<const SyntaxNode &> IdentifierExpressionSyntax::get_node_slot(const std::size_t index) const
    {
        return index == 0 ? get_red(value_) : std::nullopt;
    }

    Optional<const SyntaxNode &> IdentifierExpressionSyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 0 ? Optional<const SyntaxNode &>{value_.try_get_value(nullptr)} : std::nullopt;
    }

    SyntaxToken ParenthesizedExpressionSyntax::open() const
    {
        return SyntaxToken{
            static_cast<const GreenParenthesizedExpression &>(SyntaxNodeInternal::get_green(*this)).open(),
            this,
            position()};
    }

    const ExpressionSyntax &ParenthesizedExpressionSyntax::expression() const
    {
        return *get_red(expression_, 1);
    }

    SyntaxToken ParenthesizedExpressionSyntax::close() const
    {
        return SyntaxToken{
            static_cast<const GreenParenthesizedExpression &>(SyntaxNodeInternal::get_green(*this)).close(),
            this,
            get_slot_position(2)};
    }

    Optional<const SyntaxNode &> ParenthesizedExpressionSyntax::get_node_slot(const std::size_t index) const
    {
        return index == 1 ? get_red(expression_) : std::nullopt;
    }

    Optional<const SyntaxNode &> ParenthesizedExpressionSyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 1 ? Optional<const SyntaxNode &>{expression_.try_get_value(nullptr)} : std::nullopt;
    }

    const ExpressionSyntax &BinaryExpressionSyntax::left() const
    {
        return *get_red(left_);
    }

    SyntaxToken BinaryExpressionSyntax::op() const
    {
        return SyntaxToken{static_cast<const GreenBinaryExpression &>(SyntaxNodeInternal::get_green(*this)).op(),
                           this,
                           get_slot_position(1)};
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
                return left_.try_get_value(nullptr);
            case 2:
                return right_.try_get_value(nullptr);
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
        return SyntaxToken{static_cast<const GreenAssignmentExpression &>(SyntaxNodeInternal::get_green(*this)).op(),
                           this,
                           get_slot_position(1)};
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
                return left_.try_get_value(nullptr);
            case 2:
                return right_.try_get_value(nullptr);
            default:
                return std::nullopt;
        }
    }

    SyntaxToken PrefixExpressionSyntax::op() const
    {
        return SyntaxToken{static_cast<const GreenPrefixExpression &>(SyntaxNodeInternal::get_green(*this)).op(),
                           this,
                           position()};
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
        return index == 1 ? Optional<const SyntaxNode &>{operand_.try_get_value(nullptr)} : std::nullopt;
    }

    const ExpressionSyntax &PostfixExpressionSyntax::operand() const
    {
        return *get_red(operand_);
    }

    SyntaxToken PostfixExpressionSyntax::op() const
    {
        return SyntaxToken{static_cast<const GreenPostfixExpression &>(SyntaxNodeInternal::get_green(*this)).op(),
                           this,
                           get_slot_position(1)};
    }

    Optional<const SyntaxNode &> PostfixExpressionSyntax::get_node_slot(const std::size_t index) const
    {
        return index == 0 ? get_red(operand_) : std::nullopt;
    }

    Optional<const SyntaxNode &> PostfixExpressionSyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 0 ? Optional<const SyntaxNode &>{operand_.try_get_value(nullptr)} : std::nullopt;
    }

    const ExpressionSyntax &TernaryExpressionSyntax::condition() const
    {
        return *get_red(condition_);
    }

    SyntaxToken TernaryExpressionSyntax::question_mark() const
    {
        return SyntaxToken{
            static_cast<const GreenTernaryExpression &>(SyntaxNodeInternal::get_green(*this)).question_mark(),
            this,
            get_slot_position(1)};
    }

    const ExpressionSyntax &TernaryExpressionSyntax::when_true() const
    {
        return *get_red(when_true_, 2);
    }

    SyntaxToken TernaryExpressionSyntax::colon() const
    {
        return SyntaxToken{static_cast<const GreenTernaryExpression &>(SyntaxNodeInternal::get_green(*this)).colon(),
                           this,
                           get_slot_position(3)};
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
                return condition_.try_get_value(nullptr);
            case 2:
                return when_true_.try_get_value(nullptr);
            case 4:
                return when_false_.try_get_value(nullptr);
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
                return callee_.try_get_value(nullptr);
            case 1:
                return arguments_.try_get_value(nullptr);
            default:
                return std::nullopt;
        }
    }
} // namespace prism
