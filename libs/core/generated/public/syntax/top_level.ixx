module;

#include "prism/core/exports.h"

export module prism.core:syntax.top_level;

import :syntax.node;
import :syntax.token;
import :syntax.list;
import :syntax.green.top_level;

namespace prism
{
    class DeclarationSyntax;

    export class PRISM_CORE_API CompilationUnitSyntax final : public SyntaxNode
    {
      public:
        constexpr CompilationUnitSyntax(SyntaxLifetime &lifetime,
                                        const GreenCompilationUnit &node,
                                        const SyntaxNode *parent,
                                        const std::uint32_t position)
            : SyntaxNode{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxList<DeclarationSyntax> members() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::compilation_unit;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable std::atomic<const SyntaxNode *> members_;
    };
} // namespace prism
