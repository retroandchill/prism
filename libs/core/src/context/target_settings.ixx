/**
 * @file target_settings.ixx
 * @author Francesco Corso
 * @date 8/9/2026
 * @brief
 */
export module prism.core:context.target_settings;

import std;

namespace prism
{
    constexpr std::uint16_t byte_size = 8;

    export enum class PointerWidth : std::uint16_t
    {
        x32 = 32,
        x64 = 64,
    };

    export [[nodiscard]] constexpr std::uint16_t bit_width(const PointerWidth width) noexcept
    {
        return std::to_underlying(width);
    }

    export [[nodiscard]] constexpr std::uint16_t byte_width(const PointerWidth width) noexcept
    {
        return bit_width(width) / 8;
    }

    [[nodiscard]] constexpr PointerWidth to_pointer_width(const std::uint64_t bits)
    {
        switch (bits)
        {
            case 32:
                return PointerWidth::x32;
            case 64:
                return PointerWidth::x64;
            default:
                throw std::invalid_argument{"Invalid pointer width"};
        }
    }

    export struct TargetSettings
    {
        PointerWidth pointer_width = PointerWidth::x64;

        [[nodiscard]] constexpr static TargetSettings current_platform() noexcept
        {
            static_assert(sizeof(void *) == 4 || sizeof(void *) == 8, "Unsupported host pointer width");

            return TargetSettings{
                .pointer_width = to_pointer_width(sizeof(void *) * byte_size),
            };
        }
    };
} // namespace prism
