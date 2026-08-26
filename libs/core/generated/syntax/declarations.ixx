module;

#include "prism/core/exports.h"

export module prism.core:syntax.declarations;

import :syntax.node;
import :syntax.token;
import :syntax.trivia;
import :syntax.list;
import :syntax.green.declarations;

namespace prism
{
    class BlockSyntax;
    class ExpressionBodySyntax;
    class InitializerSyntax;
    class NameSyntax;
    class ParameterListSyntax;
    class TypeSpecifierSyntax;
    class UsingDirectiveSyntax;

    export class PRISM_CORE_API DeclarationSyntax : public SyntaxNode
    {
      protected:
        constexpr DeclarationSyntax(SyntaxLifetime &lifetime,
                                    const GreenDeclaration &node,
                                    const SyntaxNode *parent,
                                    const std::uint32_t position)
            : SyntaxNode{lifetime, node, parent, position}
        {
        }

        ~DeclarationSyntax() = default;

      public:
        [[nodiscard]] virtual SyntaxTokenList modifiers() const = 0;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::incomplete_declaration ||
                   node.kind() == SyntaxKind::block_namespace_declaration ||
                   node.kind() == SyntaxKind::file_scoped_namespace_declaration ||
                   node.kind() == SyntaxKind::variable_declaration || node.kind() == SyntaxKind::function_declaration;
        }
    };

    export class PRISM_CORE_API IncompleteDeclarationSyntax final : public DeclarationSyntax
    {
      public:
        constexpr IncompleteDeclarationSyntax(SyntaxLifetime &lifetime,
                                              const GreenIncompleteDeclaration &node,
                                              const SyntaxNode *parent,
                                              const std::uint32_t position)
            : DeclarationSyntax{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxTokenList modifiers() const override;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::incomplete_declaration;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;
    };

    export class PRISM_CORE_API NamespaceDeclarationSyntax : public DeclarationSyntax
    {
      protected:
        constexpr NamespaceDeclarationSyntax(SyntaxLifetime &lifetime,
                                             const GreenNamespaceDeclaration &node,
                                             const SyntaxNode *parent,
                                             const std::uint32_t position)
            : DeclarationSyntax{lifetime, node, parent, position}
        {
        }

        ~NamespaceDeclarationSyntax() = default;

      public:
        [[nodiscard]] virtual SyntaxToken namespace_token() const = 0;
        [[nodiscard]] virtual const NameSyntax &name() const = 0;
        [[nodiscard]] virtual SyntaxList<UsingDirectiveSyntax> usings() const = 0;
        [[nodiscard]] virtual SyntaxList<DeclarationSyntax> members() const = 0;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::block_namespace_declaration ||
                   node.kind() == SyntaxKind::file_scoped_namespace_declaration;
        }
    };

    export class PRISM_CORE_API BlockNamespaceDeclarationSyntax final : public NamespaceDeclarationSyntax
    {
      public:
        constexpr BlockNamespaceDeclarationSyntax(SyntaxLifetime &lifetime,
                                                  const GreenBlockNamespaceDeclaration &node,
                                                  const SyntaxNode *parent,
                                                  const std::uint32_t position)
            : NamespaceDeclarationSyntax{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxTokenList modifiers() const override;
        [[nodiscard]] SyntaxToken namespace_token() const override;
        [[nodiscard]] const NameSyntax &name() const override;
        [[nodiscard]] SyntaxToken open_brace() const;
        [[nodiscard]] SyntaxList<UsingDirectiveSyntax> usings() const override;
        [[nodiscard]] SyntaxList<DeclarationSyntax> members() const override;
        [[nodiscard]] SyntaxToken close_brace() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::block_namespace_declaration;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable Lazy<const NameSyntax *> name_;
        mutable Lazy<const SyntaxNode *> usings_;
        mutable Lazy<const SyntaxNode *> members_;
    };

    export class PRISM_CORE_API FileScopedNamespaceDeclarationSyntax final : public NamespaceDeclarationSyntax
    {
      public:
        constexpr FileScopedNamespaceDeclarationSyntax(SyntaxLifetime &lifetime,
                                                       const GreenFileScopedNamespaceDeclaration &node,
                                                       const SyntaxNode *parent,
                                                       const std::uint32_t position)
            : NamespaceDeclarationSyntax{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxTokenList modifiers() const override;
        [[nodiscard]] SyntaxToken namespace_token() const override;
        [[nodiscard]] const NameSyntax &name() const override;
        [[nodiscard]] SyntaxToken semicolon() const;
        [[nodiscard]] SyntaxList<UsingDirectiveSyntax> usings() const override;
        [[nodiscard]] SyntaxList<DeclarationSyntax> members() const override;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::file_scoped_namespace_declaration;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable Lazy<const NameSyntax *> name_;
        mutable Lazy<const SyntaxNode *> usings_;
        mutable Lazy<const SyntaxNode *> members_;
    };

    export class PRISM_CORE_API VariableDeclarationSyntax final : public DeclarationSyntax
    {
      public:
        constexpr VariableDeclarationSyntax(SyntaxLifetime &lifetime,
                                            const GreenVariableDeclaration &node,
                                            const SyntaxNode *parent,
                                            const std::uint32_t position)
            : DeclarationSyntax{lifetime, node, parent, position}
        {
        }

        [[nodiscard]] SyntaxTokenList modifiers() const override;
        [[nodiscard]] SyntaxToken var_keyword() const;
        [[nodiscard]] SyntaxToken identifier() const;
        [[nodiscard]] Optional<const TypeSpecifierSyntax &> type() const;
        [[nodiscard]] Optional<const InitializerSyntax &> initializer() const;
        [[nodiscard]] SyntaxToken semicolon() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::variable_declaration;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable Lazy<const TypeSpecifierSyntax *> type_;
        mutable Lazy<const InitializerSyntax *> initializer_;
    };

    export class PRISM_CORE_API FunctionDeclarationSyntax final : public DeclarationSyntax
    {
      public:
        constexpr FunctionDeclarationSyntax(SyntaxLifetime &lifetime,
                                            const GreenFunctionDeclaration &node,
                                            const SyntaxNode *parent,
                                            const std::uint32_t position)
            : DeclarationSyntax{lifetime, node, parent, position}
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
        mutable Lazy<const ParameterListSyntax *> parameters_;
        mutable Lazy<const TypeSpecifierSyntax *> return_type_;
        mutable Lazy<const BlockSyntax *> body_;
        mutable Lazy<const ExpressionBodySyntax *> expression_body_;
    };
} // namespace prism
