/**
 * @file lazy.ixx
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
export module prism.core:util.lazy;

import std;
import :util.exceptions;
import :util.noncopyable;
import :util.optional;

namespace prism
{
    export enum class LazyState : std::uint8_t
    {
        uninitialized,
        computing,
        computed
    };

    export template <typename T>
    class Lazy final : NonCopyable
    {
      public:
        using ReferenceType = std::conditional_t<std::is_reference_v<T>, T, const T &>;

        constexpr Lazy() noexcept = default;

        [[nodiscard]] constexpr LazyState state() const noexcept
        {
            return state_;
        }

        [[nodiscard]] constexpr bool has_value() const noexcept
        {
            return state_ == LazyState::computed;
        }

        [[nodiscard]] constexpr ReferenceType value() const
        {
            auto current = state_.load(std::memory_order::acquire);

            while (current == LazyState::computing)
            {
                state_.wait(current, std::memory_order::acquire);
                current = state_.load(std::memory_order::acquire);
            }

            if (current == LazyState::uninitialized)
            {
                throw InvalidStateException{"Lazy value has not been computed"};
            }

            return *value_;
        }

        template <std::invocable Evaluator>
            requires std::convertible_to<std::invoke_result_t<Evaluator>, T>
        [[nodiscard]] ReferenceType get_or_compute(Evaluator &&evaluator)
        {
            auto current = state_.load(std::memory_order::acquire);

            while (true)
            {
                if (current == LazyState::computed)
                    return *value_;

                if (current == LazyState::computing)
                {
                    state_.wait(current, std::memory_order::acquire);
                    current = state_.load(std::memory_order::acquire);
                    continue;
                }

                auto expected = LazyState::uninitialized;
                if (state_.compare_exchange_strong(expected,
                                                   LazyState::computing,
                                                   std::memory_order::acq_rel,
                                                   std::memory_order::acquire))
                {
                    try
                    {
                        value_.emplace(std::invoke(std::forward<Evaluator>(evaluator)));
                        state_.store(LazyState::computed, std::memory_order::release);
                        state_.notify_all();
                        return *value_;
                    }
                    catch (...)
                    {
                        state_.store(LazyState::uninitialized, std::memory_order::release);
                        state_.notify_all();
                        throw;
                    }
                }

                current = expected;
            }

            if (state_ == LazyState::computed)
                return *value_;

            state_ = LazyState::computing;
            try
            {
                value_.emplace(std::invoke(std::forward<Evaluator>(evaluator)));
                state_ = LazyState::computed;
                return *value_;
            }
            catch (...)
            {
                state_ = LazyState::uninitialized;
                throw;
            }
        }

        void reset() noexcept
        {
            auto current = state_.load(std::memory_order::acquire);

            while (current == LazyState::computing)
            {
                state_.wait(current, std::memory_order::acquire);
                current = state_.load(std::memory_order::acquire);
            }

            value_.reset();
            state_.store(LazyState::uninitialized, std::memory_order::release);
            state_.notify_all();
        }

      private:
        std::atomic<LazyState> state_ = LazyState::uninitialized;
        Optional<T> value_;
    };
} // namespace prism
