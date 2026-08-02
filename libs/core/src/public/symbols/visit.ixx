/**
 * @file visit.ixx
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:symbols.visit;

import libassert;
import :type_traits.visitor;
import :symbols.assembly_symbol;
import :symbols.function_symbol;
import :symbols.namespace_symbol;
import :symbols.parameter_symbol;
import :symbols.named_type_symbol;
import :symbols.variable_symbol;

namespace prism
{
    template <typename Functor>
    concept VisitorForSymbol = ExhaustiveVisitor<Functor,
                                                 AssemblySymbol,
                                                 FunctionSymbol,
                                                 NamespaceSymbol,
                                                 ParameterSymbol,
                                                 NamedTypeSymbol,
                                                 VariableSymbol>;

    template <typename Functor, typename R>
    concept VisitorForSymbolReturning = ConvertibleVisitor<Functor,
                                                           R,
                                                           AssemblySymbol,
                                                           FunctionSymbol,
                                                           NamespaceSymbol,
                                                           ParameterSymbol,
                                                           NamedTypeSymbol,
                                                           VariableSymbol>;

    template <VisitorForSymbol Functor>
    constexpr decltype(auto) visit(const Symbol &symbol, Functor &&functor)
    {
        switch (symbol.kind())
        {
            case SymbolKind::assembly:
                return std::invoke(std::forward<Functor>(functor), static_cast<const AssemblySymbol &>(symbol));
            case SymbolKind::namespace_:
                return std::invoke(std::forward<Functor>(functor), static_cast<const NamespaceSymbol &>(symbol));
            case SymbolKind::variable:
                return std::invoke(std::forward<Functor>(functor), static_cast<const VariableSymbol &>(symbol));
            case SymbolKind::function:
                return std::invoke(std::forward<Functor>(functor), static_cast<const FunctionSymbol &>(symbol));
            case SymbolKind::parameter:
                return std::invoke(std::forward<Functor>(functor), static_cast<const ParameterSymbol &>(symbol));
            case SymbolKind::named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const NamedTypeSymbol &>(symbol));
            default:
                UNREACHABLE("Invalid symbol type passed into visit");
        }
    }

    template <typename R, VisitorForSymbolReturning<R> Functor>
    constexpr R visit(const Symbol &symbol, Functor &&functor)
    {
        switch (symbol.kind())
        {
            case SymbolKind::assembly:
                return std::invoke(std::forward<Functor>(functor), static_cast<const AssemblySymbol &>(symbol));
            case SymbolKind::namespace_:
                return std::invoke(std::forward<Functor>(functor), static_cast<const NamespaceSymbol &>(symbol));
            case SymbolKind::variable:
                return std::invoke(std::forward<Functor>(functor), static_cast<const VariableSymbol &>(symbol));
            case SymbolKind::function:
                return std::invoke(std::forward<Functor>(functor), static_cast<const FunctionSymbol &>(symbol));
            case SymbolKind::parameter:
                return std::invoke(std::forward<Functor>(functor), static_cast<const ParameterSymbol &>(symbol));
            case SymbolKind::named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const NamedTypeSymbol &>(symbol));
            default:
                UNREACHABLE("Invalid symbol type passed into visit");
        }
    }

    template <typename Functor>
    concept VisitorForTypeSymbol = ExhaustiveVisitor<Functor, NamedTypeSymbol>;

    template <typename Functor, typename R>
    concept VisitorForTypeSymbolReturning = ConvertibleVisitor<Functor, R, NamedTypeSymbol>;

    template <VisitorForTypeSymbol Functor>
    constexpr decltype(auto) visit(const TypeSymbol &symbol, Functor &&functor)
    {
        switch (symbol.kind())
        {
            case SymbolKind::named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const NamedTypeSymbol &>(symbol));
            default:
                UNREACHABLE("Invalid symbol type passed into visit");
        }
    }

    template <typename R, VisitorForTypeSymbolReturning<R> Functor>
    constexpr R visit(const TypeSymbol &symbol, Functor &&functor)
    {
        switch (symbol.kind())
        {
            case SymbolKind::named_type:
                return std::invoke(std::forward<Functor>(functor), static_cast<const NamedTypeSymbol &>(symbol));
            default:
                UNREACHABLE("Invalid symbol type passed into visit");
        }
    }
} // namespace prism
