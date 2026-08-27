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
import :compilation;
import :declarations.visit;
import :syntax.visit;
import :binder.binding_helpers;
import :binder;
import :binder.lookup_context;
import :symbols.error;
import :semantic.bound.bound_expression;
import :binder.binder_factory;
import :semantic.semantic_model;
import :semantic.semantic_model_state;
import :diagnostics.factories;
import :syntax.syntax_utils;
import :util.deferred;

namespace prism
{
    namespace
    {
        void cache_symbol(const Compilation &compilation, const SyntaxNode &node, const Symbol &symbol)
        {
            auto &semantic_model = compilation.get_semantic_model(node.tree());
            auto &state = SemanticModelInternal::get_state(semantic_model);
            state.cache_symbol(node, symbol);
        }
    } // namespace

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

    bool SourceAssemblySymbol::needs_completion() const noexcept
    {
        return true;
    }

    void SourceAssemblySymbol::force_complete(const Optional<SourceLocation> &location,
                                              const Optional<SymbolPredicate> &filter) const
    {
        while (true)
        {
            const auto incomplete_part = completion_state_.next_incomplete_part();
            switch (incomplete_part)
            {
                case CompletionPart::members:
                    SymbolInternal::force_complete(global_namespace(), location, filter);
                    if (SymbolInternal::is_complete(global_namespace(), CompletionPart::members_completed))
                    {
                        completion_state_.mark_part_complete(CompletionPart::members);
                        break;
                    }

                    DEBUG_ASSERT(!location.has_value() || !filter.has_value(),
                                 "If no location or filter is provided, the global namespace should be complete");
                    return;
                case CompletionPart::none:
                    return;
                default:
                    // Any other values are for other kinds of symbols
                    completion_state_.mark_part_complete(CompletionPart::all & ~CompletionPart::assembly_all);
                    break;
            }

            completion_state_.wait_part_complete(incomplete_part);
        }
    }

    bool SourceAssemblySymbol::is_complete(const CompletionPart part) const noexcept
    {
        return completion_state_.has_complete(part);
    }

