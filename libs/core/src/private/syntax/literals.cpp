/**
 * @file literals.cpp
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
module;

#include "prism/core/syntax.hpp"

module prism.core:syntax.literals.impl;

import :syntax.literals;
import :memory.buffer_pool;

namespace prism
{

    IntegerLiteralData IntegerLiteralData::parse(std::string_view text)
    {
        using namespace std::literals;
        static constexpr std::array integer_suffixes = {
            std::make_pair("i8"sv, IntegerSuffix::i8),
            std::make_pair("i16"sv, IntegerSuffix::i16),
            std::make_pair("i32"sv, IntegerSuffix::i32),
            std::make_pair("i64"sv, IntegerSuffix::i64),
            std::make_pair("i128"sv, IntegerSuffix::i128),
            std::make_pair("u8"sv, IntegerSuffix::u8),
            std::make_pair("u16"sv, IntegerSuffix::u16),
            std::make_pair("u32"sv, IntegerSuffix::u32),
            std::make_pair("u64"sv, IntegerSuffix::u64),
            std::make_pair("u128"sv, IntegerSuffix::u128),
            std::make_pair("iz"sv, IntegerSuffix::iz),
            std::make_pair("uz"sv, IntegerSuffix::uz),
        };

        auto suffix = IntegerSuffix::none;
        for (auto [str, type] : integer_suffixes)
        {
            if (!text.ends_with(str))
                continue;
            suffix = type;
            text.remove_suffix(str.size());
            break;
        }

        IntegerBase base;
        std::uint8_t base_value;
        if (text.starts_with("0x") || text.starts_with("0X"))
        {
            base = IntegerBase::hex;
            base_value = 16;
        }
        else if (text.starts_with("0b") || text.starts_with("0B"))
        {
            base = IntegerBase::binary;
            base_value = 2;
        }
        else
        {
            base = IntegerBase::decimal;
            base_value = 10;
        }

        Optional<PooledString> scratch_buffer;
        if (text.contains('_'))
        {
            scratch_buffer.emplace();
            scratch_buffer->reserve(text.size());
            std::size_t index = 0;
            for (auto c : text)
            {
                if (c == '_')
                    continue;

                scratch_buffer->push_back(c);
                index++;
            }

            text = *scratch_buffer;
        }

        BigInteger value = 0;
        for (const auto c : text)
        {
            value *= base_value;
            value += hex_digit_value(c);
        }

        return IntegerLiteralData{
            .value = std::move(value),
            .base = base,
            .suffix = suffix,
        };
    }

    FloatLiteralData FloatLiteralData::parse(std::string_view text)
    {
        auto suffix = FloatSuffix::none;
        using namespace std::literals;
        static constexpr std::array float_suffixes = {std::make_pair("f16"sv, FloatSuffix::f16),
                                                      std::make_pair("f32"sv, FloatSuffix::f32),
                                                      std::make_pair("f64"sv, FloatSuffix::f64)};
        for (auto [str, type] : float_suffixes)
        {
            if (!text.ends_with(str))
                continue;

            suffix = type;
            text.remove_suffix(str.size());
            break;
        }

        return FloatLiteralData{
            .value = BigDecimal{text},
            .suffix = suffix,
        };
    }
} // namespace prism
