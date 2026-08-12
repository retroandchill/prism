/**
 * @file alignment.ixx
 * @author Francesco Corso
 * @date 6/30/2026
 * @brief
 */
export module prism.core:memory.alignment;

import std;

namespace prism
{
    export template <typename T>
        requires std::integral<T> || std::is_pointer_v<T>
    constexpr T align(T value, const std::size_t alignment)
    {
        return static_cast<T>(static_cast<std::size_t>(value) + alignment - 1 & ~(alignment - 1));
    }
} // namespace prism
