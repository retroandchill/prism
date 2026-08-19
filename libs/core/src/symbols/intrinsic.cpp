/**
 * @file intrinsic.cpp
 * @author Francesco Corso
 * @date 8/5/2026
 * @brief
 */
module prism.core:symbols.intrinsic.impl;

import :symbols.intrinsic;

namespace prism
{
    std::span<const SyntaxReference> IntrinsicNamedTypeSymbol::declaring_syntax_references() const
    {
        return {};
    }

    SymbolSpan<Symbol> IntrinsicNamedTypeSymbol::members() const
    {
        return {};
    }

    SymbolSpan<Symbol> IntrinsicNamedTypeSymbol::members(Name) const
    {
        return {};
    }

    std::span<const SyntaxReference> IntrinsicNamespaceSymbol::declaring_syntax_references() const
    {
        return {};
    }

    Optional<const Compilation &> IntrinsicNamespaceSymbol::containing_compilation() const noexcept
    {
        return std::nullopt;
    }

    SymbolSpan<Symbol> IntrinsicNamespaceSymbol::members(const Name name) const
    {
        const auto it = name_to_members_.find(name);
        if (it == name_to_members_.end())
        {
            return {};
        }

        return it->second;
    }

    void IntrinsicNamespaceSymbol::add_member(const Symbol &member)
    {
        members_.emplace_back(member);
        name_to_members_[member.name()].emplace_back(member);
    }

    IntrinsicSymbols::IntrinsicSymbols()
        : global_namespace_{KnownName::global, nullptr}, std_namespace_{KnownName::std, &global_namespace_},
          void_{SpecialType::void_, std_namespace_}, bool_{SpecialType::bool_, std_namespace_},
          i8_{SpecialType::i8, std_namespace_}, i16_{SpecialType::i16, std_namespace_},
          i32_{SpecialType::i32, std_namespace_}, i64_{SpecialType::i64, std_namespace_},
          i128_{SpecialType::i128, std_namespace_}, isize_{SpecialType::isize, std_namespace_},
          u8_{SpecialType::u8, std_namespace_}, u16_{SpecialType::u16, std_namespace_},
          u32_{SpecialType::u32, std_namespace_}, u64_{SpecialType::u64, std_namespace_},
          u128_{SpecialType::u128, std_namespace_}, usize_{SpecialType::usize, std_namespace_},
          f32_{SpecialType::f32, std_namespace_}, f64_{SpecialType::f64, std_namespace_},
          char_{SpecialType::char_, std_namespace_}, char16_{SpecialType::char16, std_namespace_},
          rune_{SpecialType::rune, std_namespace_}, str_{SpecialType::str, std_namespace_}
    {
        global_namespace_.add_member(std_namespace_);

        std_namespace_.add_member(void_);
        std_namespace_.add_member(bool_);
        std_namespace_.add_member(i8_);
        std_namespace_.add_member(i16_);
        std_namespace_.add_member(i32_);
        std_namespace_.add_member(i64_);
        std_namespace_.add_member(i128_);
        std_namespace_.add_member(isize_);
        std_namespace_.add_member(u8_);
        std_namespace_.add_member(u16_);
        std_namespace_.add_member(u32_);
        std_namespace_.add_member(u64_);
        std_namespace_.add_member(u128_);
        std_namespace_.add_member(usize_);
        std_namespace_.add_member(f32_);
        std_namespace_.add_member(f64_);
        std_namespace_.add_member(char_);
        std_namespace_.add_member(char16_);
        std_namespace_.add_member(rune_);
        std_namespace_.add_member(str_);
    }

    IntrinsicSymbols &IntrinsicSymbols::instance() noexcept
    {
        static IntrinsicSymbols instance;
        return instance;
    }

    const NamedTypeSymbol &IntrinsicSymbols::get_type(const SpecialType type) const
    {
        switch (type)
        {
            case SpecialType::void_:
                return void_;
            case SpecialType::bool_:
                return bool_;
            case SpecialType::i8:
                return i8_;
            case SpecialType::i16:
                return i16_;
            case SpecialType::i32:
                return i32_;
            case SpecialType::i64:
                return i64_;
            case SpecialType::i128:
                return i128_;
            case SpecialType::isize:
                return isize_;
            case SpecialType::u8:
                return u8_;
            case SpecialType::u16:
                return u16_;
            case SpecialType::u32:
                return u32_;
            case SpecialType::u64:
                return u64_;
            case SpecialType::u128:
                return u128_;
            case SpecialType::usize:
                return usize_;
            case SpecialType::f32:
                return f32_;
            case SpecialType::f64:
                return f64_;
            case SpecialType::char_:
                return char_;
            case SpecialType::char16:
                return char16_;
            case SpecialType::rune:
                return rune_;
            case SpecialType::str:
                return str_;
            default:
                throw std::invalid_argument{"Invalid special type"};
        }
    }
} // namespace prism
