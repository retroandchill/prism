/**
 * @file initializer.cpp
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
module prism.core:syntax.green.initializer.impl;

import :syntax.green.initializer;
import :syntax.green.token;
import :syntax.green.expression;

namespace prism
{

    GreenInitializer::GreenInitializer(GreenPtr<GreenToken> equal_sign, GreenPtr<GreenExpression> expression)
        : GreenNode{SyntaxKind::initializer_node}, equal_sign_{std::move(equal_sign)},
          expression_{std::move(expression)}
    {
        set_child_count(2);
        adjust_flags_and_width(*equal_sign_);
        adjust_flags_and_width(*expression_);
    }
    Optional<const GreenNode &> GreenInitializer::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *equal_sign_;
            case 1:
                return *expression_;
            default:
                return std::nullopt;
        }
    }
} // namespace prism
