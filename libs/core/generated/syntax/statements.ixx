module;

#include "prism/core/exports.h"

export module prism.core:syntax.statements;

import :syntax.node;
import :syntax.token;
import :syntax.trivia;
import :syntax.list;
import :syntax.green.statements;

namespace prism
{
    class ElseClauseSyntax;
    class ExpressionSyntax;
    class VariableDeclarationSyntax;

    export class PRISM_CORE_API StatementSyntax : public SyntaxNode
    {
      protected:
        constexpr StatementSyntax(SyntaxLifetime &lifetime,
                                  const GreenStatement &node,
                                  const SyntaxNode *parent,
                                  const std::uint32_t position)
            : SyntaxNode{lifetime, node, parent, position}
        {
        }

        ~StatementSyntax() = default;

      public:
        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::empty_statement ||
                   node.kind() == SyntaxKind::variable_declaration_statement || node.kind() == SyntaxKind::block ||
                   node.kind() == SyntaxKind::return_statement || node.kind() == SyntaxKind::expression_statement ||
                   node.kind() == SyntaxKind::if_statement || node.kind() == SyntaxKind::while_statement ||
                   node.kind() == SyntaxKind::loop_statement || node.kind() == SyntaxKind::for_statement ||
                   node.kind() == SyntaxKind::break_statement || node.kind() == SyntaxKind::continue_statement;
        }
    };

    export class PRISM_CORE_API EmptyStatementSyntax final : public StatementSyntax
    {
      public:
        constexpr EmptyStatementSyntax(SyntaxLifetime &lifetime,
                                       const GreenEmptyStatement &node,
                                       const SyntaxNode *parent,
                                       const std::uint32_t position)
            : StatementSyntax{lifetime, node, parent, position}
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

    export class PRISM_CORE_API VariableDeclarationStatementSyntax final : public StatementSyntax
    {
      public:
        constexpr VariableDeclarationStatementSyntax(SyntaxLifetime &lifetime,
                                                     const GreenVariableDeclarationStatement &node,
                                                     const SyntaxNode *parent,
                                                     const std::uint32_t position)
            : StatementSyntax{lifetime, node, parent, position}
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
        mutable Lazy<const VariableDeclarationSyntax *> declaration_;
    };

    export class PRISM_CORE_API BlockSyntax final : public StatementSyntax
    {
      public:
        constexpr BlockSyntax(SyntaxLifetime &lifetime,
                              const GreenBlock &node,
                              const SyntaxNode *parent,
                              const std::uint32_t position)
            : StatementSyntax{lifetime, node, parent, position}
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
        mutable Lazy<const SyntaxNode *> statements_;
    };

    export class PRISM_CORE_API ReturnStatementSyntax final : public StatementSyntax
    {
      public:
        constexpr ReturnStatementSyntax(SyntaxLifetime &lifetime,
                                        const GreenReturnStatement &node,
                                        const SyntaxNode *parent,
                                        const std::uint32_t position)
            : StatementSyntax{lifetime, node, parent, position}
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
        mutable Lazy<const ExpressionSyntax *> expression_;
    };

    export class PRISM_CORE_API ExpressionStatementSyntax final : public StatementSyntax
    {
      public:
        constexpr ExpressionStatementSyntax(SyntaxLifetime &lifetime,
                                            const GreenExpressionStatement &node,
                                            const SyntaxNode *parent,
                                            const std::uint32_t position)
            : StatementSyntax{lifetime, node, parent, position}
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
        mutable Lazy<const ExpressionSyntax *> expression_;
    };

    export class PRISM_CORE_API IfStatementSyntax final : public StatementSyntax
    {
      public:
        constexpr IfStatementSyntax(SyntaxLifetime &lifetime,
                                    const GreenIfStatement &node,
                                    const SyntaxNode *parent,
                                    const std::uint32_t position)
            : StatementSyntax{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken if_keyword() const;
        [[nodiscard]] SyntaxToken open_paren() const;
        [[nodiscard]] const ExpressionSyntax &condition() const;
        [[nodiscard]] SyntaxToken close_paren() const;
        [[nodiscard]] const BlockSyntax &block() const;
        [[nodiscard]] Optional<const ElseClauseSyntax &> else_clause() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::if_statement;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable Lazy<const ExpressionSyntax *> condition_;
        mutable Lazy<const BlockSyntax *> block_;
        mutable Lazy<const ElseClauseSyntax *> else_clause_;
    };

    export class PRISM_CORE_API WhileStatementSyntax final : public StatementSyntax
    {
      public:
        constexpr WhileStatementSyntax(SyntaxLifetime &lifetime,
                                       const GreenWhileStatement &node,
                                       const SyntaxNode *parent,
                                       const std::uint32_t position)
            : StatementSyntax{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken while_keyword() const;
        [[nodiscard]] SyntaxToken open_paren() const;
        [[nodiscard]] const ExpressionSyntax &condition() const;
        [[nodiscard]] SyntaxToken close_paren() const;
        [[nodiscard]] const BlockSyntax &block() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::while_statement;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable Lazy<const ExpressionSyntax *> condition_;
        mutable Lazy<const BlockSyntax *> block_;
    };

    export class PRISM_CORE_API LoopStatementSyntax final : public StatementSyntax
    {
      public:
        constexpr LoopStatementSyntax(SyntaxLifetime &lifetime,
                                      const GreenLoopStatement &node,
                                      const SyntaxNode *parent,
                                      const std::uint32_t position)
            : StatementSyntax{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken loop_keyword() const;
        [[nodiscard]] const BlockSyntax &block() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::loop_statement;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable Lazy<const BlockSyntax *> block_;
    };

    export class PRISM_CORE_API ForStatementSyntax final : public StatementSyntax
    {
      public:
        constexpr ForStatementSyntax(SyntaxLifetime &lifetime,
                                     const GreenForStatement &node,
                                     const SyntaxNode *parent,
                                     const std::uint32_t position)
            : StatementSyntax{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken for_keyword() const;
        [[nodiscard]] SyntaxToken open_paren() const;
        [[nodiscard]] Optional<const VariableDeclarationStatementSyntax &> declaration() const;
        [[nodiscard]] SeparatedSyntaxList<ExpressionSyntax> initializers() const;
        [[nodiscard]] SyntaxToken first_semicolon() const;
        [[nodiscard]] Optional<const ExpressionSyntax &> condition() const;
        [[nodiscard]] SyntaxToken second_semicolon() const;
        [[nodiscard]] SeparatedSyntaxList<ExpressionSyntax> incrementors() const;
        [[nodiscard]] SyntaxToken close_paren() const;
        [[nodiscard]] const BlockSyntax &block() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::for_statement;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable Lazy<const VariableDeclarationStatementSyntax *> declaration_;
        mutable Lazy<const SyntaxNode *> initializers_;
        mutable Lazy<const ExpressionSyntax *> condition_;
        mutable Lazy<const SyntaxNode *> incrementors_;
        mutable Lazy<const BlockSyntax *> block_;
    };

    export class PRISM_CORE_API BreakStatementSyntax final : public StatementSyntax
    {
      public:
        constexpr BreakStatementSyntax(SyntaxLifetime &lifetime,
                                       const GreenBreakStatement &node,
                                       const SyntaxNode *parent,
                                       const std::uint32_t position)
            : StatementSyntax{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken keyword() const;
        [[nodiscard]] SyntaxToken semicolon() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::break_statement;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;
    };

    export class PRISM_CORE_API ContinueStatementSyntax final : public StatementSyntax
    {
      public:
        constexpr ContinueStatementSyntax(SyntaxLifetime &lifetime,
                                          const GreenContinueStatement &node,
                                          const SyntaxNode *parent,
                                          const std::uint32_t position)
            : StatementSyntax{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken keyword() const;
        [[nodiscard]] SyntaxToken semicolon() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::continue_statement;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;
    };
} // namespace prism
