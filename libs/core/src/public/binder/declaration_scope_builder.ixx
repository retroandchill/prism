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
                                                   NamespaceSymbol &global_namespace,
                                                   DiagnosticBag &diagnostics) noexcept
            : lifetime_{lifetime}, global_namespace_{global_namespace}, diagnostics_{diagnostics}
        {
        }

        void add(const SyntaxTree &tree);

        [[nodiscard]] constexpr DeclarationScopeMap build() && noexcept
        {
            return std::move(scopes_);
        }

      private:
        DeclarationScope &create_scope(const SyntaxNode &owner, DeclarationScope *parent);

        void bind_compilation_unit(const CompilationUnitSyntax &syntax);
        void bind_declaration(const DeclarationSyntax &syntax, DeclarationScope &current);
        void bind_statement(const StatementSyntax &syntax, DeclarationScope &current);

        Optional<const Symbol &> resolve_symbol(const NameSyntax &syntax);
        Optional<const Symbol &> resolve_symbol(Name syntax, const Symbol &ns);

        SemanticLifetime &lifetime_;
        NamespaceSymbol &global_namespace_;
        DiagnosticBag &diagnostics_;
        DeclarationScopeMap scopes_;
    };
} // namespace prism
