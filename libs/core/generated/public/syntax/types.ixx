module;

#include "prism/core/exports.h"

export module prism.core:syntax.types;

import :syntax.node;
import :syntax.token;
import :syntax.list;
import :syntax.green.types;

namespace prism
{
    export class PRISM_CORE_API TypeSyntax : public SyntaxNode
    {
      protected:
        constexpr TypeSyntax(SyntaxLifetime &lifetime,
                             const GreenType &node,
                             const SyntaxNode *parent,
                             const std::uint32_t position)
            : SyntaxNode{lifetime, node, parent, position}
        {
        }

        ~TypeSyntax() = default;

      public:
        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::identifier_named_type;
        }
    };

    export class PRISM_CORE_API NamedTypeSyntax : public TypeSyntax
    {
      protected:
        constexpr NamedTypeSyntax(SyntaxLifetime &lifetime,
                                  const GreenNamedType &node,
                                  const SyntaxNode *parent,
                                  const std::uint32_t position)
            : TypeSyntax{lifetime, node, parent, position}
        {
        }

        ~NamedTypeSyntax() = default;

      public:
        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::identifier_named_type;
        }
    };

    export class PRISM_CORE_API SimpleNamedTypeSyntax : public NamedTypeSyntax
    {
      protected:
        constexpr SimpleNamedTypeSyntax(SyntaxLifetime &lifetime,
                                        const GreenSimpleNamedType &node,
                                        const SyntaxNode *parent,
                                        const std::uint32_t position)
            : NamedTypeSyntax{lifetime, node, parent, position}
        {
        }

        ~SimpleNamedTypeSyntax() = default;

      public:
        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::identifier_named_type;
        }
    };

    export class PRISM_CORE_API IdentifierNamedTypeSyntax final : public SimpleNamedTypeSyntax
    {
      public:
        constexpr IdentifierNamedTypeSyntax(SyntaxLifetime &lifetime,
                                            const GreenIdentifierNamedType &node,
                                            const SyntaxNode *parent,
                                            const std::uint32_t position)
            : SimpleNamedTypeSyntax{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken identifier() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::identifier_named_type;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;
    };
} // namespace prism
