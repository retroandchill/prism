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
        LookupResult result;
        auto *binder = this;
        if (has_any_flags(options, LookupOptions::callable))
        {
            while (binder != nullptr)
            {
                auto local_result = binder->lookup_local(name, options, context);

                // If any symbol is not a function symbol, then we are going to return that result
                if (local_result.found() &&
                    std::ranges::any_of(local_result.symbols(),
                                        [](const Symbol &s) { return !s.is<FunctionSymbol>(); }))
                {
                    if (!result.found())
                    {
                        result = std::move(local_result);
                    }

                    break;
                }

                result.append(std::move(local_result));
                binder = binder->next_;
            }
        }
        else
        {
            while (binder != nullptr)
            {
                result = binder->lookup_local(name, options, context);
                if (result.found())
                {
                    break;
                }
                binder = binder->next_;
            }
        }

        return result;
    }

    LookupResult Binder::lookup_qualified_name(const Name name,
                                               const MemberContainerSymbol &container,
                                               const LookupOptions options,
                                               const LookupContext &context) const
    {
        SymbolList symbols;

        for (const auto symbol :
             container.members(name) | std::views::filter([&](const Symbol &s) { return visible_from(s, context); }))
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

        return LookupResult{std::move(symbols)};
    }

    SemanticLifetime &Binder::lifetime() const noexcept
    {
        return CompilationInternal::get_lifetime(compilation_);
    }

    bool Binder::visible_from(const Symbol &symbol, const LookupContext &context) const
    {
        switch (symbol.declared_visibility())
        {
            case DeclaredVisibility::not_applicable:
            case DeclaredVisibility::public_:
                return true;
            case DeclaredVisibility::internal:
                return &context.assembly_symbol() == &compilation_.assembly();
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

        // TODO: Add visibility check
        return true;
    }

    LookupResult Binder::lookup_from_simple_name(const SimpleNameSyntax &syntax,
                                                 const LookupOptions options,
                                                 const LookupContext &context) const
    {
        const auto name = get_unqualified_name(syntax);
        return lookup_unqualified_name(name, options, context);
    }

    LookupResult Binder::lookup_from_qualified_name(const QualifiedNameSyntax &syntax,
                                                    LookupOptions options,
                                                    const LookupContext &context) const
    {
        auto lookup_result = lookup_from_syntax(syntax.left(), LookupOptions::namespace_or_type, context);
        if (!lookup_result.found())
            return lookup_result;

        if (!lookup_result.ambiguous())
        {
            const auto symbol = lookup_result.symbol().as<MemberContainerSymbol>();
            DEBUG_ASSERT(symbol.has_value());
            const auto unqualified_name = get_unqualified_name(syntax.right());
            return lookup_qualified_name(unqualified_name, *symbol, options, context);
        }

        auto location = visit(syntax.left(),
                              Overload{[](const SimpleNameSyntax &simple) { return simple.location(); },
                                       [](const QualifiedNameSyntax &qualified)
                                       {
                                           return qualified.right().location();
                                       }});
        context.report_diagnostic(
            Diagnostic{DiagnosticInfo::create<DiagnosticCode::ambiguous_symbol>(lookup_result.symbols()[0]->name()),
                       std::move(location)});

        return LookupResult{};
    }
} // namespace prism
