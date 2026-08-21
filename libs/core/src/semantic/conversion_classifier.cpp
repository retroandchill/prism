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

        if (is_character_type(source) && is_character_type(destination))
        {
            return classify_character_conversion(source.special_type(), destination.special_type());
        }

        return no_conversion;
    }

    Optional<OperandConversion> ConversionClassifier::classify_unary_operand_type(const UnaryOperation operation,
                                                                                  const TypeSymbol &operand) const
    {
        switch (operation)
        {
            case UnaryOperation::identity:
                if (is_numeric_type(operand))
                {
                    auto &promoted = promote_numeric_type(operand);
                    return OperandConversion{
                        .conversion = classify_numeric_conversion(operand.special_type(), promoted.special_type()),
                        .type = promoted};
                }
                break;
            case UnaryOperation::negation:
                if (is_numeric_type(operand))
                {
                    return promote_negation_type(operand).transform(
                        [&](const TypeSymbol &promoted)
                        {
                            return OperandConversion{.conversion = classify_numeric_conversion(operand.special_type(),
                                                                                               promoted.special_type()),
                                                     .type = promoted};
                        });
                }
                break;
            case UnaryOperation::logical_not:
                if (operand.special_type() == SpecialType::bool_)
                {
                    return OperandConversion{.conversion = get_trivial_conversion(ConversionKind::identity),
                                             .type = operand};
                }
                break;
            case UnaryOperation::bitwise_not:
                if (is_integral_type(operand))
                {
                    auto &promoted = promote_numeric_type(operand);
                    return OperandConversion{
                        .conversion = classify_numeric_conversion(operand.special_type(), promoted.special_type()),
                        .type = promoted};
                }
                break;
            case UnaryOperation::pre_increment:
            case UnaryOperation::pre_decrement:
            case UnaryOperation::post_increment:
            case UnaryOperation::post_decrement:
                if (is_numeric_type(operand))
                {
                    return OperandConversion{.conversion = get_trivial_conversion(ConversionKind::identity),
                                             .type = operand};
                }
                break;
        }

        return std::nullopt;
    }

    Optional<BinaryOperandConversion> ConversionClassifier::classify_binary_operand_type(
        const BinaryOperation operation,
        const TypeSymbol &left,
        const TypeSymbol &right) const
    {
        const auto get_numeric_conversion = [&](const TypeSymbol &promoted)
        {
            return BinaryOperandConversion{
                .left_conversion = classify_numeric_conversion(left.special_type(), promoted.special_type()),
                .right_conversion = classify_numeric_conversion(right.special_type(), promoted.special_type()),
                .type = promoted};
        };

        switch (operation)
        {
            case BinaryOperation::addition:
            case BinaryOperation::subtraction:
            case BinaryOperation::multiplication:
            case BinaryOperation::division:
            case BinaryOperation::modulo:
                if (is_numeric_type(left) && is_numeric_type(right))
                {
                    return get_common_numeric_type(left, right).transform(get_numeric_conversion);
                }
                break;
            case BinaryOperation::bitwise_and:
            case BinaryOperation::bitwise_or:
            case BinaryOperation::bitwise_xor:
            case BinaryOperation::shift_left:
            case BinaryOperation::shift_right:
            case BinaryOperation::unsigned_shift_right:
                if (is_integral_type(left) && is_integral_type(right))
                {
                    return get_common_numeric_type(left, right).transform(get_numeric_conversion);
                }
                break;
            case BinaryOperation::logical_and:
            case BinaryOperation::logical_or:
                if (left.special_type() == SpecialType::bool_ && right.special_type() == SpecialType::bool_)
                {
                    return BinaryOperandConversion{.left_conversion = get_trivial_conversion(ConversionKind::identity),
                                                   .right_conversion = get_trivial_conversion(ConversionKind::identity),
                                                   .type = left};
                }
                break;
            case BinaryOperation::equals:
            case BinaryOperation::not_equals:
                if (left.special_type() == SpecialType::bool_ && right.special_type() == SpecialType::bool_)
                {
                    return BinaryOperandConversion{.left_conversion = get_trivial_conversion(ConversionKind::identity),
                                                   .right_conversion = get_trivial_conversion(ConversionKind::identity),
                                                   .type = left};
                }

                if (is_numeric_type(left) && is_numeric_type(right))
                {
                    return get_common_numeric_type(left, right).transform(get_numeric_conversion);
                }

                if (is_character_type(left) && is_numeric_type(right))
                {
                    auto &promoted = get_common_character_type(left, right);
                    return BinaryOperandConversion{
                        .left_conversion = classify_character_conversion(left.special_type(), promoted.special_type()),
                        .right_conversion = classify_numeric_conversion(promoted.special_type(), right.special_type()),
                        .type = promoted};
                }

                break;
            case BinaryOperation::less_than:
            case BinaryOperation::less_than_or_equals:
            case BinaryOperation::greater_than:
            case BinaryOperation::greater_than_or_equals:
            case BinaryOperation::three_way_comparison:
                if (is_numeric_type(left) && is_numeric_type(right))
                {
                    return get_common_numeric_type(left, right).transform(get_numeric_conversion);
                }
                break;
            case BinaryOperation::null_coalescing:
                throw NotImplementedException{};
        }

        UNREACHABLE("Invalid binary operation");
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
                        throw InvalidStateException{"Invalid pointer width"};
                }

            case SpecialType::u128:
                return std::nullopt;

            default:
                return source;
        }
    }
    Optional<const TypeSymbol &> ConversionClassifier::get_common_numeric_type(const TypeSymbol &left,
                                                                               const TypeSymbol &right) const
    {
        DEBUG_ASSERT(is_numeric(left.special_type()) && is_numeric(right.special_type()));

        if (left.special_type() == right.special_type())
            return promote_numeric_type(left);

        auto left_family = numeric_info(left.special_type()).family;
        auto right_family = numeric_info(right.special_type()).family;

        if (left_family == right_family)
        {
            return promote_numeric_type(wider_precision(left, right));
        }

        if (is_integer_family(left_family) && right_family == NumericFamily::floating_point)
        {
            return common_float_type(right, left);
        }

        if (left_family == NumericFamily::floating_point && is_integer_family(right_family))
        {
            return common_float_type(left, right);
        }

        if (left_family == NumericFamily::signed_integer && right_family == NumericFamily::unsigned_integer)
        {
            return common_signed_type(left, right)
                .transform([this](const TypeSymbol &symbol) -> auto & { return promote_numeric_type(symbol); });
        }

        if (left_family == NumericFamily::unsigned_integer && right_family == NumericFamily::signed_integer)
        {
            return common_signed_type(right, left)
                .transform([this](const TypeSymbol &symbol) -> auto & { return promote_numeric_type(symbol); });
        }

        return std::nullopt;
    }

    const TypeSymbol &ConversionClassifier::wider_precision(const TypeSymbol &left, const TypeSymbol &right) const
    {
        const auto left_info = numeric_info(left.special_type());
        const auto right_info = numeric_info(right.special_type());
        DEBUG_ASSERT(left_info.family == right_info.family);

        if (left_info.precision < right_info.precision)
            return right;

        return left;
    }

    Optional<const TypeSymbol &> ConversionClassifier::common_float_type(const TypeSymbol &float_type,
                                                                         const TypeSymbol &integer_type) const
    {
        const auto float_info = numeric_info(float_type.special_type());
        const auto integer_info = numeric_info(integer_type.special_type());
        DEBUG_ASSERT(float_info.family == NumericFamily::floating_point);
        DEBUG_ASSERT(is_integer_family(integer_info.family));

        if (float_info.precision <= integer_info.precision)
            return float_type;

        if (float_type.special_type() == SpecialType::f32 &&
            integer_info.precision <= numeric_info(SpecialType::f64).precision)
        {
            return compilation().get_special_type(SpecialType::f64);
        }

        return std::nullopt;
    }

    Optional<const TypeSymbol &> ConversionClassifier::common_signed_type(const TypeSymbol &signed_type,
                                                                          const TypeSymbol &unsigned_type) const
    {
        const auto signed_info = numeric_info(signed_type.special_type());
        const auto unsigned_info = numeric_info(unsigned_type.special_type());
        DEBUG_ASSERT(signed_info.family == NumericFamily::signed_integer);
        DEBUG_ASSERT(unsigned_info.family == NumericFamily::unsigned_integer);

        if (signed_info.precision > unsigned_info.precision)
            return signed_type;

        switch (unsigned_type.special_type())
        {
            case SpecialType::u8:
                return compilation().get_special_type(SpecialType::i16);
            case SpecialType::u16:
                return compilation().get_special_type(SpecialType::i32);
            case SpecialType::u32:
                return compilation().get_special_type(SpecialType::i64);
            case SpecialType::u64:
                return compilation().get_special_type(SpecialType::i128);
            case SpecialType::u128:
                return std::nullopt;
            case SpecialType::usize:
                switch (compilation().target_settings().pointer_width)
                {
                    case 32:
                        return compilation().get_special_type(SpecialType::i64);
                    case 64:
                        return compilation().get_special_type(SpecialType::i128);
                    default:
                        throw InvalidStateException{"Invalid pointer width"};
                }
            default:
                UNREACHABLE("This isn't a valid path");
        }
    }

    bool ConversionClassifier::is_character_type(const TypeSymbol &type) noexcept
    {
        return is_character(type.special_type());
    }

    std::int32_t ConversionClassifier::character_width(const SpecialType type) noexcept
    {
        DEBUG_ASSERT(is_character(type));
        switch (type)
        {
            case SpecialType::char_:
                return 8;
            case SpecialType::char16:
                return 16;
            case SpecialType::rune:
                return 32;
            default:
                std::unreachable();
        }
    }

    Conversion ConversionClassifier::classify_character_conversion(const SpecialType source,
                                                                   const SpecialType destination) noexcept
    {
        if (source == destination)
            return get_trivial_conversion(ConversionKind::identity);

        const auto source_width = character_width(source);
        const auto destination_width = character_width(destination);
        DEBUG_ASSERT(source_width != destination_width);

        if (source_width > destination_width)
            return get_trivial_conversion(ConversionKind::explicit_character);

        return get_trivial_conversion(ConversionKind::implicit_character);
    }

    const TypeSymbol &ConversionClassifier::get_common_character_type(const TypeSymbol &left,
                                                                      const TypeSymbol &right) noexcept
    {
        if (&left == &right)
            return left;

        const auto source_width = character_width(left.special_type());
        const auto destination_width = character_width(right.special_type());
        DEBUG_ASSERT(source_width != destination_width);

        if (source_width > destination_width)
            return left;

        return right;
    }

} // namespace prism
