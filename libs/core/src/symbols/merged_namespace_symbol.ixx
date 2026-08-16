/**
 * @file merged_namespace_symbol.ixx
 * @author Francesco Corso
 * @date 8/5/2026
 * @brief
 */
export module prism.core:symbols.merged_namespace_symbol;

import :symbols.namespace_symbol;
import :util.lazy;

namespace prism
{

    class MergedNamespaceSymbol final : public NamespaceSymbol
    {
      public:
        constexpr MergedNamespaceSymbol(const Name &name,
                                        const Symbol *containing,
                                        const Compilation &compilation,
                                        std::vector<Ref<const NamespaceSymbol>> namespaces)
            : NamespaceSymbol(name, containing), compilation_(compilation), namespaces_{std::move(namespaces)}
        {
        }

        [[nodiscard]] static const NamespaceSymbol &create(const Compilation &compilation,
                                                           const NamespaceSymbol *containing_namespace,
                                                           std::vector<Ref<const NamespaceSymbol>> namespaces);

        [[nodiscard]] const ImmutableArray<Location> &locations() const override;
        [[nodiscard]] std::span<const SyntaxReference> declaring_syntax_references() const override;
        [[nodiscard]] Optional<const Compilation &> containing_compilation() const noexcept override;
        [[nodiscard]] SymbolSpan<Symbol> members() const override;

      private:
        const Compilation &compilation_;
        std::vector<Ref<const NamespaceSymbol>> namespaces_{};
        mutable Lazy<ImmutableArray<Location>> locations_;
        mutable Lazy<std::vector<Ref<const Symbol>>> members_;
        mutable Lazy<std::vector<SyntaxReference>> syntax_references_;
    };
} // namespace prism
