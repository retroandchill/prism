/**
 * @file util.ixx
 * @author Francesco Corso
 * @date 6/26/2026
 * @brief
 */
export module prism.core.util;

import std;

namespace prism
{
    export class NonCopyable
    {
      protected:
        NonCopyable() = default;
        ~NonCopyable() = default;

      public:
        NonCopyable(const NonCopyable &) = delete;
        NonCopyable(NonCopyable &&) noexcept = delete;
        NonCopyable &operator=(const NonCopyable &) = delete;
        NonCopyable &operator=(NonCopyable &&) noexcept = delete;
    };

    export template <typename T>
    class Ref : public std::reference_wrapper<T>
    {
      public:
        using std::reference_wrapper<T>::reference_wrapper;
        using std::reference_wrapper<T>::operator=;

        constexpr T *operator->() const noexcept
        {
            return std::addressof(std::reference_wrapper<T>::get());
        }
    };

    export template <typename T>
    concept LValueReference = std::is_lvalue_reference_v<T>;

    export template <typename T>
    concept Nullable = requires(const T &value) {
        {
            value == nullptr
        } -> std::convertible_to<bool>;
        {
            *value
        } -> LValueReference;
    };

    export template <Nullable T>
    using ReferenceType = decltype(*std::declval<T>());

    export template <Nullable T1, Nullable T2 = T1>
        requires std::equality_comparable_with<ReferenceType<T1>, ReferenceType<T2>>
    constexpr bool values_equal(const T1 &lhs, const T2 &rhs) noexcept
    {
        if (lhs == nullptr)
        {
            return rhs == nullptr;
        }

        return rhs != nullptr && *lhs == *rhs;
    }

    export template <typename T>
        requires std::is_scoped_enum_v<T>
    constexpr bool is_flag_enum = false;

    export template <typename T>
        requires std::is_scoped_enum_v<T> && is_flag_enum<T>
    constexpr T operator|(T lhs, T rhs)
    {
        return static_cast<T>(static_cast<std::underlying_type_t<T>>(lhs) |
                              static_cast<std::underlying_type_t<T>>(rhs));
    }

    export template <typename T>
        requires std::is_scoped_enum_v<T> && is_flag_enum<T>
    constexpr T &operator|=(T &lhs, T rhs)
    {
        lhs = lhs | rhs;
        return lhs;
    }

    export template <typename T>
        requires std::is_scoped_enum_v<T> && is_flag_enum<T>
    constexpr T operator&(T lhs, T rhs)
    {
        return static_cast<T>(static_cast<std::underlying_type_t<T>>(lhs) &
                              static_cast<std::underlying_type_t<T>>(rhs));
    }

    export template <typename T>
        requires std::is_scoped_enum_v<T> && is_flag_enum<T>
    constexpr T &operator&=(T &lhs, T rhs)
    {
        lhs = lhs & rhs;
        return lhs;
    }

    export template <typename T>
        requires std::is_scoped_enum_v<T> && is_flag_enum<T>
    constexpr T operator^(T lhs, T rhs)
    {
        return static_cast<T>(static_cast<std::underlying_type_t<T>>(lhs) ^
                              static_cast<std::underlying_type_t<T>>(rhs));
    }

    export template <typename T>
        requires std::is_scoped_enum_v<T> && is_flag_enum<T>
    constexpr T &operator^=(T &lhs, T rhs)
    {
        lhs = lhs ^ rhs;
        return lhs;
    }

    export template <typename T>
        requires std::is_scoped_enum_v<T> && is_flag_enum<T>
    constexpr T operator~(T value)
    {
        return static_cast<T>(~static_cast<std::underlying_type_t<T>>(value));
    }

    export template <typename T>
    concept FlagEnum = std::is_scoped_enum_v<T> && requires(T &lhs, T rhs) {
        {
            lhs | rhs
        } -> std::convertible_to<T>;
        {
            lhs |= rhs
        } -> std::same_as<T &>;
        {
            lhs &rhs
        } -> std::convertible_to<T>;
        {
            lhs &= rhs
        } -> std::same_as<T &>;
        {
            lhs ^ rhs
        } -> std::convertible_to<T>;
        {
            lhs ^= rhs
        } -> std::same_as<T &>;
        {
            ~lhs
        } -> std::convertible_to<T>;
    };

    export template <FlagEnum T>
    constexpr bool has_all_flags(T enum_value, T flags)
    {
        return (enum_value & flags) == flags;
    }

    export template <FlagEnum T>
    constexpr bool has_any_flags(T enum_value, T flags)
    {
        return (enum_value & flags) != static_cast<T>(0);
    }

    export template <typename... Functors>
    struct Overload : Functors...
    {
        using Functors::operator()...;
    };

    export template <typename... Functors>
    Overload(Functors...) -> Overload<Functors...>;

    export template <typename>
    struct IsVariant : std::false_type
    {
    };

    template <typename... Ts>
    struct IsVariant<std::variant<Ts...>> : std::true_type
    {
    };

    export template <typename T>
    concept VariantLike = IsVariant<std::remove_cvref_t<T>>::value;

    export template <typename Variant, typename Alternative>
    struct IsVariantAlternative : std::false_type
    {
    };

