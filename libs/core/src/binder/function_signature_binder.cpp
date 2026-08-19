/**
 * @file function_signature_binder.cpp
 * @author Francesco Corso
 * @date 8/18/2026
 * @brief
 */
module prism.core:binder.function_signature_binder.impl;

import :binder.function_signature_binder;
import :syntax.declarations;
import :symbols.function_symbol;
import :symbols.parameter_symbol;

namespace prism
{

    FunctionSignatureBinder::FunctionSignatureBinder(const Binder &next,
                                                     const FunctionSymbol &symbol,
                                                     const FunctionDeclarationSyntax &syntax)
        : Binder{next}, symbol_{symbol}, syntax_{syntax}
    {
    }

    Optional<const SyntaxNode &> FunctionSignatureBinder::scope_designator() const
    {
        return syntax_;
    }

    Optional<const Symbol &> FunctionSignatureBinder::containing_symbol() const
    {
        return symbol_;
    }

    LookupResult FunctionSignatureBinder::lookup_local(Name name,
                                                       LookupOptions options,
                                                       const LookupContext &context) const
    {
        if (has_any_flags(options, LookupOptions::value))
        {
            return make_lookup_result(
                symbol_.parameters() |
                    std::views::filter([name](const Symbol &symbol) { return symbol.name() == name; }) |
                    std::ranges::to<SymbolList>(),
                LookupOptions::value);
        }

        return LookupResult::not_found();
    }
} // namespace prism
