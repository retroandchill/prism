/**
 * @file enum_set.ixx
 * @author Francesco Corso
 * @date 7/28/2026
 * @brief
 */
export module prism.core:util.enum_set;

namespace prism
{
    export template <typename T, T... Values>
    struct EnumSet
    {
        [[nodiscard]] static constexpr bool contains(const T value) noexcept
        {
            return ((value == Values) || ...);
        }
    };
} // namespace prism
