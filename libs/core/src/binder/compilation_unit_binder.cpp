/**
 * @file compilation_unit_binder.cpp
 * @author Francesco Corso
 * @date 8/18/2026
 * @brief
 */
module prism.core:binder.compilation_unit_binder.impl;

import :binder.compilation_unit_binder;
import :syntax.top_level;
import :symbols.namespace_symbol;
import :semantic.compilation;

namespace prism
{
    CompilationUnitBinder::CompilationUnitBinder(const Binder &next, const CompilationUnitSyntax &syntax)
        : Binder{next}, syntax_{syntax}
    {
    }

    Optional<const SyntaxNode &> CompilationUnitBinder::scope_designator() const
    {
        return syntax_;
    }

    Optional<const Symbol &> CompilationUnitBinder::containing_symbol() const
    {
        return compilation().global_namespace();
    }

    LookupResult CompilationUnitBinder::lookup_local(const Name name,
                                                     const LookupOptions options,
                                                     [[maybe_unused]] const LookupContext &context) const
    {
        auto found = get_using_namespaces() |
                     std::views::transform([&](const NamespaceSymbol &ns)
                                           { return lookup_qualified_name(name, ns, options).symbols(); }) |
                     std::views::join | std::ranges::to<SymbolList>();

        if (!found.empty())
        {
            return make_lookup_result(std::move(found), options);
        }

        return lookup_qualified_name(name, compilation().global_namespace(), options);
    }

    const ImmutableArray<Ref<const NamespaceSymbol>> &CompilationUnitBinder::get_using_namespaces() const
    {
        return using_namespaces_.get_or_compute([this] { return build_using_namespaces(syntax_.usings()); });
    }
} // namespace prism
