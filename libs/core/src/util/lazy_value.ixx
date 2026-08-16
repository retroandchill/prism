/**
 * @file lazy_value.ixx
 * @author Francesco Corso
 * @date 8/15/2026
 * @brief
 */
module;

#include <functional>
#include <libassert/assert-macros.hpp>

export module prism.core:util.lazy_value;

import :util.noncopyable;
import :util.lazy;
import :memory.ref_counted_ptr;

namespace prism
{
    export template <typename T>
    class LazyValue final
    {
      public:
        using ValueType = T;
        using FactoryType = std::move_only_function<T()>;

        explicit constexpr LazyValue(FactoryType factory) noexcept
            : payload_{make_ref_counted<Payload>(std::move(factory))}
        {
        }

        [[nodiscard]] constexpr bool is_created() const noexcept
        {
            DEBUG_ASSERT(payload_ != nullptr);
            return payload_->is_created();
        }

        [[nodiscard]] constexpr const T &get() const noexcept
        {
            DEBUG_ASSERT(payload_ != nullptr);
            return payload_->get();
        }

        [[nodiscard]] constexpr bool valueless_after_move() const noexcept
        {
            return payload_ == nullptr;
        }

        [[nodiscard]] constexpr const T &operator*() const noexcept
        {
            return get();
        }

        [[nodiscard]] constexpr const T *operator->() const noexcept
        {
            return std::addressof(*get());
        }

        [[nodiscard]] constexpr friend std::size_t hash_value(const LazyValue &value) noexcept
        {
            return std::hash<RefCountPtr<Payload>>{}(value.payload_);
        }

        [[nodiscard]] constexpr friend bool operator==(const LazyValue &lhs, const LazyValue &rhs) noexcept
        {
            return lhs.payload_ == rhs.payload_;
        }

        [[nodiscard]] constexpr friend std::strong_ordering operator<=>(const LazyValue &lhs,
                                                                        const LazyValue &rhs) noexcept
        {
            return lhs.payload_ <=> rhs.payload_;
        }

      private:
        friend std::hash<LazyValue>;

        class Payload : public IntrusiveRefCounted
        {
          public:
            explicit constexpr Payload(FactoryType factory) noexcept : factory_{std::move(factory)}
            {
                DEBUG_ASSERT(factory_ != nullptr);
            }

            [[nodiscard]] constexpr bool is_created() const noexcept
            {
                return value_.has_value();
            }

            [[nodiscard]] constexpr const T &get() noexcept
            {
                return value_.get_or_compute([this] { return factory_(); });
            }

          private:
            FactoryType factory_;
            Lazy<T> value_{};
        };

        RefCountPtr<Payload> payload_{};
    };
} // namespace prism

template <typename T>
struct std::hash<prism::LazyValue<T>>
{
    constexpr size_t operator()(const prism::LazyValue<T> &value) const noexcept
    {
        return hash_value(value);
    }
};
