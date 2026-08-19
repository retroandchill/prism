/**
 * @file constant_value.ixx
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:semantic.constant_value;

import std;
import libassert;
import :util.numerics;
import :symbols.type_symbol;

namespace prism
{
    export class ConstantValue final
    {
        union Storage
        {
            bool bool_value;
            char32_t character_value;
            std::int64_t i64_value;
            std::uint64_t u64_value;
            Int128 i128_value;
            UInt128 u128_value;
            float f32_value;
            double f64_value;
            std::string_view str_value;
        };

      public:
        enum class Kind : std::uint8_t
        {
            bool_,
            char_,
            char16,
            rune,
            i8,
            i16,
            i32,
            i64,
            i128,
            isize,
            u8,
            u16,
            u32,
            u64,
            u128,
            usize,
            f32,
            f64,
            str
        };

      private:
        constexpr ConstantValue(const Kind kind, const Storage storage) noexcept : kind_{kind}, storage_{storage}
        {
        }

      public:
        [[nodiscard]] static constexpr ConstantValue boolean(const bool value) noexcept
        {
            return ConstantValue{Kind::bool_, {.bool_value = value}};
        }

        [[nodiscard]] static constexpr ConstantValue character(const char8_t value) noexcept
        {
            return ConstantValue{Kind::char_, {.character_value = value}};
        }

        [[nodiscard]] static constexpr ConstantValue character16(const char16_t value) noexcept
        {
            return ConstantValue{Kind::char16, {.character_value = value}};
        }

        [[nodiscard]] static constexpr ConstantValue rune(const char32_t value) noexcept
        {
            return ConstantValue{Kind::rune, {.character_value = value}};
        }

        [[nodiscard]] static constexpr ConstantValue i8(const std::int8_t value) noexcept
        {
            return ConstantValue{Kind::i8, {.i64_value = value}};
        }

        [[nodiscard]] static constexpr ConstantValue i16(const std::int16_t value) noexcept
        {
            return ConstantValue{Kind::i16, {.i64_value = value}};
        }

        [[nodiscard]] static constexpr ConstantValue i32(const std::int32_t value) noexcept
        {
            return ConstantValue{Kind::i32, {.i64_value = value}};
        }

        [[nodiscard]] static constexpr ConstantValue i64(const std::int64_t value) noexcept
        {
            return ConstantValue{Kind::i64, {.i64_value = value}};
        }

        [[nodiscard]] static constexpr ConstantValue i128(const Int128 value) noexcept
        {
            return ConstantValue{Kind::i128, {.i128_value = value}};
        }

        [[nodiscard]] static constexpr ConstantValue isize(const std::int64_t value) noexcept
        {
            return ConstantValue{Kind::isize, {.i64_value = value}};
        }

        [[nodiscard]] static constexpr ConstantValue u8(const std::uint8_t value) noexcept
        {
            return ConstantValue{Kind::u8, {.u64_value = value}};
        }

        [[nodiscard]] static constexpr ConstantValue u16(const std::uint16_t value) noexcept
        {
            return ConstantValue{Kind::u16, {.u64_value = value}};
        }

        [[nodiscard]] static constexpr ConstantValue u32(const std::uint32_t value) noexcept
        {
            return ConstantValue{Kind::u32, {.u64_value = value}};
        }

        [[nodiscard]] static constexpr ConstantValue u64(const std::uint64_t value) noexcept
        {
            return ConstantValue{Kind::u64, {.u64_value = value}};
        }

        [[nodiscard]] static constexpr ConstantValue u128(const UInt128 value) noexcept
        {
            return ConstantValue{Kind::u128, {.u128_value = value}};
        }

        [[nodiscard]] static constexpr ConstantValue usize(const std::uint64_t value) noexcept
        {
            return ConstantValue{Kind::usize, {.u64_value = value}};
        }

        [[nodiscard]] static constexpr ConstantValue f32(const float value) noexcept
        {
            return ConstantValue{Kind::f32, {.f32_value = value}};
        }

        [[nodiscard]] static constexpr ConstantValue f64(const double value) noexcept
        {
            return ConstantValue{Kind::f64, {.f64_value = value}};
        }

        [[nodiscard]] static constexpr ConstantValue str(const std::string_view value) noexcept
        {
            return ConstantValue{Kind::str, {.str_value = value}};
        }

        [[nodiscard]] constexpr Kind kind() const noexcept
        {
            return kind_;
        }

        [[nodiscard]] constexpr SpecialType special_type() const noexcept
        {
            switch (kind_)
            {
                case Kind::bool_:
                    return SpecialType::bool_;
                case Kind::char_:
                    return SpecialType::char_;
                case Kind::char16:
                    return SpecialType::char16;
                case Kind::rune:
                    return SpecialType::rune;
                case Kind::i8:
                    return SpecialType::i8;
                case Kind::i16:
                    return SpecialType::i16;
                case Kind::i32:
                    return SpecialType::i32;
                case Kind::i64:
                    return SpecialType::i64;
                case Kind::i128:
                    return SpecialType::i128;
                case Kind::isize:
                    return SpecialType::isize;
                case Kind::u8:
                    return SpecialType::u8;
                case Kind::u16:
                    return SpecialType::u16;
                case Kind::u32:
                    return SpecialType::u32;
                case Kind::u64:
                    return SpecialType::u64;
                case Kind::u128:
                    return SpecialType::u128;
                case Kind::usize:
                    return SpecialType::usize;
                case Kind::f32:
                    return SpecialType::f32;
                case Kind::f64:
                    return SpecialType::f64;
                case Kind::str:
                    return SpecialType::str;
                default:
                    UNREACHABLE("Invalid special type");
            }
        }

        [[nodiscard]] bool is_numeric() const noexcept
        {
            return is_signed_integer() || is_unsigned_integer() || is_float();
        }

        [[nodiscard]] constexpr bool is_signed_integer() const noexcept
        {
            return kind_ == Kind::i8 || kind_ == Kind::i16 || kind_ == Kind::i32 || kind_ == Kind::i64 ||
                   kind_ == Kind::i128 || kind_ == Kind::isize;
        }

        [[nodiscard]] constexpr bool is_unsigned_integer() const noexcept
        {
            return kind_ == Kind::u8 || kind_ == Kind::u16 || kind_ == Kind::u32 || kind_ == Kind::u64 ||
                   kind_ == Kind::u128 || kind_ == Kind::usize;
        }

        [[nodiscard]] constexpr bool is_float() const noexcept
        {
            return kind_ == Kind::f32 || kind_ == Kind::f64;
        }

        [[nodiscard]] constexpr bool can_be_negative() const noexcept
        {
            return is_signed_integer() || is_float();
        }

        [[nodiscard]] constexpr bool is_character() const noexcept
        {
            return kind_ == Kind::char_ || kind_ == Kind::char16 || kind_ == Kind::rune;
        }

        [[nodiscard]] constexpr bool as_boolean() const noexcept
        {
            DEBUG_ASSERT(kind_ == Kind::bool_);
            return storage_.bool_value;
        }

        [[nodiscard]] constexpr char32_t as_character() const noexcept
        {
            DEBUG_ASSERT(is_character());
            return storage_.character_value;
        }

        [[nodiscard]] constexpr std::int64_t as_i64() const noexcept
        {
            DEBUG_ASSERT(is_signed_integer() && kind_ != Kind::i128);
            return storage_.i64_value;
        }

        [[nodiscard]] constexpr std::uint64_t as_u64() const noexcept
        {
            DEBUG_ASSERT(is_unsigned_integer() && kind_ != Kind::u128);
            return storage_.u64_value;
        }

        [[nodiscard]] constexpr const Int128 &as_i128() const noexcept
        {
            DEBUG_ASSERT(kind_ == Kind::i128);
            return storage_.i128_value;
        }

        [[nodiscard]] constexpr const UInt128 &as_u128() const noexcept
        {
            DEBUG_ASSERT(kind_ == Kind::u128);
            return storage_.u128_value;
        }

        [[nodiscard]] constexpr float as_f32() const noexcept
        {
            DEBUG_ASSERT(kind_ == Kind::f32);
            return storage_.f32_value;
        }

        [[nodiscard]] constexpr double as_f64() const noexcept
        {
            DEBUG_ASSERT(kind_ == Kind::f64);
            return storage_.f64_value;
        }

        [[nodiscard]] constexpr std::string_view as_str() const noexcept
        {
            DEBUG_ASSERT(kind_ == Kind::str);
            return storage_.str_value;
        }

      private:
        Kind kind_;
        Storage storage_;
    };
} // namespace prism
