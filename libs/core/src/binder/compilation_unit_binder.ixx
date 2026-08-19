/**
 * @file compilation_unit_binder.ixx
 * @author Francesco Corso
 * @date 8/18/2026
 * @brief
 */
export module prism.core:binder.compilation_unit_binder;

import :binder;

namespace prism
{
    class CompilationUnitSyntax;

    class CompilationUnitBinder final : public Binder
    {
      public:
        explicit CompilationUnitBinder(const Binder &next, const CompilationUnitSyntax &syntax);

        [[nodiscard]] Optional<const SyntaxNode &> scope_designator() const override;

        [[nodiscard]] Optional<const Symbol &> containing_symbol() const override;

      protected:
        [[nodiscard]] LookupResult lookup_local(Name name,
                                                LookupOptions options,
                                                const LookupContext &context) const override;

      private:
        [[nodiscard]] const ImmutableArray<Ref<const NamespaceSymbol>> &get_using_namespaces() const;

        const CompilationUnitSyntax &syntax_;
        mutable Lazy<ImmutableArray<Ref<const NamespaceSymbol>>> using_namespaces_;
    };
} // namespace prism
