/**
 * @file optional_ref.ixx
 * @author Francesco Corso
 * @date 7/10/2026
 * @brief
 */
export module prism.core:util.optional_ref;

import std;

namespace prism
{
    export template <typename T>
    constexpr std::optional<T &> make_optional_ref(T *ptr) noexcept
    {
        if (ptr == nullptr)
            return std::nullopt;

        return *ptr;
    }
} // namespace prism
