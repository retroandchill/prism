/**
 * @file binder.cpp
 * @author Francesco Corso
 * @date 8/17/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:binder.impl;

import :binder;
import :semantic.compilation;
import :semantic.conversion_classifier;
import :syntax.visit;
import :binder.binding_helpers;
import :binder.lookup_context;
import :symbols.visit;

namespace prism
{
    LookupResult make_lookup_result(SymbolList symbols, const LookupOptions options)
    {
        if (symbols.empty())
            return LookupResult::not_found();

        if (has_any_flags(options, LookupOptions::callable))
            return LookupResult::viable(std::move(symbols));

        if (symbols.size() == 1)
            return LookupResult::viable(std::move(symbols));

        return LookupResult::ambiguous(std::move(symbols));
    }

    std::string to_string(LookupOptions options)
    {
        PooledVector<std::string_view> result;
        if (has_any_flags(options, LookupOptions::namespace_))
            result.push_back("namespace");
        if (has_any_flags(options, LookupOptions::type))
            result.push_back("type");
        if (has_any_flags(options, LookupOptions::value))
            result.push_back("value");
        if (has_any_flags(options, LookupOptions::callable))
            result.push_back("callable");

        if (result.empty())
            return "none";

        if (result.size() == 1)
            return std::string{result.front()};

        if (result.size() == 2)
            return std::format("{} or {}", result.front(), result.back());

        std::string output;
        for (auto [i, view] : result | std::views::enumerate)
        {
            if (i > 0)
            {
                if (i == result.size() - 1)
                    output += ", or ";
                else
                    output += ", ";
            }

            output += view;
        }

        return output;
    }

    Binder::Binder(const Compilation &compilation) : compilation_{compilation}
    {
    }

    Binder::Binder(const Binder &next) : compilation_{next.compilation_}, next_{&next}
    {
    }

    Binder::Binder(const Binder &next, const ConversionClassifier &conversion_classifier)
        : compilation_{next.compilation_}, next_{&next}, conversion_classifier_{conversion_classifier}
    {
    }

    const ConversionClassifier &Binder::conversion_classifier() const
    {
        return conversion_classifier_.get_or_compute([this] -> auto &
                                                     { return lifetime().create<ConversionClassifier>(*this); });
    }

    Optional<const Binder &> Binder::get_binder(const SyntaxNode &node) const
    {
        DEBUG_ASSERT(next_ != nullptr);
        return next_->get_binder(node);
    }

    const Binder &Binder::get_binder_checked(const SyntaxNode &node) const
    {
        const auto binder = get_binder(node);
        DEBUG_ASSERT(binder.has_value());
        return *binder;
    }

    Optional<const Symbol &> Binder::containing_symbol() const
    {
        DEBUG_ASSERT(next_ != nullptr);
        return next_->containing_symbol();
    }

    Optional<const SyntaxNode &> Binder::scope_designator() const
    {
        return std::nullopt;
    }

    VariablesSpan Binder::get_declared_local_variables_for_scope(const SyntaxNode &designator) const
    {
        DEBUG_ASSERT(next_ != nullptr);
        return next_->get_declared_local_variables_for_scope(designator);
    }

    const BoundExpression &Binder::get_bound_expression(const ExpressionSyntax &node) const
    {
        throw NotImplementedException{};
    }

    LookupResult Binder::lookup_from_syntax(const NameSyntax &syntax,
                                            const LookupOptions options,
                                            const LookupContext &context) const
    {
        return visit(syntax,
                     Overload{[&](const SimpleNameSyntax &simple)
                              { return lookup_from_simple_name(simple, options, context); },
                              [&](const QualifiedNameSyntax &qualified)
                              {
                                  return lookup_from_qualified_name(qualified, options, context);
                              }});
    }

    LookupResult Binder::lookup_unqualified_name(const Name name,
                                                 const LookupOptions options,
                                                 const LookupContext &context) const
    {

        if (has_any_flags(options, LookupOptions::callable))
        {
            SymbolList symbols;
            auto *binder = this;
            while (binder != nullptr)
            {
                if (auto local_result = binder->lookup_local(name, options, context);
                    local_result.kind() != LookupResultKind::not_found)
                {
                    if (!local_result.viable())
                        return local_result;

                    if (std::ranges::any_of(local_result.symbols(),
                                            [](const Symbol &s) { return !s.is<FunctionSymbol>(); }))
                    {
                        return local_result;
                    }

                    symbols.append_range(local_result.symbols());
                }

                binder = binder->next_;
            }

            return make_lookup_result(std::move(symbols), options);
        }

        auto *binder = this;
        while (binder != nullptr)
        {
            auto result = binder->lookup_local(name, options, context);
            if (result.kind() != LookupResultKind::not_found)
                return result;

            binder = binder->next_;
        }

        return LookupResult::not_found();
    }

    LookupResult Binder::lookup_qualified_name(const Name name,
                                               const MemberContainerSymbol &container,
                                               const LookupOptions options,
                                               const LookupContext &context) const
    {
        SymbolList symbols;

        for (const auto symbol :
             container.members(name) | std::views::filter([&](const Symbol &s) { return visible_from(s); }))
        {
            visit(symbol,
                  Overload{[&](const VariableSymbol &variable)
                           {
                               if (has_any_flags(options, LookupOptions::value))
                               {
                                   symbols.emplace_back(variable);
                               }
                           },
                           [&](const FunctionSymbol &function)
                           {
                               if (has_any_flags(options, LookupOptions::callable))
                               {
                                   symbols.emplace_back(function);
                               }
                           },
                           [&](const NamespaceSymbol &ns)
                           {
                               if (has_any_flags(options, LookupOptions::namespace_))
                               {
                                   symbols.emplace_back(ns);
                               }
                           },
                           [&](const TypeSymbol &type)
                           {
                               if (has_any_flags(options, LookupOptions::type))
                               {
                                   symbols.emplace_back(type);
                               }
                           },
                           [&](const Symbol &)
                           {
                               UNREACHABLE("Invalid member symbol kind");
                           }});
        }

        return make_lookup_result(std::move(symbols), options);
    }

    SemanticLifetime &Binder::lifetime() const noexcept
    {
        return CompilationInternal::get_lifetime(compilation_);
    }

    bool Binder::visible_from(const Symbol &symbol) const
    {
        switch (symbol.declared_visibility())
        {
            case DeclaredVisibility::not_applicable:
            case DeclaredVisibility::public_:
                return true;
            case DeclaredVisibility::internal:
                return symbol.containing_assembly().value_ptr() == &compilation_.assembly();
            case DeclaredVisibility::file:
                {
                    const auto designator = scope_designator();
                    if (!designator.has_value())
                        return false;

                    return std::ranges::all_of(symbol.declaring_syntax_references(),
                                               [&](const SyntaxReference &ref)
                                               { return &ref.tree() == &designator->tree(); });
                }
        }

        UNREACHABLE("Invalid visibility");
    }

    LookupResult Binder::lookup_from_simple_name(const SimpleNameSyntax &syntax,
                                                 const LookupOptions options,
                                                 const LookupContext &context) const
    {
        const auto name = get_unqualified_name(syntax);
        return lookup_unqualified_name(name, options, context);
    }

    LookupResult Binder::lookup_from_qualified_name(const QualifiedNameSyntax &syntax,
                                                    const LookupOptions options,
                                                    const LookupContext &context) const
    {
        auto lookup_result = lookup_from_syntax(syntax.left(), LookupOptions::namespace_or_type, context);
        if (!lookup_result.viable())
            return lookup_result;

        const auto symbol = lookup_result.symbol().as<MemberContainerSymbol>();
        if (!symbol.has_value())
            return LookupResult::wrong_kind(lookup_result.symbols());

        const auto unqualified_name = get_unqualified_name(syntax.right());
        return lookup_qualified_name(unqualified_name, *symbol, options, context);
    }
} // namespace prism
