module;

#include "prism/core/exports.h"

export module prism.core:syntax.statements;

import :syntax.node;
import :syntax.token;
import :syntax.list;
import :syntax.green.statements;

namespace prism
{
    class ExpressionSyntax;
    class VariableDeclarationSyntax;

    export class PRISM_CORE_API StatementSyntax : public SyntaxNode
    {
      protected:
        constexpr StatementSyntax(const GreenStatement &node, const SyntaxTree &tree, const std::uint32_t position)
            : SyntaxNode{node, tree, position}
        {
        }

        constexpr StatementSyntax(const GreenStatement &node, const SyntaxNode &parent, const std::uint32_t position)
            : SyntaxNode{node, parent, position}
        {
        }

        ~StatementSyntax() = default;

      public:
        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::variable_declaration_statement || node.kind() == SyntaxKind::block ||
                   node.kind() == SyntaxKind::return_statement || node.kind() == SyntaxKind::expression_statement ||
                   node.kind() == SyntaxKind::empty_statement;
        }
    };

    export class PRISM_CORE_API VariableDeclarationStatementSyntax final : public StatementSyntax
    {
      public:
        constexpr VariableDeclarationStatementSyntax(const GreenVariableDeclarationStatement &node,
                                                     const SyntaxTree &tree,
                                                     const std::uint32_t position)
            : StatementSyntax{node, tree, position}
        {
        }

        constexpr VariableDeclarationStatementSyntax(const GreenVariableDeclarationStatement &node,
                                                     const SyntaxNode &parent,
                                                     const std::uint32_t position)
            : StatementSyntax{node, parent, position}
        {
        }

        [[nodiscard]] const VariableDeclarationSyntax &declaration() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::variable_declaration_statement;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable std::atomic<const VariableDeclarationSyntax *> declaration_;
    };

    export class PRISM_CORE_API BlockSyntax final : public StatementSyntax
    {
      public:
        constexpr BlockSyntax(const GreenBlock &node, const SyntaxTree &tree, const std::uint32_t position)
            : StatementSyntax{node, tree, position}
        {
        }

        constexpr BlockSyntax(const GreenBlock &node, const SyntaxNode &parent, const std::uint32_t position)
            : StatementSyntax{node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken open_brace() const;
        [[nodiscard]] SyntaxList<StatementSyntax> statements() const;
        [[nodiscard]] SyntaxToken close_brace() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::block;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable std::atomic<const SyntaxNode *> statements_;
    };

    export class PRISM_CORE_API ReturnStatementSyntax final : public StatementSyntax
    {
      public:
        constexpr ReturnStatementSyntax(const GreenReturnStatement &node,
                                        const SyntaxTree &tree,
                                        const std::uint32_t position)
            : StatementSyntax{node, tree, position}
        {
        }

        constexpr ReturnStatementSyntax(const GreenReturnStatement &node,
                                        const SyntaxNode &parent,
                                        const std::uint32_t position)
            : StatementSyntax{node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken return_keyword() const;
        [[nodiscard]] Optional<const ExpressionSyntax &> expression() const;
        [[nodiscard]] SyntaxToken semicolon() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::return_statement;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable std::atomic<const ExpressionSyntax *> expression_;
    };

    export class PRISM_CORE_API ExpressionStatementSyntax final : public StatementSyntax
    {
      public:
        constexpr ExpressionStatementSyntax(const GreenExpressionStatement &node,
                                            const SyntaxTree &tree,
                                            const std::uint32_t position)
            : StatementSyntax{node, tree, position}
        {
        }

        constexpr ExpressionStatementSyntax(const GreenExpressionStatement &node,
                                            const SyntaxNode &parent,
                                            const std::uint32_t position)
            : StatementSyntax{node, parent, position}
        {
        }

        [[nodiscard]] const ExpressionSyntax &expression() const;
        [[nodiscard]] SyntaxToken semicolon() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::expression_statement;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable std::atomic<const ExpressionSyntax *> expression_;
    };

    export class PRISM_CORE_API EmptyStatementSyntax final : public StatementSyntax
    {
      public:
        constexpr EmptyStatementSyntax(const GreenEmptyStatement &node,
                                       const SyntaxTree &tree,
                                       const std::uint32_t position)
            : StatementSyntax{node, tree, position}
        {
        }

        constexpr EmptyStatementSyntax(const GreenEmptyStatement &node,
                                       const SyntaxNode &parent,
                                       const std::uint32_t position)
            : StatementSyntax{node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken semicolon() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::empty_statement;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;
    };
} // namespace prism
