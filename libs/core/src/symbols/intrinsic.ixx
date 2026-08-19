/**
 * @file intrinsic.ixx
 * @author Francesco Corso
 * @date 8/5/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:symbols.intrinsic;

import :symbols.namespace_symbol;
import :symbols.named_type_symbol;
import libassert;

namespace prism
{
    [[nodiscard]] constexpr KnownName get_built_in_type_name(const SpecialType type) noexcept
    {
        switch (type)
        {
            case SpecialType::none:
                return KnownName::none;
            case SpecialType::void_:
                return KnownName::void_type;
            case SpecialType::bool_:
                return KnownName::boolean;
            case SpecialType::i8:
                return KnownName::int8;
            case SpecialType::i16:
                return KnownName::int16;
            case SpecialType::i32:
                return KnownName::int32;
            case SpecialType::i64:
                return KnownName::int64;
            case SpecialType::i128:
                return KnownName::int128;
            case SpecialType::isize:
                return KnownName::isize;
            case SpecialType::u8:
                return KnownName::uint8;
            case SpecialType::u16:
                return KnownName::uint16;
            case SpecialType::u32:
                return KnownName::uint32;
            case SpecialType::u64:
                return KnownName::uint64;
            case SpecialType::u128:
                return KnownName::uint128;
            case SpecialType::usize:
                return KnownName::usize;
            case SpecialType::f32:
                return KnownName::float32;
            case SpecialType::f64:
                return KnownName::float64;
            case SpecialType::char_:
                return KnownName::char_type;
            case SpecialType::char16:
                return KnownName::char16;
            case SpecialType::rune:
                return KnownName::rune;
            case SpecialType::str:
                return KnownName::string_view;
            default:
                UNREACHABLE("Invalid special type");
        }
    }

    class IntrinsicNamedTypeSymbol final : public NamedTypeSymbol
    {
      public:
        constexpr IntrinsicNamedTypeSymbol(const SpecialType type, const NamespaceSymbol &containing)
            : NamedTypeSymbol{get_built_in_type_name(type), &containing, type}
        {
        }

        [[nodiscard]] const ImmutableArray<Location> &locations() const override
        {
            static constexpr ImmutableArray<Location> empty{};
            return empty;
        }

        [[nodiscard]] std::span<const SyntaxReference> declaring_syntax_references() const override;

        [[nodiscard]] SymbolSpan<Symbol> members() const override;

        [[nodiscard]] SymbolSpan<Symbol> members(Name name) const override;
    };

    class IntrinsicNamespaceSymbol final : public NamespaceSymbol
    {
      public:
        using NamespaceSymbol::NamespaceSymbol;

        [[nodiscard]] constexpr const ImmutableArray<Location> &locations() const override
        {
            static constexpr ImmutableArray<Location> empty{};
            return empty;
        }

        [[nodiscard]] constexpr NamespaceKind namespace_kind() const noexcept override
        {
            return NamespaceKind::assembly;
        }

        [[nodiscard]] std::span<const SyntaxReference> declaring_syntax_references() const override;
        [[nodiscard]] Optional<const Compilation &> containing_compilation() const noexcept override;
        [[nodiscard]] constexpr SymbolSpan<Symbol> members() const override
        {
            return members_;
        }

        [[nodiscard]] SymbolSpan<Symbol> members(Name name) const override;

      private:
        friend class IntrinsicSymbols;

        void add_member(const Symbol &member);

        std::vector<Ref<const Symbol>> members_{};
        std::unordered_map<Name, std::vector<Ref<const Symbol>>> name_to_members_;
    };

    class IntrinsicSymbols final
    {
        IntrinsicSymbols();
        ~IntrinsicSymbols() = default;

      public:
        static IntrinsicSymbols &instance() noexcept;

        [[nodiscard]] constexpr const NamespaceSymbol &global_namespace() const
        {
            return global_namespace_;
        }

        [[nodiscard]] const NamedTypeSymbol &get_type(SpecialType type) const;

      private:
        IntrinsicNamespaceSymbol global_namespace_;
        IntrinsicNamespaceSymbol std_namespace_;

        IntrinsicNamedTypeSymbol void_;
        IntrinsicNamedTypeSymbol bool_;
        IntrinsicNamedTypeSymbol i8_;
        IntrinsicNamedTypeSymbol i16_;
        IntrinsicNamedTypeSymbol i32_;
        IntrinsicNamedTypeSymbol i64_;
        IntrinsicNamedTypeSymbol i128_;
        IntrinsicNamedTypeSymbol isize_;
        IntrinsicNamedTypeSymbol u8_;
        IntrinsicNamedTypeSymbol u16_;
        IntrinsicNamedTypeSymbol u32_;
        IntrinsicNamedTypeSymbol u64_;
        IntrinsicNamedTypeSymbol u128_;
        IntrinsicNamedTypeSymbol usize_;
        IntrinsicNamedTypeSymbol f32_;
        IntrinsicNamedTypeSymbol f64_;
        IntrinsicNamedTypeSymbol char_;
        IntrinsicNamedTypeSymbol char16_;
        IntrinsicNamedTypeSymbol rune_;
        IntrinsicNamedTypeSymbol str_;
    };
} // namespace prism
