/**
 * @file ref.ixx
 * @author Francesco Corso
 * @date 7/30/2026
 * @brief
 */

export module prism.core:util.ref;

import std;
import :type_traits.basic;

namespace prism
{
    export template <typename T>
    class Ref
    {
      public:
        using element_type = T;

        template <typename U>
            requires(std::constructible_from<T &, U> && !ReferenceConvertsFromTemporary<T &, U> &&
                     !std::same_as<std::remove_cvref_t<U>, Ref>)
        constexpr explicit(false) Ref(U &&value) : ptr_{&Ref::get_value(std::forward<U>(value))}
        {
        }

        constexpr Ref(const Ref &) = default;
        constexpr Ref(Ref &&) noexcept = default;

        ~Ref() = default;

        constexpr Ref &operator=(const Ref &) = default;
        constexpr Ref &operator=(Ref &&) noexcept = default;

        [[nodiscard]] constexpr T &get() const noexcept
        {
            return *ptr_;
        }

        [[nodiscard]] constexpr explicit(false) operator T &() const noexcept
        {
            return *ptr_;
        }

        [[nodiscard]] constexpr T *operator->() const noexcept
        {
            return ptr_;
        }

        [[nodiscard]] constexpr std::strong_ordering operator<=>(const Ref &other) const noexcept = default;

      private:
        template <typename U>
        [[nodiscard]] constexpr static T &get_value(U &&value)
        {
            T &ref = std::forward<U>(value);
            return ref;
        }

        T *ptr_;
    };

    export template <typename T>
    Ref(T &) -> Ref<T>;
} // namespace prism
