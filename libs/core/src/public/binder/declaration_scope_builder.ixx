/**
 * @file declaration_scope_builder.ixx
 * @author Francesco Corso
 * @date 8/4/2026
 * @brief
 */
export module prism.core:binder.declaration_scope_builder;

import :util.noncopyable;
import :binder.declaration_scope;

namespace prism
{
    class SemanticMappings;
    class FunctionDeclarationSyntax;
    class NamespaceDeclarationSyntax;
    class QualifiedNameSyntax;
    class SimpleNameSyntax;
    class NameSyntax;
    class DiagnosticBag;
    class UsingDirectiveSyntax;
    class CompilationUnitSyntax;
    class DeclarationSyntax;
    class StatementSyntax;
    class SemanticLifetime;
    class SyntaxTree;

    class DeclarationScopeBuilder final : NonCopyable
    {
      public:
        constexpr explicit DeclarationScopeBuilder(SemanticLifetime &lifetime,
                                                   const NamespaceSymbol &global_namespace,
                                                   DiagnosticBag &diagnostics,
                                                   SemanticMappings &mappings) noexcept
            : lifetime_{lifetime}, global_namespace_{global_namespace}, diagnostics_{diagnostics}, mappings_{mappings}
        {
        }

        void add(const SyntaxTree &tree) const;

      private:
        DeclarationScope &create_scope(const SyntaxNode &owner, DeclarationScope *parent) const;

        void bind_compilation_unit(const CompilationUnitSyntax &syntax) const;

        void bind_declaration(const DeclarationSyntax &syntax, DeclarationScope &current) const;
        void bind_namespace_declaration(const NamespaceDeclarationSyntax &syntax, DeclarationScope &current) const;
        void bind_function_declaration(const FunctionDeclarationSyntax &syntax, DeclarationScope &current) const;

        [[nodiscard]] Optional<const NamespaceSymbol &> resolve_namespace(const NameSyntax &syntax) const;

        SemanticLifetime &lifetime_;
        const NamespaceSymbol &global_namespace_;
        DiagnosticBag &diagnostics_;
        SemanticMappings &mappings_;
    };
} // namespace prism
