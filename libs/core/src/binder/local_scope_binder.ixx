/**
 * @file local_scope_binder.ixx
 * @author Francesco Corso
 * @date 8/17/2026
 * @brief
 */

export module prism.core:binder.local_scope_binder;

import :binder;
import :util.ref;
import :syntax.list;
import :memory.buffer_pool;

namespace prism
{
    class VariableDeclarationSyntax;
    class SourceVariableSymbol;
    class StatementSyntax;
    class VariableSymbol;

    class LocalScopeBinder : public Binder
    {
      protected:
        explicit LocalScopeBinder(const Binder &next);

        ~LocalScopeBinder() = default;

      public:
        [[nodiscard]] std::span<Ref<const VariableSymbol>> local_variables() const;

      protected:
        [[nodiscard]] virtual [[nodiscard]] std::span<Ref<const VariableSymbol>> build_local_variables_impl() const = 0;

        [[nodiscard]] std::span<Ref<const VariableSymbol>> build_local_variables(SyntaxList<StatementSyntax> statements,
                                                                                 const Binder &enclosing_binder) const;

        void build_local_variables(const Binder &enclosing_binder,
                                   const StatementSyntax &statement,
                                   PooledVector<Ref<const VariableSymbol>> &variables) const;

        const SourceVariableSymbol &make_local(const VariableDeclarationSyntax &declaration,
                                               const Binder *initializer_binder = nullptr) const;

        [[nodiscard]] LookupResult lookup_local(Name name,
                                                LookupOptions options,
                                                const LookupContext &context) const override;

      private:
        mutable Lazy<std::span<Ref<const VariableSymbol>>> local_variables_{};
    };
} // namespace prism
