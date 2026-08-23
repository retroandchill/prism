/**
 * @file type_symbol.ixx
 * @author Francesco Corso
 * @date 7/30/2026
 * @brief
 */
export module prism.core:symbols.type_symbol;

import :symbols.member_container_symbol;

namespace prism
{
    export enum class SpecialType : std::uint8_t
    {
        none,
        void_,
        bool_,
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
        char_,
        char16,
        rune,
        str
    };

    constexpr bool is_integer(const SpecialType type) noexcept
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
                return true;

            default:
                return false;
        }
    }

    constexpr bool is_signed_integer(const SpecialType type) noexcept
    {
        switch (type)
        {
            case SpecialType::i8:
            case SpecialType::i16:
            case SpecialType::i32:
            case SpecialType::i64:
            case SpecialType::i128:
            case SpecialType::isize:
                return true;

            default:
                return false;
        }
    }

    constexpr bool is_unsigned_integer(const SpecialType type) noexcept
    {
        switch (type)
        {
            case SpecialType::u8:
            case SpecialType::u16:
            case SpecialType::u32:
            case SpecialType::u64:
            case SpecialType::u128:
            case SpecialType::usize:
                return true;

            default:
                return false;
        }
    }

    constexpr bool is_floating_point(const SpecialType type) noexcept
    {
        return type == SpecialType::f32 || type == SpecialType::f64;
    }

    constexpr bool is_numeric(const SpecialType type) noexcept
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
            case SpecialType::f32:
            case SpecialType::f64:
                return true;

            default:
                return false;
        }
    }

    constexpr bool is_character(const SpecialType type) noexcept
    {
        return type == SpecialType::char_ || type == SpecialType::char16 || type == SpecialType::rune;
    }

    export class TypeSymbol : public MemberContainerSymbol
    {
      protected:
        constexpr TypeSymbol(const SymbolKind kind,
                             const Name name,
                             const Symbol *containing,
                             const SpecialType special_type = SpecialType::none)
            : MemberContainerSymbol{kind, name, containing}, special_type_{special_type}
        {
        }

        ~TypeSymbol() noexcept = default;

      public:
        [[nodiscard]] constexpr SpecialType special_type() const noexcept
        {
            return special_type_;
        }

        [[nodiscard]] constexpr bool is_void() const noexcept
        {
            return special_type_ == SpecialType::void_;
        }

        static constexpr bool instance_of(const Symbol &symbol) noexcept
        {
            return symbol.kind() == SymbolKind::named_type;
        }

      private:
        SpecialType special_type_;
    };
} // namespace prism
