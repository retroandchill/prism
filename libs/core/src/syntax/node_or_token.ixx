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

import :syntax.token;
import :util.overload;
import :util.ref;

namespace prism
{
    export using SyntaxNodeRef = Ref<const SyntaxNode>;
    export using SyntaxNodeOrToken = std::variant<SyntaxNodeRef, SyntaxToken>;

    Optional<const SyntaxTree &> get_tree(const SyntaxNodeOrToken &node_or_token) noexcept;

    TextSpan get_span(const SyntaxNodeOrToken &node_or_token) noexcept;

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

        [[nodiscard]] std::size_t size() const noexcept;

        [[nodiscard]] SyntaxNodeOrToken operator[](std::size_t index) const;

        [[nodiscard]] constexpr Optional<const SyntaxNode &> node() const noexcept
        {
            return node_;
        }

      private:
        friend class SyntaxNodeInternal;

        [[nodiscard]] Optional<const SyntaxNode &> parent() const;

        const SyntaxNode *node_ = nullptr;
    };
} // namespace prism