    SourceNamespaceSymbol::SourceNamespaceSymbol(RefCountPtr<const MergedNamespaceDeclaration> declaration,
                                                 const AssemblySymbol &assembly,
                                                 const Symbol *containing)
        : NamespaceSymbol{declaration->name(), containing}, containing_assembly_{assembly},
          merged_declaration_{std::move(declaration)}
    {
        // ReSharper disable once CppVirtualFunctionCallInsideCtor (Class is final)
        auto &compilation = declaring_compilation().value();
        for (auto &decl : merged_declaration_->declarations())
        {
            cache_symbol(compilation, decl->syntax_reference().syntax(), *this);
        }
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

    bool SourceNamespaceSymbol::is_defined_in_source_tree(const SyntaxTree &tree,
                                                          Optional<TextSpan> defined_within) const
    {
        if (is_global())
            return true;

        for (auto &declaration : merged_declaration_->declarations())
        {
            auto &ref = declaration->syntax_reference();
            if (&ref.tree() != &tree)
                continue;

            if (!defined_within.has_value())
                return true;

            if (auto &syntax = get_namespace_declaration_syntax(ref);
                syntax.full_span().intersects_with(*defined_within))
                return true;
        }

        return false;
    }

    bool SourceNamespaceSymbol::needs_completion() const noexcept
    {
        return true;
    }

    void SourceNamespaceSymbol::force_complete(const Optional<SourceLocation> &location,
                                               const Optional<SymbolPredicate> &filter) const
    {
        if (filter.has_value() && !(*filter)(*this))
            return;

        const auto wait_complete = [&]
        {
            const auto all_parts = (!location.has_value() && !filter.has_value())
                                       ? CompletionPart::namespace_all
                                       : CompletionPart::namespace_all & ~CompletionPart::members_completed;
            completion_state_.mark_part_complete(all_parts);
        };

        while (true)
        {
            const auto incomplete_part = completion_state_.next_incomplete_part();
            switch (incomplete_part)
            {
                case CompletionPart::members:
                    std::ignore = get_name_to_members_map();
                    break;
                case CompletionPart::members_completed:
                    {
                        auto members = this->members();
                        bool all_completed = true;

                        for (auto member : members)
                        {
                            force_complete_member_conditionally(location, filter, member);
                            all_completed = all_completed && SymbolInternal::is_complete(member, CompletionPart::all);
                        }

                        if (all_completed)
                        {
                            completion_state_.mark_part_complete(CompletionPart::members_completed);
                            break;
                        }

                        wait_complete();
                        return;
                    }
                case CompletionPart::none:
                    return;
                default:
                    completion_state_.mark_part_complete(CompletionPart::all & ~CompletionPart::namespace_all);
                    break;
            }

            completion_state_.wait_part_complete(incomplete_part);
        }

        wait_complete();
    }

    bool SourceNamespaceSymbol::is_complete(const CompletionPart part) const noexcept
    {
        return completion_state_.has_complete(part);
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

        completion_state_.mark_part_complete(CompletionPart::members);
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
        : VariableSymbol{name, containing}, syntax_{syntax}, syntax_reference_{syntax},
          is_mutable_{std::ranges::any_of(syntax.modifiers(),
                                          [](const SyntaxToken &token)
                                          { return token.kind() == SyntaxKind::mutable_keyword; })}
    {
    }

    const ImmutableArray<Location> &SourceVariableSymbol::locations() const
    {
        return locations_.get_or_compute([this] { return ImmutableArray{syntax_.identifier().location()}; });
    }

    const TypeSymbol &SourceVariableSymbol::type() const
    {
        return type_.get_or_compute(
            [this] -> const TypeSymbol &
            {
                DiagnosticBag diagnostics;
                auto &type = compute_type(diagnostics);
                add_declaration_diagnostics(diagnostics);
                completion_state_.mark_part_complete(CompletionPart::type);
                return type;
            });
    }

    bool SourceVariableSymbol::is_mutable() const noexcept
    {
        return is_mutable_;
    }

    bool SourceVariableSymbol::has_initializer() const noexcept
    {
        return syntax_.initializer().has_value();
    }

    std::span<const SyntaxReference> SourceVariableSymbol::declaring_syntax_references() const
    {
        return {&syntax_reference_, 1};
    }

    const Optional<ConstantValue> &SourceVariableSymbol::constant_value() const noexcept
    {
        return constant_value_.get_or_compute(
            [this]
            {
                DiagnosticBag diagnostics;
                auto value = compute_constant_value(diagnostics);
                add_declaration_diagnostics(diagnostics);
                completion_state_.mark_part_complete(CompletionPart::type);
                return value;
            });
    }

    bool SourceVariableSymbol::is_defined_in_source_tree(const SyntaxTree &tree,
                                                         Optional<TextSpan> defined_within) const
    {
        return Symbol::is_defined_in_source_tree(syntax_reference_, tree, defined_within);
    }

    const VariableDeclarationSyntax &SourceVariableSymbol::syntax() const noexcept
    {
        return syntax_;
    }

    bool SourceVariableSymbol::needs_completion() const noexcept
    {
        return true;
    }

    void SourceVariableSymbol::force_complete(const Optional<SourceLocation> &location,
                                              const Optional<SymbolPredicate> &filter) const
    {
        if (filter.has_value() && !(*filter)(*this))
            return;

        while (true)
        {
            const auto incomplete_part = completion_state_.next_incomplete_part();
            switch (incomplete_part)
            {
                case CompletionPart::none:
                    return;
                case CompletionPart::type:
                    std::ignore = type();
                    break;
                case CompletionPart::constant_value:
                    std::ignore = constant_value();
                    break;
                default:
                    completion_state_.mark_part_complete(CompletionPart::all & ~CompletionPart::variable_all);
                    break;
            }

            completion_state_.wait_part_complete(incomplete_part);
        }
    }

    bool SourceVariableSymbol::is_complete(const CompletionPart part) const noexcept
    {
        return completion_state_.has_complete(part);
    }

    SourceLocalVariableSymbol::SourceLocalVariableSymbol(const Name name,
                                                         const Symbol *containing,
                                                         const VariableDeclarationSyntax &syntax,
                                                         const Binder &scope_binder,
                                                         const Binder *initializer_binder)
        : SourceVariableSymbol{name, containing, syntax}, scope_binder_{scope_binder},
          initializer_binder_{initializer_binder}
    {
        // ReSharper disable once CppVirtualFunctionCallInsideCtor
        cache_symbol(declaring_compilation().value(), syntax, *this);
    }

    bool SourceLocalVariableSymbol::is_global() const noexcept
    {
        return false;
    }

    const TypeSymbol &SourceLocalVariableSymbol::compute_type(DiagnosticBag &diagnostics) const
    {
        const LookupContext context{diagnostics};
        if (syntax().type().has_value())
        {
            return scope_binder_.resolve_type(syntax().type()->type(), context);
        }

        if (!syntax().initializer().has_value())
        {
            diagnostics.add(diagnostics::make_expected_type_specifier(syntax().identifier().location()));
            return unnamed_error_type;
        }

        auto &semantic_model = declaring_compilation()->get_semantic_model(syntax().tree());
        auto &initializer =
            SemanticModelInternal::get_bound_initializer(semantic_model, syntax(), *initializer_binder_, context);
        return initializer.type();
    }

    Optional<ConstantValue> SourceLocalVariableSymbol::compute_constant_value(DiagnosticBag &diagnostics) const
    {
        const LookupContext context{diagnostics};
        if (!syntax().initializer().has_value())
        {
            return std::nullopt;
            ;
        }

        auto &semantic_model = declaring_compilation()->get_semantic_model(syntax().tree());
        auto &initializer =
            SemanticModelInternal::get_bound_initializer(semantic_model, syntax(), *initializer_binder_, context);
        return initializer.constant_value();
    }

    SourceGlobalVariableSymbol::SourceGlobalVariableSymbol(const Name name,
                                                           const Symbol *containing,
                                                           const VariableDeclarationSyntax &syntax)
        : SourceVariableSymbol{name, containing, syntax}
    {
        // ReSharper disable once CppVirtualFunctionCallInsideCtor
        cache_symbol(declaring_compilation().value(), syntax, *this);
    }

    bool SourceGlobalVariableSymbol::is_global() const noexcept
    {
        return true;
    }

    const TypeSymbol &SourceGlobalVariableSymbol::compute_type(DiagnosticBag &diagnostics) const
    {
        if (syntax().type().has_value())
        {
            auto &factory = CompilationInternal::get_binder_factory(*declaring_compilation(), syntax().tree());
            auto &binder = factory.get_binder(syntax());
            const LookupContext context{diagnostics};
            return binder.resolve_type(syntax().type()->type(), context);
        }

        diagnostics.add(diagnostics::make_expected_type_specifier(syntax().identifier().location()));
        return unnamed_error_type;
    }

    Optional<ConstantValue> SourceGlobalVariableSymbol::compute_constant_value(DiagnosticBag &diagnostics) const
    {
        if (!syntax().initializer().has_value())
        {
            return std::nullopt;
        }

        const LookupContext context{diagnostics};
        auto &semantic_model = declaring_compilation()->get_semantic_model(syntax().tree());
        auto &initializer = SemanticModelInternal::get_bound_initializer(semantic_model, syntax(), context);
        return initializer.constant_value();
    }

    SourceFunctionSymbol::SourceFunctionSymbol(const Name &name,
                                               const Symbol *containing,
                                               const FunctionDeclarationSyntax &syntax)
        : FunctionSymbol(name, containing), syntax_{syntax}, syntax_reference_{syntax}
    {
        // ReSharper disable once CppVirtualFunctionCallInsideCtor
        cache_symbol(declaring_compilation().value(), syntax, *this);
    }

    const ImmutableArray<Location> &SourceFunctionSymbol::locations() const
    {
        return locations_.get_or_compute([this] { return ImmutableArray{syntax_.identifier().location()}; });
    }

    const TypeSymbol &SourceFunctionSymbol::return_type() const
    {
        return return_type_.get_or_compute(
            [this] -> auto &
            {
                DiagnosticBag diagnostics;
                auto &type = compute_return_type(diagnostics);
                add_declaration_diagnostics(diagnostics);
                return type;
            });
    }

    SymbolSpan<ParameterSymbol> SourceFunctionSymbol::parameters() const noexcept
    {
        return parameters_.get_or_compute([this] { return compute_parameters(); });
    }

    std::span<const SyntaxReference> SourceFunctionSymbol::declaring_syntax_references() const
    {
        return {&syntax_reference_, 1};
    }

    bool SourceFunctionSymbol::needs_completion() const noexcept
    {
        return true;
    }

    void SourceFunctionSymbol::force_complete(const Optional<SourceLocation> &location,
                                              const Optional<SymbolPredicate> &filter) const
    {
        if (filter.has_value() && !(*filter)(*this))
            return;

        const auto wait_for_completion = [this]
        {
            constexpr auto all_parts = CompletionPart::function_all;
            completion_state_.wait_part_complete(all_parts);
        };

        while (true)
        {
            const auto incomplete_part = completion_state_.next_incomplete_part();
            switch (incomplete_part)
            {
                case CompletionPart::type:
                    std::ignore = return_type();
                    break;
                case CompletionPart::parameters:
                    for (auto parameter : parameters())
                    {
                        SymbolInternal::force_complete(parameter, location, std::nullopt);
                    }

                    completion_state_.mark_part_complete(CompletionPart::parameters);
                    break;
                case CompletionPart::start_checks:
                case CompletionPart::finish_checks:
                    lazy_function_checks();
                    break;
                case CompletionPart::none:
                    return;
                default:
                    completion_state_.wait_part_complete(CompletionPart::function_all);
                    break;
            }

            completion_state_.wait_part_complete(CompletionPart::function_all);
        }

        wait_for_completion();
    }

    bool SourceFunctionSymbol::is_complete(const CompletionPart part) const noexcept
    {
        return completion_state_.has_complete(part);
    }

    void SourceFunctionSymbol::lazy_function_checks() const
    {
        if (completion_state_.has_complete(CompletionPart::finish_checks))
        {
            return;
        }

        std::scoped_lock lock{function_checks_mutex_};
        if (completion_state_.mark_part_complete(CompletionPart::start_checks))
        {
            DiagnosticBag diagnostics;
            Deferred deferred([this] noexcept { completion_state_.mark_part_complete(CompletionPart::finish_checks); });

            function_checks(diagnostics);
            add_declaration_diagnostics(diagnostics);
        }
    }

    void SourceFunctionSymbol::function_checks(DiagnosticBag &bag) const
    {
        // TODO: Perform function validation
    }

    const TypeSymbol &SourceFunctionSymbol::compute_return_type(DiagnosticBag &diagnostics) const
    {
        const LookupContext context{diagnostics};
        if (syntax_.return_type().has_value())
        {
            auto &semantic_model = declaring_compilation()->get_semantic_model(syntax_.tree());
            auto &binder = SemanticModelInternal::get_binder(semantic_model, syntax_);
            auto &type = binder.resolve_type(syntax_.return_type()->type(), context);
            completion_state_.mark_part_complete(CompletionPart::type);
            return type;
        }

        // Omitting the return type just results in void
        return declaring_compilation()->get_special_type(SpecialType::void_);
    }

    SymbolSpan<ParameterSymbol> SourceFunctionSymbol::compute_parameters() const
    {
        auto source = syntax_.parameters().parameters();
        if (source.empty())
            return {};

        auto &lifetime = CompilationInternal::get_lifetime(declaring_compilation().value());
        PooledVector<Ref<const ParameterSymbol>> parameters;
        parameters.reserve(source.size());
        for (auto &syntax : source)
        {
            auto name = get_identifier_name(syntax.name());
            parameters.emplace_back(lifetime.create<SourceParameterSymbol>(name, this, syntax));
        }

        completion_state_.mark_part_complete(CompletionPart::parameters);
        return lifetime.copy_refs(parameters);
    }

    SourceParameterSymbol::SourceParameterSymbol(const Name &name,
                                                 const Symbol *containing,
                                                 const ParameterSyntax &syntax)
        : ParameterSymbol(name, containing), syntax_{syntax}, syntax_reference_{syntax}
    {
        // ReSharper disable once CppVirtualFunctionCallInsideCtor
        cache_symbol(declaring_compilation().value(), syntax, *this);
    }

    const ImmutableArray<Location> &SourceParameterSymbol::locations() const
    {
        return locations_.get_or_compute([this] { return ImmutableArray{syntax_.name().location()}; });
    }

    const TypeSymbol &SourceParameterSymbol::type() const
    {
        return type_.get_or_compute(
            [this] -> auto &
            {
                DiagnosticBag diagnostics;
                auto &type = compute_type(diagnostics);
                add_declaration_diagnostics(diagnostics);
                completion_state_.mark_part_complete(CompletionPart::type);
                return type;
            });
    }

    bool SourceParameterSymbol::is_mutable() const noexcept
    {
        return syntax_.mutable_keyword().has_value();
    }

    std::span<const SyntaxReference> SourceParameterSymbol::declaring_syntax_references() const
    {
        return {&syntax_reference_, 1};
    }

    const TypeSymbol &SourceParameterSymbol::compute_type(DiagnosticBag &diagnostics) const
    {
        auto &factory = CompilationInternal::get_binder_factory(*declaring_compilation(), syntax_.tree());
        auto &binder = factory.get_binder(syntax_);
        const LookupContext context{diagnostics};

        // TODO: When we add support for lambdas we need to ensure we can infer a type from the context
        return binder.resolve_type(syntax_.type_specifier()->type(), context);
    }

    bool SourceParameterSymbol::needs_completion() const noexcept
    {
        return true;
    }

    void SourceParameterSymbol::force_complete(const Optional<SourceLocation> &,
                                               const Optional<SymbolPredicate> &) const
    {
        while (true)
        {
            const auto incomplete_part = completion_state_.next_incomplete_part();
            switch (incomplete_part)
            {
                case CompletionPart::type:
                    std::ignore = type();
                    break;
                case CompletionPart::none:
                    return;
                default:
                    completion_state_.mark_part_complete(CompletionPart::all & ~CompletionPart::parameter_all);
                    break;
            }

            completion_state_.wait_part_complete(incomplete_part);
        }
    }

    bool SourceParameterSymbol::is_complete(const CompletionPart part) const noexcept
    {
        return completion_state_.has_complete(part);
    }
} // namespace prism
