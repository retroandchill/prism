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
import libassert;

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

        constexpr Lazy() noexcept
            requires std::is_trivially_default_constructible_v<T>
        = default;

        constexpr Lazy() noexcept
            requires(!std::is_trivially_default_constructible_v<T>)
            : null_{std::nullopt}
        {
        }

        template <typename U>
            requires std::constructible_from<T, U>
        constexpr explicit(std::convertible_to<U, T>) Lazy(U &&value) noexcept(std::is_nothrow_constructible_v<T, U>)
            : state_{LazyState::computed}, value_{std::forward<U>(value)}
        {
        }

        constexpr ~Lazy()
            requires std::is_trivially_destructible_v<T>
        = default;

        constexpr ~Lazy() noexcept
            requires(!std::is_trivially_destructible_v<T>)
        {
            reset();
        }

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

            return value_;
        }

        template <std::convertible_to<T> U>
        [[nodiscard]] constexpr T value_or(U &&default_value) const noexcept(std::is_nothrow_constructible_v<T, U>)
            requires std::is_copy_constructible_v<T>
        {
            auto current = state_.load(std::memory_order::acquire);

            while (current == LazyState::computing)
            {
                state_.wait(current, std::memory_order::acquire);
                current = state_.load(std::memory_order::acquire);
            }

            if (current == LazyState::uninitialized)
            {
                return std::forward<U>(default_value);
            }

            return value_;
        }

        [[nodiscard]] Optional<const T &> try_get_value() const noexcept
        {
            if (state_.load(std::memory_order::acquire) != LazyState::computed)
                return std::nullopt;

            return value_;
        }

        template <std::convertible_to<T> U>
        [[nodiscard]] constexpr T try_get_value(U &&default_value) const noexcept(std::is_nothrow_constructible_v<T, U>)
            requires std::is_copy_constructible_v<T>
        {
            if (state_.load(std::memory_order::acquire) != LazyState::computed)
            {
                return std::forward<U>(default_value);
            }

            return value_;
        }

        template <std::invocable Evaluator>
            requires std::convertible_to<std::invoke_result_t<Evaluator>, T>
        [[nodiscard]] ReferenceType get_or_compute(Evaluator &&evaluator)
        {
            auto current = state_.load(std::memory_order::acquire);

            while (true)
            {
                if (current == LazyState::computed)
                    return value_;

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
                        std::construct_at(std::addressof(value_), std::invoke(std::forward<Evaluator>(evaluator)));
                        state_.store(LazyState::computed, std::memory_order::release);
                        state_.notify_all();
                        return value_;
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
                return value_;

            state_ = LazyState::computing;
            try
            {
                std::construct_at(std::addressof(value_), std::invoke(std::forward<Evaluator>(evaluator)));
                state_ = LazyState::computed;
                return value_;
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

            if constexpr (!std::is_trivially_destructible_v<T>)
            {
                if (current == LazyState::computed)
                {
                    std::destroy_at(std::addressof(value_));
                }
            }
            state_.store(LazyState::uninitialized, std::memory_order::release);
            state_.notify_all();
        }

      private:
        std::atomic<LazyState> state_ = LazyState::uninitialized;
        union
        {
            T value_;
            std::nullopt_t null_{std::nullopt};
        };
    };

    template <typename T>
    class Lazy<T &> final : NonCopyable
    {
        static constexpr std::uintptr_t uninitialized = 0;
        static constexpr std::uintptr_t computing = 1;

      public:
        using ReferenceType = T &;

        constexpr Lazy() noexcept = default;

        explicit constexpr Lazy(T &value) noexcept : value_{reinterpret_cast<std::uintptr_t>(std::addressof(value))}
        {
        }

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

        [[nodiscard]] Optional<ReferenceType> try_get_value() const noexcept
        {
            const auto current = value_.load(std::memory_order::acquire);

            if (current == uninitialized || current == computing)
                return std::nullopt;

            return decode(current);
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

    template <typename T>
    class Lazy<T *> final : NonCopyable
    {
        static constexpr std::uintptr_t uninitialized = std::numeric_limits<std::uintptr_t>::max();
        static constexpr std::uintptr_t computing = uninitialized - 1;

      public:
        using ValueType = T *;
        using ReferenceType = T *;

        constexpr Lazy() noexcept = default;

        [[nodiscard]] constexpr LazyState state() const noexcept
        {
            const auto current = value_.load(std::memory_order::acquire);
            if (current == uninitialized)
                return LazyState::uninitialized;

            if (current == computing)
                return LazyState::computing;

            return LazyState::computed;
        }

        [[nodiscard]] constexpr bool has_value() const noexcept
        {
            return state() == LazyState::computed;
        }

        [[nodiscard]] constexpr ReferenceType value() const
        {
            auto current = value_.load(std::memory_order::acquire);

            while (current == computing)
            {
                value_.wait(current, std::memory_order::acquire);
                current = value_.load(std::memory_order::acquire);
            }

            if (current == uninitialized)
                throw InvalidStateException{"Lazy value has not been computed"};

            return decode(current);
        }

        [[nodiscard]] Optional<ValueType> wait_if_computing() const noexcept
        {
            auto current = value_.load(std::memory_order::acquire);

            while (current == computing)
            {
                value_.wait(current, std::memory_order::acquire);
                current = value_.load(std::memory_order::acquire);
            }

            if (current == uninitialized)
                return std::nullopt;

            return decode(current);
        }

        [[nodiscard]] Optional<ValueType> try_get_value() const noexcept
        {
            const auto current = value_.load(std::memory_order::acquire);

            if (current == uninitialized || current == computing)
                return std::nullopt;

            return decode(current);
        }

        [[nodiscard]] ValueType value_or(ValueType default_value) const noexcept
        {
            auto current = value_.load(std::memory_order::acquire);

            while (current == computing)
            {
                value_.wait(current, std::memory_order::acquire);
                current = value_.load(std::memory_order::acquire);
            }

            if (current == uninitialized)
                return default_value;

            return decode(current);
        }

        [[nodiscard]] ValueType try_get_value(ValueType default_value) const noexcept
        {
            const auto current = value_.load(std::memory_order::acquire);

            if (current == uninitialized || current == computing)
                return default_value;

            return decode(current);
        }

        template <std::invocable Evaluator>
            requires std::convertible_to<std::invoke_result_t<Evaluator>, ValueType>
        [[nodiscard]] ReferenceType get_or_compute(Evaluator &&evaluator)
        {
            auto current = value_.load(std::memory_order::acquire);

            while (true)
            {
                if (current != uninitialized && current != computing)
                    return decode(current);

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
                        auto *result = static_cast<ValueType>(std::invoke(std::forward<Evaluator>(evaluator)));
                        const auto published = encode(result);

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

        void set(ValueType value) noexcept
        {
            value_.store(encode(value), std::memory_order::release);
            value_.notify_all();
        }

      private:
        [[nodiscard]] static constexpr ValueType decode(const std::uintptr_t value) noexcept
        {
            DEBUG_ASSERT(value != uninitialized);
            DEBUG_ASSERT(value != computing);
            return reinterpret_cast<ValueType>(value);
        }

        [[nodiscard]] static constexpr std::uintptr_t encode(ValueType value) noexcept
        {
            const auto result = reinterpret_cast<std::uintptr_t>(value);

            DEBUG_ASSERT(result != uninitialized);
            DEBUG_ASSERT(result != computing);

            return result;
        }

        std::atomic<std::uintptr_t> value_ = uninitialized;
    };

    export template <typename T>
    Lazy(T &&) -> Lazy<std::decay_t<T>>;
} // namespace prism
