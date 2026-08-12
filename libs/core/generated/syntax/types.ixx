module;

#include "prism/core/exports.h"

export module prism.core:syntax.types;

import :syntax.node;
import :syntax.token;
import :syntax.trivia;
import :syntax.list;
import :syntax.green.types;

namespace prism
{
    class NameSyntax;

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
            return node.kind() == SyntaxKind::predefined_type || node.kind() == SyntaxKind::named_type;
        }
    };

    export class PRISM_CORE_API PredefinedTypeSyntax final : public TypeSyntax
    {
      public:
        constexpr PredefinedTypeSyntax(SyntaxLifetime &lifetime,
                                       const GreenPredefinedType &node,
                                       const SyntaxNode *parent,
                                       const std::uint32_t position)
            : TypeSyntax{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken keyword() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::predefined_type;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;
    };

    export class PRISM_CORE_API NamedTypeSyntax final : public TypeSyntax
    {
      public:
        constexpr NamedTypeSyntax(SyntaxLifetime &lifetime,
                                  const GreenNamedType &node,
                                  const SyntaxNode *parent,
                                  const std::uint32_t position)
            : TypeSyntax{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] const NameSyntax &identifier() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::named_type;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable Lazy<const NameSyntax *> identifier_;
    };
} // namespace prism
