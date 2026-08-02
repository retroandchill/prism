/**
 * @file source.cpp
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
module prism.core:symbols.source.impl;

import :symbols.source;
import :syntax.declarations;
import :syntax.clauses;

namespace prism
{
    const TypeSymbol &SourceVariableSymbol::type() const noexcept
    {
        throw NotImplementedException{};
    }

    bool SourceVariableSymbol::is_mutable() const noexcept
    {
        return syntax_.mut_keyword().has_value();
    }

    const TypeSymbol &SourceFunctionSymbol::returnType() const noexcept
    {
        throw NotImplementedException{};
    }

    const TypeSymbol &SourceParameterSymbol::type() const noexcept
    {
        throw NotImplementedException{};
    }

    bool SourceParameterSymbol::is_mutable() const noexcept
    {
        return syntax_.mut_keyword().has_value();
    }
} // namespace prism
