module;

#include "prism/core/exports.h"

export module prism.core:syntax.clauses;

import :syntax.node;
import :syntax.token;
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
        constexpr InitializerSyntax(const GreenInitializer &node, const SyntaxTree &tree, const std::uint32_t position)
            : SyntaxNode{node, tree, position}
        {
        }

        constexpr InitializerSyntax(const GreenInitializer &node,
                                    const SyntaxNode &parent,
                                    const std::uint32_t position)
            : SyntaxNode{node, parent, position}
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
        mutable std::atomic<const ExpressionSyntax *> value_;
    };

    export class PRISM_CORE_API TypeSpecifierSyntax final : public SyntaxNode
    {
      public:
        constexpr TypeSpecifierSyntax(const GreenTypeSpecifier &node,
                                      const SyntaxTree &tree,
                                      const std::uint32_t position)
            : SyntaxNode{node, tree, position}
        {
        }

        constexpr TypeSpecifierSyntax(const GreenTypeSpecifier &node,
                                      const SyntaxNode &parent,
                                      const std::uint32_t position)
            : SyntaxNode{node, parent, position}
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
        mutable std::atomic<const TypeSyntax *> type_;
    };

    export class PRISM_CORE_API NamedParameterSyntax final : public SyntaxNode
    {
      public:
        constexpr NamedParameterSyntax(const GreenNamedParameter &node,
                                       const SyntaxTree &tree,
                                       const std::uint32_t position)
            : SyntaxNode{node, tree, position}
        {
        }

        constexpr NamedParameterSyntax(const GreenNamedParameter &node,
                                       const SyntaxNode &parent,
                                       const std::uint32_t position)
            : SyntaxNode{node, parent, position}
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
        constexpr ArgumentListSyntax(const GreenArgumentList &node,
                                     const SyntaxTree &tree,
                                     const std::uint32_t position)
            : SyntaxNode{node, tree, position}
        {
        }

        constexpr ArgumentListSyntax(const GreenArgumentList &node,
                                     const SyntaxNode &parent,
                                     const std::uint32_t position)
            : SyntaxNode{node, parent, position}
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
        mutable std::atomic<const SyntaxNode *> arguments_;
    };

    class PRISM_CORE_API ArgumentSyntax final : public SyntaxNode
    {
      public:
        constexpr ArgumentSyntax(const GreenArgument &node, const SyntaxTree &tree, const std::uint32_t position)
            : SyntaxNode{node, tree, position}
        {
        }

        constexpr ArgumentSyntax(const GreenArgument &node, const SyntaxNode &parent, const std::uint32_t position)
            : SyntaxNode{node, parent, position}
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
        mutable std::atomic<const NamedParameterSyntax *> name_;
        mutable std::atomic<const ExpressionSyntax *> value_;
    };

    export class PRISM_CORE_API ParameterListSyntax final : public SyntaxNode
    {
      public:
        constexpr ParameterListSyntax(const GreenParameterList &node,
                                      const SyntaxTree &tree,
                                      const std::uint32_t position)
            : SyntaxNode{node, tree, position}
        {
        }

        constexpr ParameterListSyntax(const GreenParameterList &node,
                                      const SyntaxNode &parent,
                                      const std::uint32_t position)
            : SyntaxNode{node, parent, position}
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
        mutable std::atomic<const SyntaxNode *> parameters_;
    };

    class PRISM_CORE_API ParameterSyntax final : public SyntaxNode
    {
      public:
        constexpr ParameterSyntax(const GreenParameter &node, const SyntaxTree &tree, const std::uint32_t position)
            : SyntaxNode{node, tree, position}
        {
        }

        constexpr ParameterSyntax(const GreenParameter &node, const SyntaxNode &parent, const std::uint32_t position)
            : SyntaxNode{node, parent, position}
        {
        }

        [[nodiscard]] Optional<SyntaxToken> mut_keyword() const;
        [[nodiscard]] SyntaxToken name() const;
        [[nodiscard]] const TypeSpecifierSyntax &type_specifier() const;
        [[nodiscard]] Optional<const InitializerSyntax &> default_value() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::parameter;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable std::atomic<const TypeSpecifierSyntax *> type_specifier_;
        mutable std::atomic<const InitializerSyntax *> default_value_;
    };

    export class PRISM_CORE_API ExpressionBodySyntax final : public SyntaxNode
    {
      public:
        constexpr ExpressionBodySyntax(const GreenExpressionBody &node,
                                       const SyntaxTree &tree,
                                       const std::uint32_t position)
            : SyntaxNode{node, tree, position}
        {
        }

        constexpr ExpressionBodySyntax(const GreenExpressionBody &node,
                                       const SyntaxNode &parent,
                                       const std::uint32_t position)
            : SyntaxNode{node, parent, position}
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
        mutable std::atomic<const ExpressionSyntax *> expression_;
    };
} // namespace prism
