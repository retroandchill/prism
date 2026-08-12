/**
 * @file source.cpp
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:symbols.source.impl;

import :symbols.source;
import :syntax.declarations;
import :syntax.clauses;

namespace prism
{
    std::span<const SyntaxReference> SourceAssemblySymbol::declaring_syntax_references() const
    {
        return {};
    }

    Optional<const Compilation &> SourceNamespaceSymbol::containing_compilation() const noexcept
    {
        return std::nullopt;
    }

    SourceVariableSymbol::SourceVariableSymbol(const Name &name,
                                               const Symbol *containing,
                                               const VariableDeclarationSyntax &syntax)
        : VariableSymbol(name, containing), syntax_{syntax}, syntax_reference_{syntax}
    {
    }

    const TypeSymbol &SourceVariableSymbol::type() const
    {
        ASSUME(type_ == nullptr);
        return *type_;
    }

    bool SourceVariableSymbol::is_mutable() const noexcept
    {
        return syntax_.mut_keyword().has_value();
    }

    std::span<const SyntaxReference> SourceVariableSymbol::declaring_syntax_references() const
    {
        return {&syntax_reference_, 1};
    }

    SourceFunctionSymbol::SourceFunctionSymbol(const Name &name,
                                               const Symbol *containing,
                                               const FunctionDeclarationSyntax &syntax)
        : FunctionSymbol(name, containing), syntax_{syntax}, syntax_reference_{syntax}
    {
    }

    const TypeSymbol &SourceFunctionSymbol::return_type() const
    {
        ASSUME(return_type_ == nullptr);
        return *return_type_;
    }

    std::span<const SyntaxReference> SourceFunctionSymbol::declaring_syntax_references() const
    {
        return {&syntax_reference_, 1};
    }

    SourceParameterSymbol::SourceParameterSymbol(const Name &name,
                                                 const Symbol *containing,
                                                 const ParameterSyntax &syntax)
        : ParameterSymbol(name, containing), syntax_{syntax}, syntax_reference_{syntax}
    {
    }

    const TypeSymbol &SourceParameterSymbol::type() const
    {
        ASSUME(type_ == nullptr);
        return *type_;
    }

    bool SourceParameterSymbol::is_mutable() const noexcept
    {
        return syntax_.mut_keyword().has_value();
    }

    std::span<const SyntaxReference> SourceParameterSymbol::declaring_syntax_references() const
    {
        return {&syntax_reference_, 1};
    }
} // namespace prism
