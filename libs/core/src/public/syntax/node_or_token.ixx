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
import :util.overload;
import :util.ref;

namespace prism
{
    export using SyntaxNodeRef = Ref<const SyntaxNode>;
    export using SyntaxNodeOrToken = std::variant<SyntaxNodeRef, SyntaxToken>;

    export constexpr Optional<const SyntaxTree &> get_tree(const SyntaxNodeOrToken &node_or_token) noexcept
    {
        return std::visit(Overload{[](const SyntaxNode &node) -> Optional<const SyntaxTree &> { return node.tree(); },
                                   [](const SyntaxToken &token)
                                   {
                                       return token.tree();
                                   }},
                          node_or_token);
    }

    export constexpr TextSpan get_span(const SyntaxNodeOrToken &node_or_token) noexcept
    {
        return std::visit(Overload{[](const SyntaxNode &node) { return node.span(); },
                                   [](const SyntaxToken &token)
                                   {
                                       return token.span();
                                   }},
                          node_or_token);
    }

    export class PRISM_CORE_API SyntaxNodeOrTokenList final : public SyntaxListView<SyntaxNodeOrToken>
    {
        constexpr explicit SyntaxNodeOrTokenList(const SyntaxNode *node) : node_{node}
        {
        }

      public:
        constexpr SyntaxNodeOrTokenList() = default;

        constexpr explicit SyntaxNodeOrTokenList(const SyntaxNode &node) : node_{&node}
        {
        }

        [[nodiscard]] constexpr std::size_t size() const noexcept
        {
            if (node_ == nullptr)
                return 0;

            return node_->green().is_list() ? node_->green().slot_count() : 1;
        }

        [[nodiscard]] SyntaxNodeOrToken operator[](std::size_t index) const;

        [[nodiscard]] constexpr Optional<const SyntaxNode &> node() const noexcept
        {
            return node_;
        }

      private:
        friend class SyntaxNode;

        [[nodiscard]] constexpr Optional<const SyntaxNode &> parent() const
        {
            ASSUME(node_ != nullptr);
            return node_->parent();
        }

        const SyntaxNode *node_ = nullptr;
    };
} // namespace prism
