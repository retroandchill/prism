/**
 * @file conversion_classifier.ixx
 * @author Francesco Corso
 * @date 8/9/2026
 * @brief
 */
export module prism.core:semantic.conversion_classifier;

import :semantic.conversion;
import :symbols.type_symbol;
import :context.target_settings;

namespace prism
{
    class Binder;

    class ConversionClassifier final
    {
      public:
        explicit constexpr ConversionClassifier(const Binder &binder) : binder_{binder}
        {
        }

        [[nodiscard]] Conversion classify_conversion(const TypeSymbol &source, const TypeSymbol &destination) const;

      private:
        [[nodiscard]] const Compilation &compilation() const noexcept;

        [[nodiscard]] static bool is_numeric_type(const TypeSymbol &type) noexcept;

        [[nodiscard]] static bool is_numeric_special_type(SpecialType type) noexcept;

        [[nodiscard]] Conversion classify_numeric_conversion(SpecialType source,
                                                             SpecialType destination) const noexcept;

        [[nodiscard]] bool is_implicit_numeric_conversion(SpecialType source, SpecialType destination) const noexcept;

        enum class NumericFamily : std::uint8_t
        {
            none,
            signed_integer,
            unsigned_integer,
            floating_point,
        };

        struct NumericInfo final
        {
            NumericFamily family = NumericFamily::none;
            std::uint16_t width = 0;
            std::uint16_t precision = 0;
        };

        [[nodiscard]] NumericInfo numeric_info(SpecialType type) const;

        [[nodiscard]] static bool is_integer_family(NumericFamily family) noexcept;

        const Binder &binder_;
    };
} // namespace prism
