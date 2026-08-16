/**
 * @file visit.ixx
 * @author Francesco Corso
 * @date 8/15/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:declarations.visit;

import :type_traits.visitor;
import :declarations.merged_namespace_declaration;

namespace prism
{
    template <typename Functor>
    concept VisitorForDeclaration = ExhaustiveVisitor<Functor, SingleNamespaceDeclaration, MergedNamespaceDeclaration>;

    template <VisitorForDeclaration Functor>
    constexpr decltype(auto) visit(const Declaration &declaration, Functor &&functor)
    {
        if (declaration.is_merged())
        {
            return visit(static_cast<const MergedNamespaceDeclaration &>(declaration), std::forward<Functor>(functor));
        }

        return visit(static_cast<const SingleNamespaceDeclaration &>(declaration), std::forward<Functor>(functor));
    }

    template <typename Functor>
    concept VisitorForSingleDeclaration = ExhaustiveVisitor<Functor, SingleNamespaceDeclaration>;

    template <VisitorForSingleDeclaration Functor>
    constexpr decltype(auto) visit(const SingleDeclaration &declaration, Functor &&functor)
    {
        if (declaration.kind() == DeclarationKind::namespace_)
        {
            return std::invoke(std::forward<Functor>(functor),
                               static_cast<const SingleNamespaceDeclaration &>(declaration));
        }

        // We don't have an implementation for types yet
        throw NotImplementedException{};
    }

    template <typename Functor>
    concept VisitorForMergedDeclaration = ExhaustiveVisitor<Functor, MergedNamespaceDeclaration>;

    template <VisitorForMergedDeclaration Functor>
    constexpr decltype(auto) visit(const MergedDeclaration &declaration, Functor &&functor)
    {
        if (declaration.kind() == DeclarationKind::namespace_)
        {
            return std::invoke(std::forward<Functor>(functor),
                               static_cast<const MergedNamespaceDeclaration &>(declaration));
        }

        // We don't have an implementation for types yet
        throw NotImplementedException{};
    }
} // namespace prism