    template <typename Alternative, typename... Alternatives>
    struct IsVariantAlternative<std::variant<Alternatives...>, Alternative>
        : std::bool_constant<(std::same_as<Alternative, Alternatives> || ...)>
    {
    };

    export template <typename Alternative, typename Variant>
    concept VariantAlternative =
        VariantLike<Variant> && IsVariantAlternative<std::remove_cvref_t<Variant>, Alternative>::value;

    export template <VariantLike VariantType, std::size_t Index>
    using VariantAlternativeBase = std::variant_alternative_t<Index, std::remove_cvref_t<VariantType>>;

    export template <VariantLike VariantType, std::size_t Index>
    using VariantAlternativeReference =
        std::conditional_t<std::is_lvalue_reference_v<VariantType>,
                           std::conditional_t<std::is_const_v<std::remove_reference_t<VariantType>>,
                                              const VariantAlternativeBase<VariantType, Index> &,
                                              VariantAlternativeBase<VariantType, Index> &>,
                           std::conditional_t<std::is_const_v<std::remove_reference_t<VariantType>>,
                                              const VariantAlternativeBase<VariantType, Index> &&,
                                              VariantAlternativeBase<VariantType, Index> &&>>;

    template <typename Visitor, typename VariantType, typename Indicies>
    struct IsVariantVisitorForImpl;

    template <typename Visitor, VariantLike VariantType, std::size_t... Indicies>
    struct IsVariantVisitorForImpl<Visitor, VariantType, std::index_sequence<Indicies...>>
        : std::bool_constant<(std::invocable<Visitor, VariantAlternativeReference<VariantType, Indicies>> && ...)>
    {
    };

    template <typename Visitor, typename VariantType, typename Indicies>
    struct IsVariantVisitorReturnSameImpl;

    template <typename Visitor, VariantLike VariantType, std::size_t First, std::size_t... Indicies>
    struct IsVariantVisitorReturnSameImpl<Visitor, VariantType, std::index_sequence<First, Indicies...>>
        : std::bool_constant<(
              std::same_as<std::invoke_result_t<Visitor, VariantAlternativeReference<VariantType, First>>,
                           VariantAlternativeReference<VariantType, Indicies>> &&
              ...)>
    {
    };

    template <typename R, typename Visitor, typename VariantType, typename Indicies>
    struct IsVariantVisitorReturnConvertableToImpl;

    template <typename R, typename Visitor, VariantLike VariantType, std::size_t... Indicies>
    struct IsVariantVisitorReturnConvertableToImpl<R, Visitor, VariantType, std::index_sequence<Indicies...>>
        : std::bool_constant<(std::convertible_to<VariantAlternativeReference<VariantType, Indicies>, R> && ...)>
    {
    };

    export template <typename Visitor, typename VariantType>
    concept VariantVisitorFor =
        VariantLike<VariantType> &&
        IsVariantVisitorForImpl<
            Visitor,
            VariantType,
            std::make_index_sequence<std::variant_size_v<std::remove_cvref_t<VariantType>>>>::value &&
        IsVariantVisitorReturnSameImpl<
            Visitor,
            VariantType,
            std::make_index_sequence<std::variant_size_v<std::remove_cvref_t<VariantType>>>>::value;

    export template <typename Visitor, typename R, typename VariantType>
    concept VariantVisitorReturnSame =
        VariantLike<VariantType> &&
        IsVariantVisitorForImpl<
            Visitor,
            VariantType,
            std::make_index_sequence<std::variant_size_v<std::remove_cvref_t<VariantType>>>>::value &&
        IsVariantVisitorReturnConvertableToImpl<
            R,
            Visitor,
            VariantType,
            std::make_index_sequence<std::variant_size_v<std::remove_cvref_t<VariantType>>>>::value;

    template <bool IsConst, class T>
    using MaybeConst = std::conditional_t<IsConst, const T, T>;

    template <class T,
              class U,
              class Tmp = MaybeConst<std::is_const_v<std::remove_reference_t<T>>, std::remove_reference_t<U>>>
    using ForwardLikeTypeHelper = std::conditional_t<std::is_rvalue_reference_v<T &&>, Tmp &&, Tmp &>;

    export template <typename T, typename U>
    using ForwardLikeType = ForwardLikeTypeHelper<T, U>;

    template <typename T, std::size_t... Is, std::invocable<std::size_t> Factory>
        requires std::convertible_to<std::invoke_result_t<Factory, std::size_t>, T> ||
                 std::same_as<std::invoke_result_t<Factory, std::size_t>, T>
    constexpr std::array<T, sizeof...(Is)> make_array_impl(std::index_sequence<Is...>, Factory &&factory)
    {
        return std::array<T, sizeof...(Is)>{std::invoke(factory, Is)...};
    }

    export template <typename T, std::size_t N, std::invocable<std::size_t> Factory>
        requires std::convertible_to<std::invoke_result_t<Factory, std::size_t>, T> ||
                 std::same_as<std::invoke_result_t<Factory, std::size_t>, T>
    constexpr std::array<T, N> make_array(Factory &&factory)
    {
        return make_array_impl<T>(std::make_index_sequence<N>{}, std::forward<Factory>(factory));
    }
} // namespace prism
