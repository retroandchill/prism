/**
 * @file identifier.ixx
 * @author Francesco Corso
 * @date 6/28/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

export module prism.core.strings.identifier;

import std;

namespace prism
{
    export enum class FindIdentifier : std::uint8_t
    {
        find,
        add
    };

    export constexpr inline std::size_t max_identifier_length = 1024;

    export class PRISM_CORE_API Identifier
    {
      public:
        constexpr Identifier() = default;

        inline explicit(false)
            Identifier(const std::string_view name, const FindIdentifier find_identifier = FindIdentifier::add)
            : id_{lookup_name(name, find_identifier)}
        {
        }

        inline explicit(false) Identifier(const char *name, const FindIdentifier find_identifier = FindIdentifier::add)
            : id_{lookup_name(name, find_identifier)}
        {
        }

        [[nodiscard]] constexpr std::uint32_t id() const noexcept
        {
            return id_;
        }

        [[nodiscard]] constexpr bool is_none() const noexcept
        {
            return id_ == 0;
        }

        [[nodiscard]] constexpr static Identifier none() noexcept
        {
            return {};
        }

      private:
        static std::uint32_t lookup_name(std::string_view name, FindIdentifier find_identifier);

        std::uint32_t id_ = 0;
    };
} // namespace prism
