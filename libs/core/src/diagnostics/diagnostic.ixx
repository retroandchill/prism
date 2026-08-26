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
        constexpr Diagnostic(RefCountPtr<const DiagnosticInfo> info, Location location)
            : info_{std::move(info)}, location_{std::move(location)}
        {
            DEBUG_ASSERT(info_ != nullptr);
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
        constexpr Diagnostic(RefCountPtr<const DiagnosticInfo> info, Location location, Range &&additional_locations)
            : info_{std::move(info)}, location_{std::move(location)},
              additional_locations_{std::from_range, std::forward<Range>(additional_locations)}
        {
            DEBUG_ASSERT(info_ != nullptr);
        }

      public:
        template <std::formattable<char>... Args>
        constexpr Diagnostic(const DiagnosticDescriptor &descriptor, Location location, Args &&...args)
            : Diagnostic{prism::make_ref_counted<const DiagnosticInfo>(descriptor, std::forward<Args>(args)...),
                         std::move(location)}
        {
        }

        template <std::formattable<char>... Args>
        constexpr Diagnostic(const DiagnosticDescriptor &descriptor,
                             DiagnosticSeverity severity,
                             Location location,
                             Args &&...args)
            : Diagnostic{
                  prism::make_ref_counted<const DiagnosticInfo>(descriptor, severity, std::forward<Args>(args)...),
                  std::move(location)}
        {
        }

        template <std::ranges::input_range Range, std::formattable<char>... Args>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
        constexpr Diagnostic(const DiagnosticDescriptor &descriptor, Location location, Range &&range, Args &&...args)
            : Diagnostic{prism::make_ref_counted<const DiagnosticInfo>(descriptor, std::forward<Args>(args)...),
                         std::move(location),
                         std::forward<Range>(range)}
        {
        }

        template <std::ranges::input_range Range, std::formattable<char>... Args>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
        constexpr Diagnostic(const DiagnosticDescriptor &descriptor,
                             DiagnosticSeverity severity,
                             Location location,
                             Range &&range,
                             Args &&...args)
            : Diagnostic{
                  prism::make_ref_counted<const DiagnosticInfo>(descriptor, severity, std::forward<Args>(args)...),
                  std::move(location),
                  std::forward<Range>(range)}
        {
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

        [[nodiscard]] constexpr bool is_enabled_by_default() const
        {
            return info_->is_enabled_by_default();
        }

        [[nodiscard]] bool is_warning_as_error() const;

        [[nodiscard]] constexpr const Location &location() const
        {
            return location_;
        }

        [[nodiscard]] constexpr const ImmutableArray<Location> &additional_locations() const
        {
            return additional_locations_;
        }

        [[nodiscard]] constexpr DiagnosticCustomTags custom_tags() const
        {
            return info_->custom_tags();
        }

      private:
        friend struct DiagnosticInternal;

        RefCountPtr<const DiagnosticInfo> info_{};
        Location location_;
        ImmutableArray<Location> additional_locations_{};
    };

    struct DiagnosticInternal
    {
        [[nodiscard]] constexpr static Diagnostic create(RefCountPtr<const DiagnosticInfo> info,
                                                         Location location) noexcept
        {
            return Diagnostic{std::move(info), std::move(location)};
        }
    };
} // namespace prism
