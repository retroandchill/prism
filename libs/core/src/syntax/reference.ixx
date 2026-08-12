/**
 * @file reference.ixx
 * @author Francesco Corso
 * @date 8/4/2026
 * @brief
 */
export module prism.core:syntax.reference;

import :syntax.node;

namespace prism
{
    export class SyntaxReference final
    {
      public:
        explicit(false) constexpr SyntaxReference(const SyntaxNode &node) : node_{&node}
        {
        }

        [[nodiscard]] constexpr const SyntaxNode &syntax() const noexcept
        {
            return *node_;
        }

        [[nodiscard]] constexpr const SyntaxTree &tree() const
        {
            return node_->tree();
        }

        [[nodiscard]] constexpr TextSpan span() const noexcept
        {
            return node_->span();
        }

      private:
        const SyntaxNode *node_;
    };
} // namespace prism
