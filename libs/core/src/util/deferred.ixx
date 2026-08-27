/**
 * @file deferred.ixx
 * @author Francesco Corso
 * @date 8/27/2026
 * @brief
 */

export module prism.core:util.deferred;

import std;
import :util.noncopyable;

namespace prism
{
    export template <std::invocable Functor>
        requires std::is_nothrow_invocable_v<Functor>
    class Deferred final : NonCopyable
    {
      public:
        explicit constexpr Deferred(Functor functor) noexcept : functor_{std::move(functor)}
        {
        }

        constexpr ~Deferred() noexcept
        {
            std::invoke(functor_);
        }

      private:
        Functor functor_;
    };

    export template <std::invocable Functor>
        requires std::is_nothrow_invocable_v<Functor>
    Deferred(Functor) -> Deferred<Functor>;
} // namespace prism
