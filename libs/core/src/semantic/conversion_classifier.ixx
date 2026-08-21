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
import :semantic.operations;

namespace prism
{
    class Binder;

    struct OperandConversion
    {
        Conversion conversion;
        Ref<const TypeSymbol> type;
    };

    struct BinaryOperandConversion
    {
        Conversion left_conversion;
        Conversion right_conversion;
        Ref<const TypeSymbol> type;
    };

    class ConversionClassifier final
    {
      public:
        explicit constexpr ConversionClassifier(const Binder &binder) : binder_{binder}
        {
        }

        [[nodiscard]] Conversion classify_conversion(const TypeSymbol &source, const TypeSymbol &destination) const;

        [[nodiscard]] Optional<OperandConversion> classify_unary_operand_type(UnaryOperation operation,
                                                                              const TypeSymbol &operand) const;

        [[nodiscard]] Optional<BinaryOperandConversion> classify_binary_operand_type(BinaryOperation operation,
                                                                                     const TypeSymbol &left,
                                                                                     const TypeSymbol &right) const;

      private:
        [[nodiscard]] const Compilation &compilation() const noexcept;

        [[nodiscard]] static bool is_numeric_type(const TypeSymbol &type) noexcept;

        [[nodiscard]] static bool is_integral_type(const TypeSymbol &type) noexcept;

        [[nodiscard]] Conversion classify_numeric_conversion(SpecialType source,
                                                             SpecialType destination) const noexcept;

        [[nodiscard]] bool is_implicit_numeric_conversion(SpecialType source, SpecialType destination) const noexcept;

        [[nodiscard]] const TypeSymbol &promote_numeric_type(const TypeSymbol &source) const;
        [[nodiscard]] Optional<const TypeSymbol &> promote_negation_type(const TypeSymbol &source) const;

        [[nodiscard]] Optional<const TypeSymbol &> get_common_numeric_type(const TypeSymbol &left,
                                                                           const TypeSymbol &right) const;

        [[nodiscard]] const TypeSymbol &wider_precision(const TypeSymbol &left, const TypeSymbol &right) const;

        [[nodiscard]] Optional<const TypeSymbol &> common_float_type(const TypeSymbol &float_type,
                                                                     const TypeSymbol &integer_type) const;

        [[nodiscard]] Optional<const TypeSymbol &> common_signed_type(const TypeSymbol &signed_type,
                                                                      const TypeSymbol &unsigned_type) const;

        [[nodiscard]] static bool is_character_type(const TypeSymbol &type) noexcept;

        [[nodiscard]] static std::int32_t character_width(SpecialType type) noexcept;

        [[nodiscard]] static Conversion classify_character_conversion(SpecialType source,
                                                                      SpecialType destination) noexcept;

        [[nodiscard]] static const TypeSymbol &get_common_character_type(const TypeSymbol &left,
                                                                         const TypeSymbol &right) noexcept;

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
