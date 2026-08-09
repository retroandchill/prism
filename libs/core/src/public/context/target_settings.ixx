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
    constexpr std::uint16_t byte_width = 8;

    export struct TargetSettings
    {
        std::uint16_t pointer_width = 64;

        [[nodiscard]] constexpr static TargetSettings current_platform() noexcept
        {
            return TargetSettings{.pointer_width = sizeof(void *) * byte_width};
        }
    };
} // namespace prism
