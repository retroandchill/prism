module;

#include "prism/core/exports.h"

export module prism.core:syntax.clauses;

import :syntax.node;
import :syntax.token;
import :syntax.trivia;
import :syntax.list;
import :syntax.green.clauses;

namespace prism
{
    export class ArgumentSyntax;
    class ExpressionSyntax;
    export class ParameterSyntax;
    class TypeSyntax;

    export class PRISM_CORE_API InitializerSyntax final : public SyntaxNode
    {
      public:
        constexpr InitializerSyntax(SyntaxLifetime &lifetime,
                                    const GreenInitializer &node,
                                    const SyntaxNode *parent,
                                    const std::uint32_t position)
            : SyntaxNode{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken equal_sign() const;
        [[nodiscard]] const ExpressionSyntax &value() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::initializer;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable Lazy<const ExpressionSyntax *> value_;
    };

    export class PRISM_CORE_API TypeSpecifierSyntax final : public SyntaxNode
    {
      public:
        constexpr TypeSpecifierSyntax(SyntaxLifetime &lifetime,
                                      const GreenTypeSpecifier &node,
                                      const SyntaxNode *parent,
                                      const std::uint32_t position)
            : SyntaxNode{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken colon() const;
        [[nodiscard]] const TypeSyntax &type() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::type_specifier;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable Lazy<const TypeSyntax *> type_;
    };

    export class PRISM_CORE_API NamedParameterSyntax final : public SyntaxNode
    {
      public:
        constexpr NamedParameterSyntax(SyntaxLifetime &lifetime,
                                       const GreenNamedParameter &node,
                                       const SyntaxNode *parent,
                                       const std::uint32_t position)
            : SyntaxNode{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken name() const;
        [[nodiscard]] SyntaxToken colon() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::named_parameter;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;
    };

    export class PRISM_CORE_API ArgumentListSyntax final : public SyntaxNode
    {
      public:
        constexpr ArgumentListSyntax(SyntaxLifetime &lifetime,
                                     const GreenArgumentList &node,
                                     const SyntaxNode *parent,
                                     const std::uint32_t position)
            : SyntaxNode{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken open_paren() const;
        [[nodiscard]] SeparatedSyntaxList<ArgumentSyntax> arguments() const;
        [[nodiscard]] SyntaxToken close_paren() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::argument_list;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable Lazy<const SyntaxNode *> arguments_;
    };

    class PRISM_CORE_API ArgumentSyntax final : public SyntaxNode
    {
      public:
        constexpr ArgumentSyntax(SyntaxLifetime &lifetime,
                                 const GreenArgument &node,
                                 const SyntaxNode *parent,
                                 const std::uint32_t position)
            : SyntaxNode{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] Optional<const NamedParameterSyntax &> name() const;
        [[nodiscard]] const ExpressionSyntax &value() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::argument;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable Lazy<const NamedParameterSyntax *> name_;
        mutable Lazy<const ExpressionSyntax *> value_;
    };

    export class PRISM_CORE_API ParameterListSyntax final : public SyntaxNode
    {
      public:
        constexpr ParameterListSyntax(SyntaxLifetime &lifetime,
                                      const GreenParameterList &node,
                                      const SyntaxNode *parent,
                                      const std::uint32_t position)
            : SyntaxNode{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken open_paren() const;
        [[nodiscard]] SeparatedSyntaxList<ParameterSyntax> parameters() const;
        [[nodiscard]] SyntaxToken close_paren() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::parameter_list;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable Lazy<const SyntaxNode *> parameters_;
    };

    class PRISM_CORE_API ParameterSyntax final : public SyntaxNode
    {
      public:
        constexpr ParameterSyntax(SyntaxLifetime &lifetime,
                                  const GreenParameter &node,
                                  const SyntaxNode *parent,
                                  const std::uint32_t position)
            : SyntaxNode{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] Optional<SyntaxToken> mut_keyword() const;
        [[nodiscard]] SyntaxToken name() const;
        [[nodiscard]] Optional<const TypeSpecifierSyntax &> type_specifier() const;
        [[nodiscard]] Optional<const InitializerSyntax &> default_value() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::parameter;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable Lazy<const TypeSpecifierSyntax *> type_specifier_;
        mutable Lazy<const InitializerSyntax *> default_value_;
    };

    export class PRISM_CORE_API ExpressionBodySyntax final : public SyntaxNode
    {
      public:
        constexpr ExpressionBodySyntax(SyntaxLifetime &lifetime,
                                       const GreenExpressionBody &node,
                                       const SyntaxNode *parent,
                                       const std::uint32_t position)
            : SyntaxNode{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken arrow() const;
        [[nodiscard]] const ExpressionSyntax &expression() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::expression_body;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable Lazy<const ExpressionSyntax *> expression_;
    };
} // namespace prism
