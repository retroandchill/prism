module;

#include "prism/core/exports.h"

export module prism.core:syntax.names;

import :syntax.node;
import :syntax.token;
import :syntax.trivia;
import :syntax.list;
import :syntax.green.names;

namespace prism
{
    export class PRISM_CORE_API NameSyntax : public SyntaxNode
    {
      protected:
        constexpr NameSyntax(SyntaxLifetime &lifetime,
                             const GreenName &node,
                             const SyntaxNode *parent,
                             const std::uint32_t position)
            : SyntaxNode{lifetime, node, parent, position}
        {
        }

        ~NameSyntax() = default;

      public:
        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::simple_name || node.kind() == SyntaxKind::qualified_name;
        }
    };

    export class PRISM_CORE_API SimpleNameSyntax final : public NameSyntax
    {
      public:
        constexpr SimpleNameSyntax(SyntaxLifetime &lifetime,
                                   const GreenSimpleName &node,
                                   const SyntaxNode *parent,
                                   const std::uint32_t position)
            : NameSyntax{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken identifier() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::simple_name;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;
    };

    export class PRISM_CORE_API QualifiedNameSyntax final : public NameSyntax
    {
      public:
        constexpr QualifiedNameSyntax(SyntaxLifetime &lifetime,
                                      const GreenQualifiedName &node,
                                      const SyntaxNode *parent,
                                      const std::uint32_t position)
            : NameSyntax{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] const NameSyntax &left() const;
        [[nodiscard]] SyntaxToken separator() const;
        [[nodiscard]] const SimpleNameSyntax &right() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::qualified_name;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable Lazy<const NameSyntax *> left_;
        mutable Lazy<const SimpleNameSyntax *> right_;
    };
} // namespace prism
