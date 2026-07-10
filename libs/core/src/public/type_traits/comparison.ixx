/**
 * @file comparison.ixx
 * @author Francesco Corso
 * @date 7/10/2026
 * @brief
 */
export module prism.core:type_traits.comparison;

import std;

namespace prism
{
    export template <typename T, typename U>
    concept EqualityComparableWith = requires(const T &t, const U &u) {
        {
            t == u
        } -> std::convertible_to<bool>;
    };

    export template <typename T, typename U>
    concept InequalityComparableWith = requires(const T &t, const U &u) {
        {
            t != u
        } -> std::convertible_to<bool>;
    };

    export template <typename T, typename U>
    concept LessThanComparableWith = requires(const T &t, const U &u) {
        {
            t < u
        } -> std::convertible_to<bool>;
    };

    export template <typename T, typename U>
    concept GreaterThanComparableWith = requires(const T &t, const U &u) {
        {
            t > u
        } -> std::convertible_to<bool>;
    };

    export template <typename T, typename U>
    concept LessThanOrEqualComparableWith = requires(const T &t, const U &u) {
        {
            t <= u
        } -> std::convertible_to<bool>;
    };

    export template <typename T, typename U>
    concept GreaterThanOrEqualComparableWith = requires(const T &t, const U &u) {
        {
            t >= u
        } -> std::convertible_to<bool>;
    };
} // namespace prism
