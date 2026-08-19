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
