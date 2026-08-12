module;

#include "prism/core/exports.h"

export module prism.core:syntax.directives;

import :syntax.node;
import :syntax.token;
import :syntax.trivia;
import :syntax.list;
import :syntax.green.directives;

namespace prism
{
    class NameSyntax;

    export class PRISM_CORE_API UsingDirectiveSyntax final : public SyntaxNode
    {
      public:
        constexpr UsingDirectiveSyntax(SyntaxLifetime &lifetime,
                                       const GreenUsingDirective &node,
                                       const SyntaxNode *parent,
                                       const std::uint32_t position)
            : SyntaxNode{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken using_keyword() const;
        [[nodiscard]] const NameSyntax &name() const;
        [[nodiscard]] SyntaxToken semicolon() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::using_directive;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable Lazy<const NameSyntax *> name_;
    };
} // namespace prism
