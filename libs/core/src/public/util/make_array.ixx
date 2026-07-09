/**
 * @file make_array.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
export module prism.core.util.make_array;

import std;

namespace prism
{
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
