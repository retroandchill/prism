/**
 * @file declaration_binder.ixx
 * @author Francesco Corso
 * @date 8/1/2026
 * @brief
 */
export module prism.core:binder.declaration_binder;

import :util.noncopyable;
import :text.name;

namespace prism
{
    class SymbolLifetime;
    class SyntaxTree;
    class Symbol;
    class DeclarationSyntax;
    class AssemblySymbol;
    class SourceAssemblySymbol;
    class SourceNamespaceSymbol;

    class DeclarationBinder final : NonCopyable
    {
      public:
        DeclarationBinder(Name assembly_name, const SyntaxTree &tree, SymbolLifetime &lifetime) noexcept;

        [[nodiscard]] const AssemblySymbol &bind() const;

      private:
        [[nodiscard]] Optional<const Symbol &> bind_declaration(const DeclarationSyntax &syntax,
                                                                const Symbol &containing_symbol) const;

        const SyntaxTree &tree_;
        SymbolLifetime &lifetime_;
        SourceAssemblySymbol &assembly_symbol_;
        SourceNamespaceSymbol &global_namespace_;
    };
} // namespace prism
