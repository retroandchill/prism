/**
 * @file loop_binder.ixx
 * @author Francesco Corso
 * @date 8/27/2026
 * @brief
 */

export module prism.core:binder.loop_binder;

import :binder.local_scope_binder;

namespace prism
{
    class LoopBinder : public LocalScopeBinder
    {
      protected:
        explicit constexpr LoopBinder(const Binder &next) : LocalScopeBinder{next}
        {
        }

        ~LoopBinder() = default;
    };
} // namespace prism
