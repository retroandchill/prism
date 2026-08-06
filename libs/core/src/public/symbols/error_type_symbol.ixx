/**
 * @file error_type_symbol.ixx
 * @author Francesco Corso
 * @date 8/6/2026
 * @brief
 */

export module prism.core:symbols.error_type_symbol;

import :symbols.named_type_symbol;

namespace prism
{
    class ErrorTypeSymbol final : public NamedTypeSymbol
    {
      public:
        explicit constexpr ErrorTypeSymbol(const Name name = KnownName::none)
            : NamedTypeSymbol{name, nullptr, SpecialType::none}
        {
        }

        [[nodiscard]] std::span<const SyntaxReference> declaring_syntax_references() const override
        {
            return {};
        }
    };

    constexpr ErrorTypeSymbol unnamed_error_type;
} // namespace prism
