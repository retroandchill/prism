/**
 * @file default_local_scope_binder.ixx
 * @author Francesco Corso
 * @date 8/17/2026
 * @brief
 */

export module prism.core:binder.default_local_scope_binder;

import :binder.local_scope_binder;

namespace prism
{
    class DefaultLocalScopeBinder final : public LocalScopeBinder
    {
      public:
        explicit DefaultLocalScopeBinder(const Binder &next) : LocalScopeBinder{next}
        {
        }

      protected:
        [[nodiscard]] std::span<Ref<const VariableSymbol>> build_local_variables_impl() const override
        {
            return {};
        }
    };
} // namespace prism
