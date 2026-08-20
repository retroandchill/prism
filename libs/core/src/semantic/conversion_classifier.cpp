/**
 * @file conversion_classifier.cpp
 * @author Francesco Corso
 * @date 8/9/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:semantic.conversion_classifier.impl;

import :semantic.conversion_classifier;
import :binder;
import :semantic.compilation;
import :symbols.named_type_symbol;

namespace prism
{
    Conversion ConversionClassifier::classify_conversion(const TypeSymbol &source, const TypeSymbol &destination) const
    {
        if (&source == &destination)
        {
            return get_trivial_conversion(ConversionKind::identity);
        }

        if (source.special_type() == destination.special_type() && source.special_type() != SpecialType::none)
            [[unlikely]]
        {
            return get_trivial_conversion(ConversionKind::identity);
        }

        if (is_numeric_type(source) && is_numeric_type(destination))
        {
            return classify_numeric_conversion(source.special_type(), destination.special_type());
        }

        return no_conversion;
    }

    Optional<const TypeSymbol &> ConversionClassifier::classify_unary_operand_type(const UnaryOperation operation,
                                                                                   const TypeSymbol &operand) const
    {
        switch (operation)
        {
            case UnaryOperation::identity:
                if (is_numeric_type(operand))
                {
                    return promote_numeric_type(operand);
                }
                break;
            case UnaryOperation::negation:
                if (is_numeric_type(operand))
                {
                    return promote_negation_type(operand);
                }
                break;
            case UnaryOperation::logical_not:
                if (operand.special_type() == SpecialType::bool_)
                {
                    return operand;
                }
                break;
            case UnaryOperation::bitwise_not:
                if (is_integral_type(operand))
                {
                    return promote_numeric_type(operand);
                }
                break;
            case UnaryOperation::pre_increment:
            case UnaryOperation::pre_decrement:
            case UnaryOperation::post_increment:
            case UnaryOperation::post_decrement:
                if (is_numeric_type(operand))
                {
                    return operand;
                }
                break;
        }

        return std::nullopt;
    }

    Optional<const TypeSymbol &> ConversionClassifier::classify_binary_operand_type(BinaryOperation operation,
                                                                                    const TypeSymbol &left,
                                                                                    const TypeSymbol &right) const
    {
        throw NotImplementedException{};
    }

    const Compilation &ConversionClassifier::compilation() const noexcept
    {
        return binder_.compilation();
    }

    bool ConversionClassifier::is_numeric_type(const TypeSymbol &type) noexcept
    {
        return is_numeric(type.special_type());
    }

    bool ConversionClassifier::is_integral_type(const TypeSymbol &type) noexcept
    {
        return is_integer(type.special_type());
    }

    Conversion ConversionClassifier::classify_numeric_conversion(const SpecialType source,
                                                                 const SpecialType destination) const noexcept
    {
        if (source == destination)
        {
            return get_trivial_conversion(ConversionKind::identity);
        }

        if (is_implicit_numeric_conversion(source, destination))
        {
            return get_trivial_conversion(ConversionKind::implicit_numeric);
        }

        return get_trivial_conversion(ConversionKind::explicit_numeric);
    }

    bool ConversionClassifier::is_implicit_numeric_conversion(const SpecialType source,
                                                              const SpecialType destination) const noexcept
    {
        const NumericInfo source_info = numeric_info(source);
        const NumericInfo destination_info = numeric_info(destination);

        if (source_info.family == NumericFamily::none || destination_info.family == NumericFamily::none)
        {
            return false;
        }

        if ((source_info.family == NumericFamily::signed_integer &&
             destination_info.family == NumericFamily::signed_integer) ||
            (source_info.family == NumericFamily::unsigned_integer &&
             destination_info.family == NumericFamily::unsigned_integer))
        {
            return source_info.width <= destination_info.width;
        }

        if (source_info.family == NumericFamily::unsigned_integer &&
            destination_info.family == NumericFamily::signed_integer)
        {
            return source_info.width <= destination_info.width;
        }

        if (is_integer_family(source_info.family) && destination_info.family == NumericFamily::floating_point)
        {
            return source_info.precision <= destination_info.precision;
        }

        if (source_info.family == NumericFamily::floating_point &&
            destination_info.family == NumericFamily::floating_point)
        {
            return source_info.width <= destination_info.width;
        }

        return false;
    }

    ConversionClassifier::NumericInfo ConversionClassifier::numeric_info(const SpecialType type) const
    {
        switch (type)
        {
            case SpecialType::i8:
                return {.family = NumericFamily::signed_integer, .width = 8, .precision = 8};

            case SpecialType::i16:
                return {.family = NumericFamily::signed_integer, .width = 16, .precision = 16};

            case SpecialType::i32:
                return {.family = NumericFamily::signed_integer, .width = 32, .precision = 32};

            case SpecialType::i64:
                return {.family = NumericFamily::signed_integer, .width = 64, .precision = 64};

            case SpecialType::i128:
                return {.family = NumericFamily::signed_integer, .width = 128, .precision = 128};

            case SpecialType::isize:
                {
                    auto [pointer_width] = compilation().target_settings();
                    return {.family = NumericFamily::signed_integer,
                            .width = pointer_width,
                            .precision = pointer_width};
                }

            case SpecialType::u8:
                return {.family = NumericFamily::unsigned_integer, .width = 8, .precision = 8};

            case SpecialType::u16:
                return {.family = NumericFamily::unsigned_integer, .width = 16, .precision = 16};

            case SpecialType::u32:
                return {.family = NumericFamily::unsigned_integer, .width = 32, .precision = 32};

            case SpecialType::u64:
                return {.family = NumericFamily::unsigned_integer, .width = 64, .precision = 64};

            case SpecialType::u128:
                return {.family = NumericFamily::unsigned_integer, .width = 128, .precision = 128};

            case SpecialType::usize:
                {
                    auto [pointer_width] = compilation().target_settings();
                    return {.family = NumericFamily::unsigned_integer,
                            .width = pointer_width,
                            .precision = pointer_width};
                }

            case SpecialType::f32:
                return {.family = NumericFamily::floating_point, .width = 32, .precision = 24};

            case SpecialType::f64:
                return {.family = NumericFamily::floating_point, .width = 64, .precision = 53};

            default:
                return {};
        }
    }

    bool ConversionClassifier::is_integer_family(const NumericFamily family) noexcept
    {
        return family == NumericFamily::signed_integer || family == NumericFamily::unsigned_integer;
    }

    const TypeSymbol &ConversionClassifier::promote_numeric_type(const TypeSymbol &source) const
    {
        DEBUG_ASSERT(is_numeric(source.special_type()));
        switch (source.special_type())
        {
            case SpecialType::i8:
            case SpecialType::i16:
            case SpecialType::u8:
            case SpecialType::u16:
                return compilation().get_special_type(SpecialType::i32);
            default:
                return source;
        }
    }

    Optional<const TypeSymbol &> ConversionClassifier::promote_negation_type(const TypeSymbol &source) const
    {
        DEBUG_ASSERT(is_numeric(source.special_type()));

        switch (source.special_type())
        {
            case SpecialType::i8:
            case SpecialType::i16:
            case SpecialType::u8:
            case SpecialType::u16:
                return compilation().get_special_type(SpecialType::i32);

            case SpecialType::u32:
                return compilation().get_special_type(SpecialType::i64);

            case SpecialType::u64:
                return compilation().get_special_type(SpecialType::i128);

            case SpecialType::usize:
                switch (compilation().target_settings().pointer_width)
                {
                    case 32:
                        return compilation().get_special_type(SpecialType::i64);
                    case 64:
                        return compilation().get_special_type(SpecialType::i128);
                    default:
                        throw InvalidStateException("Invalid pointer width");
                }

            case SpecialType::u128:
                return std::nullopt;

            default:
                return source;
        }
    }

} // namespace prism
