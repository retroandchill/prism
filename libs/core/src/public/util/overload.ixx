/**
 * @file overload.ixx
 * @author Francesco Corso
 * @date 7/21/2026
 * @brief
 */

export module prism.core:util.overload;

namespace prism
{
    export template <typename... Ts>
    struct Overload : Ts...
    {
        using Ts::operator()...;
    };

    export template <typename... Ts>
    Overload(Ts...) -> Overload<Ts...>;
} // namespace prism
