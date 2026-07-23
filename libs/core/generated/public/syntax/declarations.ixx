module;

#include "prism/core/exports.h"

export module prism.core:syntax.declarations;

import :syntax.node;
import :syntax.token;
import :syntax.list;
import :syntax.green.declarations;

namespace prism
{
    class BlockSyntax;
    class ExpressionBodySyntax;
    class InitializerSyntax;
    class ParameterListSyntax;
    class TypeSpecifierSyntax;

    export class PRISM_CORE_API DeclarationSyntax : public SyntaxNode
    {
      protected:
        constexpr DeclarationSyntax(const GreenDeclaration &node, const SyntaxTree &tree, const std::uint32_t position)
            : SyntaxNode{node, tree, position}
        {
        }

        constexpr DeclarationSyntax(const GreenDeclaration &node,
                                    const SyntaxNode &parent,
                                    const std::uint32_t position)
            : SyntaxNode{node, parent, position}
        {
        }

        ~DeclarationSyntax() = default;

      public:
        [[nodiscard]] virtual SyntaxTokenList modifiers() const = 0;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::variable_declaration || node.kind() == SyntaxKind::function_declaration;
        }
    };

    export class PRISM_CORE_API VariableDeclarationSyntax final : public DeclarationSyntax
    {
      public:
        constexpr VariableDeclarationSyntax(const GreenVariableDeclaration &node,
                                            const SyntaxTree &tree,
                                            const std::uint32_t position)
            : DeclarationSyntax{node, tree, position}
        {
        }

        constexpr VariableDeclarationSyntax(const GreenVariableDeclaration &node,
                                            const SyntaxNode &parent,
                                            const std::uint32_t position)
            : DeclarationSyntax{node, parent, position}
        {
        }

        [[nodiscard]] SyntaxTokenList modifiers() const override;
        [[nodiscard]] SyntaxToken var_keyword() const;
        [[nodiscard]] Optional<SyntaxToken> mut_keyword() const;
        [[nodiscard]] SyntaxToken identifier() const;
        [[nodiscard]] const TypeSpecifierSyntax &type() const;
        [[nodiscard]] const InitializerSyntax &initializer() const;
        [[nodiscard]] SyntaxToken semicolon() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::variable_declaration;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable std::atomic<const TypeSpecifierSyntax *> type_;
        mutable std::atomic<const InitializerSyntax *> initializer_;
    };

    export class PRISM_CORE_API FunctionDeclarationSyntax final : public DeclarationSyntax
    {
      public:
        constexpr FunctionDeclarationSyntax(const GreenFunctionDeclaration &node,
                                            const SyntaxTree &tree,
                                            const std::uint32_t position)
            : DeclarationSyntax{node, tree, position}
        {
        }

        constexpr FunctionDeclarationSyntax(const GreenFunctionDeclaration &node,
                                            const SyntaxNode &parent,
                                            const std::uint32_t position)
            : DeclarationSyntax{node, parent, position}
        {
        }

        [[nodiscard]] SyntaxTokenList modifiers() const override;
        [[nodiscard]] SyntaxToken func_keyword() const;
        [[nodiscard]] SyntaxToken identifier() const;
        [[nodiscard]] const ParameterListSyntax &parameters() const;
        [[nodiscard]] Optional<const TypeSpecifierSyntax &> return_type() const;
        [[nodiscard]] Optional<const BlockSyntax &> body() const;
        [[nodiscard]] Optional<const ExpressionBodySyntax &> expression_body() const;
        [[nodiscard]] Optional<SyntaxToken> semicolon() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::function_declaration;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable std::atomic<const ParameterListSyntax *> parameters_;
        mutable std::atomic<const TypeSpecifierSyntax *> return_type_;
        mutable std::atomic<const BlockSyntax *> body_;
        mutable std::atomic<const ExpressionBodySyntax *> expression_body_;
    };
} // namespace prism
