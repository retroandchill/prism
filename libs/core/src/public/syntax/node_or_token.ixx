/**
 * @file node_or_token.ixx
 * @author Francesco Corso
 * @date 7/22/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

#include <libassert/assert-macros.hpp>

export module prism.core:syntax.node_or_token;

import :syntax.node;
import :syntax.token;

namespace prism
{
    export using SyntaxNodeRef = std::reference_wrapper<const SyntaxNode>;
    export using SyntaxNodeOrToken = std::variant<SyntaxNodeRef, SyntaxToken>;

    export class PRISM_CORE_API SyntaxNodeOrTokenList final : public SyntaxListView<SyntaxNodeOrToken>
    {
      public:
        constexpr SyntaxNodeOrTokenList() = default;

        constexpr explicit SyntaxNodeOrTokenList(const SyntaxNode &node) : node_{&node}
        {
        }

        [[nodiscard]] constexpr std::size_t size() const noexcept
        {
            if (node_ == nullptr)
                return 0;

            return node_->green().is_list() ? node_->green().child_count() : 1;
        }

        [[nodiscard]] SyntaxNodeOrToken operator[](std::size_t index) const;

        [[nodiscard]] constexpr Optional<const SyntaxNode &> node() const noexcept
        {
            return node_;
        }

      private:
        [[nodiscard]] constexpr Optional<const SyntaxNode &> parent() const
        {
            ASSUME(node_ != nullptr);
            return node_->parent();
        }

        const SyntaxNode *node_ = nullptr;
    };
} // namespace prism
