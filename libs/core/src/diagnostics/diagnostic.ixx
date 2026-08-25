/**
 * @file diagnostic.ixx
 * @author Francesco Corso
 * @date 8/3/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

#include <libassert/assert-macros.hpp>

export module prism.core:diagnostics.diagnostic;

import std;
import :text.string_writer;
import :diagnostics.descriptor;
import :diagnostics.location;
import :diagnostics.info;

namespace prism
{
    constexpr inline std::string_view compiler_diagnostic_category = "Compiler";

    export class PRISM_CORE_API Diagnostic final
    {
      public:
        constexpr Diagnostic(std::shared_ptr<const DiagnosticInfo> info,
                             Location location,
                             const bool is_suppressed = false)
            : info_{std::move(info)}, location_{std::move(location)}, is_suppressed_{is_suppressed}
        {
            DEBUG_ASSERT(info_ != nullptr);
        }

        [[nodiscard]] constexpr const DiagnosticDescriptor &descriptor() const
        {
            return info_->descriptor();
        }

        [[nodiscard]] constexpr std::string_view id() const
        {
            return info_->id();
        }

        [[nodiscard]] constexpr std::string_view category() const
        {
            return info_->category();
        }

        constexpr void write_message(TextWriter &writer) const
        {
            return info_->write_message(writer);
        }

        template <typename Self>
        [[nodiscard]] constexpr std::string get_message(this const Self &self)
        {
            std::string result;
            auto writer = StringWriter{result};
            self.write_message(writer);
            return result;
        }

        [[nodiscard]] constexpr DiagnosticSeverity default_severity() const
        {
            return info_->default_severity();
        }

        [[nodiscard]] constexpr DiagnosticSeverity severity() const
        {
            return info_->severity();
        }

        [[nodiscard]] constexpr bool is_suppressed() const
        {
            return is_suppressed_;
        }

        [[nodiscard]] constexpr bool is_enabled_by_default() const
        {
            return info_->is_enabled_by_default();
        }

        [[nodiscard]] bool is_warning_as_error() const;

        [[nodiscard]] constexpr const Location &location() const
        {
            return location_;
        }

        [[nodiscard]] constexpr std::span<const Location> additional_locations() const
        {
            return info_->additional_locations();
        }

        [[nodiscard]] constexpr DiagnosticCustomTags custom_tags() const
        {
            return info_->custom_tags();
        }

      private:
        std::shared_ptr<const DiagnosticInfo> info_{};
        Location location_;
        bool is_suppressed_;
    };
} // namespace prism
