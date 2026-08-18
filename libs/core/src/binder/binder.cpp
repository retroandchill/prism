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

    LookupResult Binder::lookup_nearest(const Name name) const
    {
        LookupResult result;
        for (auto *binder = this; binder != nullptr; binder = binder->next_)
        {
            result = binder->lookup_local(name);
            if (result.found())
                break;
        }

        return result;
    }

    LookupResult Binder::lookup_all_visible(const Name name) const
    {
        LookupResult result;
        for (auto *binder = this; binder != nullptr; binder = binder->next_)
        {
            result.append(binder->lookup_local(name));
        }

        return result;
    }

    SemanticLifetime &Binder::lifetime() const noexcept
    {
        return CompilationInternal::get_lifetime(compilation_);
    }
} // namespace prism
