/**
 * @file symbol.ixx
 * @author Francesco Corso
 * @date 7/1/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

export module prism.core.semantic.symbol;

import std;
import prism.core.util;
import prism.core.strings.name;
import prism.core.source.source_file;
import prism.core.ast;

namespace prism
{
    export class Scope;

    export struct VariableSymbol
    {
        Ref<const VariableDeclarationSyntax> declaration;
    };

    export struct ParameterSymbol
    {
        Ref<const ParameterDeclarationSyntax> declaration;
    };

    struct FunctionSymbol
    {
        Ref<const FunctionDeclarationSyntax> declaration;
        std::vector<Ref<const ParameterSymbol>> parameters;
        Ref<const Scope> body_scope;
    };

    export using SymbolData = std::variant<VariableSymbol, ParameterSymbol, FunctionSymbol>;

    export struct Symbol
    {
        Name name;
        SourceRange range;
        Ref<const Scope> declaring_scope;
        SymbolData data;

        template <VariantAlternative<Symbol> Alternative>
        [[nodiscard]] constexpr bool is() const
        {
            return std::holds_alternative<Alternative>(data);
        }

        template <VariantAlternative<Symbol> Alternative, typename Self>
        constexpr decltype(auto) get(this Self &&self)
        {
            return std::get<Alternative>(std::forward<Self>(self).data);
        }

        template <VariantAlternative<Symbol> Alternative>
        constexpr auto try_get() &
        {
            return std::get_if<Alternative>(data);
        }

        template <VariantAlternative<Symbol> Alternative>
        constexpr auto try_get() const &
        {
            return std::get_if<Alternative>(data);
        }

        template <typename Self, VariantVisitorFor<ForwardLikeType<Symbol, Self>> Visitor>
        constexpr decltype(auto) visit(this Self &&self, Visitor &&visitor)
        {
            return std::visit(std::forward<Visitor>(visitor), std::forward<Self>(self).data);
        }

        template <typename Self, typename R, VariantVisitorReturnSame<R, ForwardLikeType<Symbol, Self>> Visitor>
        constexpr decltype(auto) visit(this Self &&self, Visitor &&visitor)
        {
            return std::visit(std::forward<Visitor>(visitor), std::forward<Self>(self).data);
        }
    };

    export PRISM_CORE_API class SymbolSet
    {
      public:
        [[nodiscard]] constexpr std::span<const Ref<const Symbol>> symbols() const noexcept
        {
            return symbols_;
        }

        constexpr void add(const Symbol &symbol)
        {
            symbols_.emplace_back(symbol);
        }

      private:
        std::vector<Ref<const Symbol>> symbols_;
    };

    class PRISM_CORE_API Scope
    {
      public:
        constexpr Scope() = default;
        constexpr explicit Scope(const Scope &parent) : parent_{&parent}
        {
        }

        [[nodiscard]] constexpr const Scope *parent() const noexcept
        {
            return parent_;
        }

        [[nodiscard]] std::span<const Ref<const Symbol>> get(Name name) const noexcept;

        void add(const Symbol &symbol);

        [[nodiscard]] constexpr std::span<const Ref<const Scope>> children() const noexcept
        {
            return children_;
        }

      private:
        const Scope *parent_ = nullptr;
        std::unordered_map<Name, SymbolSet> symbols_;
        std::vector<Ref<const Scope>> children_;
    };
} // namespace prism
