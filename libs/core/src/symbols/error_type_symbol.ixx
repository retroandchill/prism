/**
 * @file error_type_symbol.ixx
 * @author Francesco Corso
 * @date 8/6/2026
 * @brief
 */

export module prism.core:symbols.error;

import :symbols.named_type_symbol;
import :symbols.namespace_symbol;
import :symbols.function_symbol;

namespace prism
{
    class ErrorTypeSymbol final : public NamedTypeSymbol
    {
      public:
        explicit constexpr ErrorTypeSymbol(const Name name = KnownName::none, const Symbol *containing = nullptr)
            : NamedTypeSymbol{name, containing, SpecialType::none}
        {
        }

        [[nodiscard]] std::span<const SyntaxReference> declaring_syntax_references() const override
        {
            return {};
        }

        [[nodiscard]] constexpr const ImmutableArray<Location> &locations() const override
        {
            static constexpr ImmutableArray<Location> empty{};
            return empty;
        }

        [[nodiscard]] constexpr SymbolSpan<Symbol> members() const override
        {
            return {};
        }

        [[nodiscard]] constexpr SymbolSpan<Symbol> members(Name name) const override
        {
            return {};
        }
    };

    constexpr ErrorTypeSymbol unnamed_error_type;

    class ErrorNamespaceSymbol final : public NamespaceSymbol
    {
      public:
        ErrorNamespaceSymbol(const Name &name, const NamespaceSymbol *containing) : NamespaceSymbol(name, containing)
        {
        }

        [[nodiscard]] constexpr const ImmutableArray<Location> &locations() const override
        {
            static constexpr ImmutableArray<Location> empty{};
            return empty;
        }

        [[nodiscard]] constexpr NamespaceKind namespace_kind() const noexcept override
        {
            return NamespaceKind::compilation;
        }

        [[nodiscard]] constexpr std::span<const SyntaxReference> declaring_syntax_references() const override
        {
            return {};
        }

        [[nodiscard]] constexpr Optional<const Compilation &> containing_compilation() const noexcept override
        {
            return std::nullopt;
        }

        [[nodiscard]] constexpr SymbolSpan<Symbol> members() const override
        {
            return {};
        }

        [[nodiscard]] constexpr SymbolSpan<Symbol> members(Name name) const override
        {
            return {};
        }
    };

    class ErrorFunctionSymbol final : public FunctionSymbol
    {
      public:
        constexpr ErrorFunctionSymbol(const Name name = KnownName::none,
                                      const Symbol *containing = nullptr,
                                      const TypeSymbol &return_type = unnamed_error_type)
            : FunctionSymbol{name, containing}, return_type_{return_type}
        {
        }

        [[nodiscard]] constexpr const ImmutableArray<Location> &locations() const override
        {
            static constexpr ImmutableArray<Location> empty{};
            return empty;
        }

        [[nodiscard]] constexpr std::span<const SyntaxReference> declaring_syntax_references() const override
        {
            return {};
        }

        [[nodiscard]] constexpr const TypeSymbol &return_type() const override
        {
            return return_type_;
        }

        [[nodiscard]] constexpr SymbolSpan<ParameterSymbol> parameters() const noexcept override
        {
            return {};
        }

      private:
        const TypeSymbol &return_type_;
    };

    constexpr ErrorFunctionSymbol unnamed_error_function;
} // namespace prism
