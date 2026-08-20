/**
 * @file bound_expression.cpp
 * @author Francesco Corso
 * @date 8/20/2026
 * @brief
 */
module prism.core:semantic.bound.bound_expression.impl;

import :semantic.bound.bound_expression;

namespace prism
{

    const Optional<ConstantValue> &BoundExpression::constant_value() const
    {
        return constant_value_.get_or_compute([this] { return compute_constant_value(); });
    }

    Optional<ConstantValue> BoundExpression::compute_constant_value() const
    {
        return std::nullopt;
    }

    Optional<ConstantValue> BoundLiteral::compute_constant_value() const
    {
        return value_;
    }
} // namespace prism
