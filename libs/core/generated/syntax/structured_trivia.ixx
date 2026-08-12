module;

#include "prism/core/exports.h"

export module prism.core:syntax.structured_trivia;

import :syntax.node;
import :syntax.token;
import :syntax.trivia;
import :syntax.list;
import :syntax.green.structured_trivia;

namespace prism
{
    export class PRISM_CORE_API SkippedTokensTriviaSyntax final : public StructuredTriviaSyntax
    {
      public:
        constexpr SkippedTokensTriviaSyntax(SyntaxLifetime &lifetime,
                                            const GreenSkippedTokensTrivia &node,
                                            const SyntaxNode *parent,
                                            const std::uint32_t position)
            : StructuredTriviaSyntax{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxTokenList tokens() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::skipped_tokens_trivia;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;
    };
} // namespace prism
