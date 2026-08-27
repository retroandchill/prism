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

        [[nodiscard]] constexpr NamespaceKind namespace_kind() const noexcept override
        {
            return NamespaceKind::compilation;
        }

        [[nodiscard]] const ImmutableArray<Location> &locations() const override;
        [[nodiscard]] std::span<const SyntaxReference> declaring_syntax_references() const override;
        [[nodiscard]] Optional<const Compilation &> containing_compilation() const noexcept override;
        [[nodiscard]] SymbolSpan<Symbol> members() const override;
        [[nodiscard]] SymbolSpan<Symbol> members(Name name) const override;

      protected:
        void force_complete(const Optional<SourceLocation> &location,
                            const Optional<SymbolPredicate> &filter) const override;

      private:
        [[nodiscard]] ImmutableArray<Ref<const Symbol>> compute_members(Name name) const;

        const Compilation &compilation_;
        std::vector<Ref<const NamespaceSymbol>> namespaces_{};
        mutable Lazy<ImmutableArray<Location>> locations_;
        mutable Lazy<std::vector<Ref<const Symbol>>> members_;
        mutable std::mutex name_to_members_mutex_;
        mutable std::unordered_map<Name, Lazy<ImmutableArray<Ref<const Symbol>>>> name_to_members_;
        mutable Lazy<std::vector<SyntaxReference>> syntax_references_;
    };
} // namespace prism
