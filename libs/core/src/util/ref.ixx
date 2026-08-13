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
    template <typename T, typename U>
    concept HasIsFunction = requires(T t) {
        {
            t.template is<U>()
        } -> std::convertible_to<bool>;
    };

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

        template <std::derived_from<std::remove_cv_t<T>> Other>
            requires(HasIsFunction<std::remove_cv_t<T>, Other> || std::is_polymorphic_v<std::remove_cv_t<T>>)
        [[nodiscard]] constexpr bool is() const noexcept
        {
            if constexpr (std::derived_from<std::remove_cv_t<T>, Other>)
            {
                // If Other matched exactly or is a superclass of T, then it's always the case that it's the same time
                return true;
            }
            else if constexpr (HasIsFunction<std::remove_cv_t<T>, Other>)
            {
                // If we have a user defined is<T> function, then prefer that
                return ptr_->template is<Other>();
            }
            else
            {
                // Only fall-back to using dynamic_cast when there is no other way to verify the type
                return dynamic_cast<const Other *>(ptr_) != nullptr;
            }
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

    template <typename>
    struct IsRefInstance : std::false_type
    {
    };

    template <typename T>
    struct IsRefInstance<Ref<T>> : std::true_type
    {
    };

    export template <typename T>
    concept RefInstance = IsRefInstance<std::decay_t<T>>::value;
} // namespace prism
