/**
 * @file conversion.ixx
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:semantic.conversion;

import std;
import :util.enum_class_flags;
import :util.optional;
import libassert;

namespace prism
{
    export class TypeSymbol;

    enum class ConversionKind : std::uint8_t
    {
        no_conversion,
        identity,
        implicit_numeric,
        implicit_character,

        explicit_numeric,
        explicit_character,
    };

    constexpr bool is_implicit_conversion(const ConversionKind kind) noexcept
    {
        return kind == ConversionKind::implicit_numeric || kind == ConversionKind::implicit_character ||
               kind == ConversionKind::identity;
    }

    constexpr bool is_trivial_conversion(const ConversionKind kind) noexcept
    {
        return kind == ConversionKind::identity || kind == ConversionKind::implicit_numeric ||
               kind == ConversionKind::explicit_numeric;
    }

    export class Conversion final
    {
      public:
        constexpr Conversion() noexcept = default;

      private:
        explicit Conversion(const ConversionKind kind) noexcept : kind_{kind}
        {
        }

      public:
        [[nodiscard]] constexpr bool exists() const noexcept
        {
            return kind_ != ConversionKind::no_conversion;
        }

        [[nodiscard]] constexpr bool is_identity() const noexcept
        {
            return kind_ == ConversionKind::identity;
        }

        [[nodiscard]] constexpr bool is_implicit() const noexcept
        {
            return is_implicit_conversion(kind_);
        }

        [[nodiscard]] constexpr bool is_explicit() const noexcept
        {
            return exists() && !is_implicit();
        }

        [[nodiscard]] constexpr bool is_numeric() const noexcept
        {
            return kind_ == ConversionKind::implicit_numeric || kind_ == ConversionKind::explicit_numeric;
        }

      private:
        friend constexpr Conversion get_trivial_conversion(ConversionKind kind);

        ConversionKind kind_ = ConversionKind::no_conversion;
    };

    [[nodiscard]] constexpr Conversion no_conversion;

    [[nodiscard]] constexpr Conversion get_trivial_conversion(const ConversionKind kind)
    {
        DEBUG_ASSERT(is_trivial_conversion(kind), "This conversion need additional data");
        return Conversion{kind};
    }
} // namespace prism
