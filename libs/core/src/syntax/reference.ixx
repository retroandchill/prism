/**
 * @file reference.ixx
 * @author Francesco Corso
 * @date 8/4/2026
 * @brief
 */
export module prism.core:syntax.reference;

import :syntax.node;
import :syntax.lifetime;

namespace prism
{
    export class SyntaxReference final
    {
      public:
        explicit(false) constexpr SyntaxReference(const SyntaxNode &node) : node_{node.shared_from_this()}
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
        std::shared_ptr<const SyntaxNode> node_{};
    };
} // namespace prism
