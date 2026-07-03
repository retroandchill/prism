/**
 * @file declaration_syntax.ixx
 * @author Francesco Corso
 * @date 7/2/2026
 * @brief
 */
module;

#include "ast/macros.hpp"

export module prism.core.ast:declaration_syntax;

import std;
import :syntax_node;
import prism.core.util;
import prism.core.source.source_file;
import :identifier_syntax;
import :type_syntax;
import :expression_syntax;
import :statement_syntax;

namespace prism
{
    export class DeclarationSyntax : public SyntaxNode
    {
      protected:
        constexpr DeclarationSyntax(SyntaxKind kind,
                                    const SourceRange range,
                                    const SyntaxFlags flags = SyntaxFlags::none)
            : SyntaxNode{SyntaxCategory::declaration, kind, range, flags}
        {
        }

        AST_NODE_BOILERPLATE(DeclarationSyntax)
    };

    export class VariableDeclarationSyntax : public DeclarationSyntax
    {
      public:
        constexpr VariableDeclarationSyntax(const IdentifierSyntax &name,
                                            const TypeSyntax *type,
                                            bool is_mutable,
                                            Modifiers modifiers,
                                            const ExpressionSyntax *initializer,
                                            const SourceRange range,
                                            const SyntaxFlags flags = SyntaxFlags::none)
            : DeclarationSyntax{SyntaxKind::variable_declaration, range, flags}, name_{name}, type_{type},
              is_mutable_{is_mutable}, modifiers_{modifiers}, initializer_{initializer}
        {
        }

        OWNED_REF_PROPERTY(IdentifierSyntax, name)
        VALUE_PROPERTY(const TypeSyntax *, type)
        VALUE_PROPERTY(bool, is_mutable)
        VALUE_PROPERTY(Modifiers, modifiers)
        VALUE_PROPERTY(const ExpressionSyntax *, initializer)
    };

    export class ParameterDeclarationSyntax : public DeclarationSyntax
    {
      public:
        constexpr ParameterDeclarationSyntax(const IdentifierSyntax &name,
                                             const TypeSyntax &type,
                                             bool is_mutable,
                                             const SourceRange range,
                                             const SyntaxFlags flags = SyntaxFlags::none)
            : DeclarationSyntax{SyntaxKind::parameter_declaration, range, flags}, name_{name}, type_{type},
              is_mutable_{is_mutable}
        {
        }

        OWNED_REF_PROPERTY(IdentifierSyntax, name)
        UNOWNED_REF_PROPERTY(TypeSyntax, type)
        VALUE_PROPERTY(bool, is_mutable)
    };

    export using FunctionBodySyntax =
        std::variant<std::monostate, Ref<const BlockStatementSyntax>, Ref<const ExpressionSyntax>>;

    export class FunctionDeclarationSyntax : public DeclarationSyntax
    {
      public:
        constexpr FunctionDeclarationSyntax(const IdentifierSyntax &name,
                                            const TypeSyntax *return_type,
                                            std::span<const Ref<const ParameterDeclarationSyntax>> parameters,
                                            Modifiers modifiers,
                                            FunctionBodySyntax body,
                                            const SourceRange range,
                                            const SyntaxFlags flags = SyntaxFlags::none)
            : DeclarationSyntax{SyntaxKind::function_declaration, range, flags}, name_{name}, return_type_{return_type},
              parameters_{parameters}, body_{body}, modifiers_{modifiers}
        {
        }

        OWNED_REF_PROPERTY(IdentifierSyntax, name)
        VALUE_PROPERTY(const TypeSyntax *, return_type)
        VALUE_PROPERTY(std::span<const Ref<const ParameterDeclarationSyntax>>, parameters)
        VALUE_PROPERTY(FunctionBodySyntax, body)
        VALUE_PROPERTY(Modifiers, modifiers)
    };

    export class EmptyDeclarationSyntax : public DeclarationSyntax
    {
      public:
        constexpr explicit EmptyDeclarationSyntax(const SourceRange range, const SyntaxFlags flags = SyntaxFlags::none)
            : DeclarationSyntax{SyntaxKind::empty_declaration, range, flags}
        {
        }
    };
} // namespace prism
