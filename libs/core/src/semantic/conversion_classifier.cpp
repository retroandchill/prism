/**
 * @file conversion_classifier.cpp
 * @author Francesco Corso
 * @date 8/9/2026
 * @brief
 */
module prism.core:semantic.conversion_classifier.impl;

import :semantic.conversion_classifier;
import :binder;
import :semantic.compilation;

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

    const Compilation &ConversionClassifier::compilation() const noexcept
    {
        return binder_.compilation();
    }

    bool ConversionClassifier::is_numeric_type(const TypeSymbol &type) noexcept
    {
        return is_numeric_special_type(type.special_type());
    }

    bool ConversionClassifier::is_numeric_special_type(SpecialType type) noexcept
    {
        switch (type)
        {
            case SpecialType::i8:
            case SpecialType::i16:
            case SpecialType::i32:
            case SpecialType::i64:
            case SpecialType::i128:
            case SpecialType::isize:
            case SpecialType::u8:
            case SpecialType::u16:
            case SpecialType::u32:
            case SpecialType::u64:
            case SpecialType::u128:
            case SpecialType::usize:
            case SpecialType::f16:
            case SpecialType::f32:
            case SpecialType::f64:
                return true;

            default:
                return false;
        }
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

            case SpecialType::f16:
                return {.family = NumericFamily::floating_point, .width = 16, .precision = 11};

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

} // namespace prism
