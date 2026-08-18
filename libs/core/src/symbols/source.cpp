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
import :semantic.compilation;
import :declarations.visit;
import :syntax.visit;
import :binder.binding_helpers;
import :binder;
import :binder.lookup_context;
import :symbols.error;
import :semantic.bound.bound_expression;

namespace prism
{
    SourceAssemblySymbol::SourceAssemblySymbol(const Compilation &compilation)
        : AssemblySymbol{compilation.assembly_name()}, declaring_compilation_{compilation}
    {
    }

    const ImmutableArray<Location> &SourceAssemblySymbol::locations() const
    {
        return locations_.get_or_compute(
            [this]
            {
                return CompilationInternal::merged_root_declaration(declaring_compilation_).declarations() |
                       std::views::transform([](const RefCountPtr<const SingleNamespaceDeclaration> &d)
                                             { return d->location(); }) |
                       std::ranges::to<ImmutableArray<Location>>();
            });
    }

    const NamespaceSymbol &SourceAssemblySymbol::global_namespace() const
    {
        return global_namespace_.get_or_compute(
            [this] -> auto &
            {
                return CompilationInternal::get_lifetime(declaring_compilation_)
                    .create<SourceNamespaceSymbol>(
                        CompilationInternal::merged_root_declaration(declaring_compilation_).shared_from_this(),
                        *this,
                        this);
            });
    }

    std::span<const SyntaxReference> SourceAssemblySymbol::declaring_syntax_references() const
    {
        return {};
    }

    Optional<const Compilation &> SourceAssemblySymbol::declaring_compilation() const
    {
        return declaring_compilation_;
    }

    SourceNamespaceSymbol::SourceNamespaceSymbol(RefCountPtr<const MergedNamespaceDeclaration> declaration,
                                                 const AssemblySymbol &assembly,
                                                 const Symbol *containing)
        : NamespaceSymbol{declaration->name(), containing}, containing_assembly_{assembly},
          merged_declaration_{std::move(declaration)}
    {
    }

    Optional<const AssemblySymbol &> SourceNamespaceSymbol::containing_assembly() const noexcept
    {
        return containing_assembly_;
    }

    const ImmutableArray<Location> &SourceNamespaceSymbol::locations() const
    {
        return locations_.get_or_compute([this] { return merged_declaration_->name_locations(); });
    }

    Optional<const Compilation &> SourceNamespaceSymbol::containing_compilation() const noexcept
    {
        return std::nullopt;
    }

    SymbolSpan<Symbol> SourceNamespaceSymbol::members() const
    {
        return members_.get_or_compute([this] { return compute_members(); });
    }

    SymbolSpan<Symbol> SourceNamespaceSymbol::members(const Name name) const
    {
        auto &map = get_name_to_members_map();
        const auto it = map.find(name);
        return it != map.end() ? it->second : ImmutableArray<Ref<const Symbol>>{};
    }

    std::span<const SyntaxReference> SourceNamespaceSymbol::declaring_syntax_references() const
    {
        return syntax_references_.get_or_compute(
            [this]
            {
                return merged_declaration_->declarations() |
                       std::views::transform(
                           [this](const RefCountPtr<const SingleNamespaceDeclaration> &declaration) -> auto &
                           { return declaration->syntax_reference(); }) |
                       std::ranges::to<ImmutableArray>();
            });
    }

    const std::unordered_map<Name, ImmutableArray<Ref<const Symbol>>> &SourceNamespaceSymbol::get_name_to_members_map()
        const
    {
        return name_to_members_map_.get_or_compute([this] { return make_name_to_members_map(); });
    }

    std::unordered_map<Name, ImmutableArray<Ref<const Symbol>>> SourceNamespaceSymbol::make_name_to_members_map() const
    {
        PooledMap<Name, PooledVector<Ref<const Symbol>>> result;
        for (auto &declaration : merged_declaration_->members())
        {
            auto &symbol = build_symbol(*declaration);
            result[symbol.name()].emplace_back(symbol);
        }

        constexpr static auto get_members = [](const RefCountPtr<const SingleNamespaceDeclaration> &decl)
        {
            return visit(decl->syntax_reference().syntax(),
                         Overload{[](const CompilationUnitSyntax &cu) { return cu.members(); },
                                  [](const NamespaceDeclarationSyntax &ns) { return ns.members(); },
                                  [](const SyntaxNode &)
                                  {
                                      return SyntaxList<DeclarationSyntax>{};
                                  }});
        };

        for (auto &syntax : merged_declaration_->declarations() | std::views::transform(get_members) | std::views::join)
        {
            visit(syntax,
                  Overload{[](const NamespaceDeclarationSyntax &)
                           {
                               // We already handled namespaces
                           },
                           [&](const VariableDeclarationSyntax &variable)
                           {
                               auto &symbol = build_symbol(variable);
                               result[symbol.name()].emplace_back(symbol);
                           },
                           [&](const FunctionDeclarationSyntax &function)
                           {
                               auto &symbol = build_symbol(function);
                               result[symbol.name()].emplace_back(symbol);
                           },
                           [](const IncompleteDeclarationSyntax &)
                           {
                               // Incomplete declarations are not symbols
                           }});
        }

        return result |
               std::views::transform(
                   [](const auto &pair)
                   { return std::make_pair(pair.first, pair.second | std::ranges::to<ImmutableArray>()); }) |
               std::ranges::to<std::unordered_map>();
    }

