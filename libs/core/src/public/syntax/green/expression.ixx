/**
 * @file expression.ixx
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
export module prism.core:syntax.green.expression;

import :syntax.green.node;

namespace prism
{
    class GreenExpression : public GreenNode
    {
      protected:
        explicit constexpr GreenExpression(const SyntaxKind kind) : GreenNode{kind}
        {
        }
    };
} // namespace prism
