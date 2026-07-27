/**
 * @file visitor.ixx
 * @author Francesco Corso
 * @date 7/26/2026
 * @brief
 */

export module prism.core:type_traits.visitor;

import std;

namespace prism
{
    template <typename Functor, typename... Options>
    concept InvocableOnEach = (std::invocable<Functor, Options> && ...);

    template <typename Functor, typename First, typename... Rest>
    concept ReturnsSameOnEach =
        (std::same_as<std::invoke_result_t<Functor, First>, std::invoke_result_t<Functor, Rest>> && ...);

    template <typename Functor, typename R, typename... Options>
    concept ResultsAllConvertableTo = (std::convertible_to<std::invoke_result_t<Functor, Options>, R> && ...);

    export template <typename Functor, typename... Options>
    concept ExhaustiveVisitor = InvocableOnEach<Functor, Options...> && ReturnsSameOnEach<Functor, Options...>;

    export template <typename Functor, typename R, typename... Options>
    concept ConvertibleVisitor =
        InvocableOnEach<Functor, Options...> && ResultsAllConvertableTo<Functor, R, Options...>;
} // namespace prism
