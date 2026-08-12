/**
 * @file lazy.ixx
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

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
        using ReferenceType = const T &;

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
        Optional<T> value_ = std::nullopt;
    };

    template <typename T>
    class Lazy<T &> final : NonCopyable
    {
        static constexpr std::uintptr_t uninitialized = 0;
        static constexpr std::uintptr_t computing = 1;

      public:
        using ReferenceType = T &;

        constexpr Lazy() noexcept = default;

        [[nodiscard]] constexpr LazyState state() const noexcept
        {
            const auto current = value_.load(std::memory_order_acquire);
            if (current == uninitialized)
                return LazyState::uninitialized;

            if (current == computing)
                return LazyState::computing;

            return LazyState::computed;
        }

        [[nodiscard]] constexpr bool has_value() const noexcept
        {
            return decode(value_.load(std::memory_order_acquire)) != nullptr;
        }

        [[nodiscard]] constexpr ReferenceType value() const
        {
            auto current = value_.load(std::memory_order::acquire);

            while (current == computing)
            {
                value_.wait(current, std::memory_order::acquire);
                current = value_.load(std::memory_order::acquire);
            }

            auto *result = decode(current);
            if (result == nullptr)
                throw InvalidStateException{"Lazy value has not been computed"};

            return *result;
        }

        [[nodiscard]] Optional<T &> wait_if_computing() const noexcept
        {
            auto current = value_.load(std::memory_order::acquire);

            while (current == computing)
            {
                value_.wait(current, std::memory_order::acquire);
                current = value_.load(std::memory_order::acquire);
            }

            return decode(current);
        }

        template <std::invocable Evaluator>
            requires std::convertible_to<std::invoke_result_t<Evaluator>, T &>
        [[nodiscard]] ReferenceType get_or_compute(Evaluator &&evaluator)
        {
            auto current = value_.load(std::memory_order::acquire);

            while (true)
            {
                if (auto *result = decode(current); result != nullptr)
                    return *result;

                if (current == computing)
                {
                    value_.wait(current, std::memory_order::acquire);
                    current = value_.load(std::memory_order::acquire);
                    continue;
                }

                auto expected = uninitialized;
                if (value_.compare_exchange_strong(expected,
                                                   computing,
                                                   std::memory_order::acq_rel,
                                                   std::memory_order::acquire))
                {
                    try
                    {
                        auto &result = std::invoke(std::forward<Evaluator>(evaluator));
                        const auto published = encode(std::addressof(result));

                        value_.store(published, std::memory_order::release);
                        value_.notify_all();

                        return result;
                    }
                    catch (...)
                    {
                        value_.store(uninitialized, std::memory_order::release);
                        value_.notify_all();
                        throw;
                    }
                }

                current = expected;
            }
        }

        void reset() noexcept
        {
            auto current = value_.load(std::memory_order::acquire);

            while (current == computing)
            {
                value_.wait(current, std::memory_order::acquire);
                current = value_.load(std::memory_order::acquire);
            }

            value_.store(uninitialized, std::memory_order::release);
            value_.notify_all();
        }

        void set(ReferenceType value) noexcept
        {
            const auto published = encode(std::addressof(value));
            value_.store(published, std::memory_order::release);
            value_.notify_all();
        }

      private:
        [[nodiscard]] static constexpr T *decode(const std::uintptr_t value) noexcept
        {
            return value > computing ? reinterpret_cast<T *>(value) : nullptr;
        }

        [[nodiscard]] static constexpr std::uintptr_t encode(T *value) noexcept
        {
            const auto result = reinterpret_cast<std::uintptr_t>(value);
            DEBUG_ASSERT(result > computing);
            return result;
        }

        std::atomic<std::uintptr_t> value_ = 0;
    };
} // namespace prism
