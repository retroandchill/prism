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
        constexpr TypeSyntax(const GreenType &node, const SyntaxTree &tree, const std::uint32_t position)
            : SyntaxNode{node, tree, position}
        {
        }

        constexpr TypeSyntax(const GreenType &node, const SyntaxNode &parent, const std::uint32_t position)
            : SyntaxNode{node, parent, position}
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
        constexpr NamedTypeSyntax(const GreenNamedType &node, const SyntaxTree &tree, const std::uint32_t position)
            : TypeSyntax{node, tree, position}
        {
        }

        constexpr NamedTypeSyntax(const GreenNamedType &node, const SyntaxNode &parent, const std::uint32_t position)
            : TypeSyntax{node, parent, position}
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
        constexpr SimpleNamedTypeSyntax(const GreenSimpleNamedType &node,
                                        const SyntaxTree &tree,
                                        const std::uint32_t position)
            : NamedTypeSyntax{node, tree, position}
        {
        }

        constexpr SimpleNamedTypeSyntax(const GreenSimpleNamedType &node,
                                        const SyntaxNode &parent,
                                        const std::uint32_t position)
            : NamedTypeSyntax{node, parent, position}
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
        constexpr IdentifierNamedTypeSyntax(const GreenIdentifierNamedType &node,
                                            const SyntaxTree &tree,
                                            const std::uint32_t position)
            : SimpleNamedTypeSyntax{node, tree, position}
        {
        }

        constexpr IdentifierNamedTypeSyntax(const GreenIdentifierNamedType &node,
                                            const SyntaxNode &parent,
                                            const std::uint32_t position)
            : SimpleNamedTypeSyntax{node, parent, position}
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