    const Symbol &SourceNamespaceSymbol::build_symbol(const MergedDeclaration &declaration) const
    {
        return visit(declaration,
                     Overload{[this](const MergedNamespaceDeclaration &ns) -> const Symbol &
                              {
                                  return CompilationInternal::get_lifetime(declaring_compilation().value())
                                      .create<SourceNamespaceSymbol>(ns.shared_from_this(), containing_assembly_, this);
                              }});
    }

    const Symbol &SourceNamespaceSymbol::build_symbol(const VariableDeclarationSyntax &declaration) const
    {
        auto name = get_identifier_name(declaration.identifier());
        return CompilationInternal::get_lifetime(declaring_compilation().value())
            .create<SourceGlobalVariableSymbol>(name, this, declaration);
    }

    const Symbol &SourceNamespaceSymbol::build_symbol(const FunctionDeclarationSyntax &declaration) const
    {
        auto name = get_identifier_name(declaration.identifier());
        return CompilationInternal::get_lifetime(declaring_compilation().value())
            .create<SourceFunctionSymbol>(name, this, declaration);
    }

    ImmutableArray<Ref<const Symbol>> SourceNamespaceSymbol::compute_members() const
    {
        auto unsorted =
            get_name_to_members_map() | std::views::values | std::views::join | std::ranges::to<std::vector>();
        std::ranges::sort(unsorted,
                          [this](const Symbol &lhs, const Symbol &rhs)
                          {
                              const auto comparison =
                                  CompilationInternal::compare_source_locations(declaring_compilation().value(),
                                                                                lhs.first_location(),
                                                                                rhs.first_location());
                              return comparison == std::strong_ordering::less;
                          });
        return ImmutableArray{std::from_range, unsorted};
    }

    SourceVariableSymbol::SourceVariableSymbol(const Name name,
                                               const Symbol *containing,
                                               const VariableDeclarationSyntax &syntax)
        : VariableSymbol{name, containing}, syntax_{syntax}, syntax_reference_{syntax}
    {
    }

    const ImmutableArray<Location> &SourceVariableSymbol::locations() const
    {
        return locations_.get_or_compute([this] { return ImmutableArray{syntax_.identifier().location()}; });
    }

    bool SourceVariableSymbol::is_mutable() const noexcept
    {
        return syntax_.mut_keyword().has_value();
    }

    std::span<const SyntaxReference> SourceVariableSymbol::declaring_syntax_references() const
    {
        return {&syntax_reference_, 1};
    }

    const VariableDeclarationSyntax &SourceVariableSymbol::syntax() const noexcept
    {
        return syntax_;
    }

    SourceLocalVariableSymbol::SourceLocalVariableSymbol(const Name name,
                                                         const Symbol *containing,
                                                         const VariableDeclarationSyntax &syntax,
                                                         const Binder &scope_binder,
                                                         const Binder *initializer_binder)
        : SourceVariableSymbol{name, containing, syntax}, scope_binder_{scope_binder},
          initializer_binder_{initializer_binder}
    {
    }

    const TypeSymbol &SourceLocalVariableSymbol::type() const
    {
        return type_.get_or_compute(
            [this] -> const TypeSymbol &
            {
                const LookupContext context{*containing_assembly()};
                if (syntax().type().has_value())
                {
                    return visit(syntax().type()->type(),
                                 Overload{
                                     [&](const NamedTypeSyntax &named) -> auto &
                                     {
                                         const auto symbol = scope_binder_.lookup_from_syntax(named.identifier(),
                                                                                              LookupOptions::type,
                                                                                              context);
                                         return static_cast<const TypeSymbol &>(symbol.symbol());
                                     },
                                     [&](const PredefinedTypeSyntax &predefined) -> const TypeSymbol & {
                                         return declaring_compilation().value().get_special_type(
                                             from_token(predefined.keyword().kind()));
                                     },
                                 });
                }

                if (!syntax().initializer().has_value())
                {
                    return unnamed_error_type;
                }

                auto &initializer = scope_binder_.get_bound_expression(*syntax().initializer());
                return initializer.type();
            });
    }

    SourceGlobalVariableSymbol::SourceGlobalVariableSymbol(const Name name,
                                                           const Symbol *containing,
                                                           const VariableDeclarationSyntax &syntax)
        : SourceVariableSymbol{name, containing, syntax}
    {
    }
    SourceFunctionSymbol::SourceFunctionSymbol(const Name &name,
                                               const Symbol *containing,
                                               const FunctionDeclarationSyntax &syntax)
        : FunctionSymbol(name, containing), syntax_{syntax}, syntax_reference_{syntax}
    {
    }

    const ImmutableArray<Location> &SourceFunctionSymbol::locations() const
    {
        return locations_.get_or_compute([this] { return ImmutableArray{syntax_.identifier().location()}; });
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

    const ImmutableArray<Location> &SourceParameterSymbol::locations() const
    {
        return locations_.get_or_compute([this] { return ImmutableArray{syntax_.name().location()}; });
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
