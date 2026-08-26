/**
 * @file target_settings.ixx
 * @author Francesco Corso
 * @date 8/9/2026
 * @brief
 */
export module prism.core:context.compilation_settings;

import std;
import :util.exceptions;

namespace prism
{
    constexpr std::uint16_t byte_size = 8;

    export enum class Architecture : std::uint8_t
    {
        x86,
        arm,
        riscv,
        wasm,
        unknown
    };

    [[nodiscard]] consteval Architecture current_platform_architecture() noexcept
    {
#if defined(__x86_64__) || defined(_M_X64) || defined(__i386__) || defined(_M_IX86)
        return Architecture::x86;
#elif defined(__aarch64__) || defined(_M_ARM64) || defined(__arm__) || defined(_M_ARM)
        return = Architecture::arm;
#elif defined(__riscv)
        return = Architecture::riscv;
#elif defined(__wasm32__) || defined(__wasm64__)
        return = Architecture::wasm;
#else
        return Architecture::unknown;
#endif
    }

    export enum class OperatingSystem : std::uint8_t
    {
        linux,
        windows,
        macos,
        freestanding
    };

    [[nodiscard]] consteval OperatingSystem current_operating_system() noexcept
    {
#if defined(_WIN32)
        return OperatingSystem::windows;
#elif defined(__linux__)
        return OperatingSystem::linux;
#elif defined(__APPLE__) && defined(__MACH__)
        return OperatingSystem::macos;
#else
        return OperatingSystem::freestanding;
#endif
    }

    export enum class Environment : std::uint8_t
    {
        gnu,
        musl,
        msvc,
        none
    };

    [[nodiscard]] consteval Environment current_platform_environment() noexcept
    {
#if defined(_MSC_VER)
        return Environment::msvc;
#elif defined(__GLIBC__)
        return Environment::gnu;
#elif defined(__linux__) && !defined(__GLIBC__)
        // Fallback heuristic for musl on Linux environments
        return Environment::musl;
#else
        return Environment::none;
#endif
    }

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

    [[nodiscard]] consteval PointerWidth current_platform_pointer_width() noexcept
    {
        static_assert(sizeof(void *) == 4 || sizeof(void *) == 8, "Unsupported host pointer width");
        return to_pointer_width(sizeof(void *) * byte_size);
    }

    export enum class OutputKind : std::uint8_t
    {
        executable,
        static_library,
        shared_library
    };

    export struct CompilationSettings
    {
        Architecture architecture = current_platform_architecture();
        OperatingSystem operating_system = current_operating_system();
        Environment environment = current_platform_environment();
        PointerWidth pointer_width = current_platform_pointer_width();
        OutputKind kind = OutputKind::executable;

        [[nodiscard]] constexpr bool is_application() const noexcept
        {
            return kind == OutputKind::executable;
        }

        [[nodiscard]] constexpr bool is_library() const noexcept
        {
            return kind == OutputKind::static_library || kind == OutputKind::shared_library;
        }

        [[nodiscard]] constexpr bool is_shared_library() const noexcept
        {
            return kind == OutputKind::shared_library;
        }
    };
} // namespace prism
