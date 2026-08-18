/**
 * @file binder.ixx
 * @author Francesco Corso
 * @date 8/17/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:binder;

import libassert;
import :util.noncopyable;
import :util.optional;
import :util.lazy;
import :util.ref;
import :semantic.lookup_result;

namespace prism
{
    class VariableSymbol;
    class Symbol;
    class SyntaxNode;
    class SemanticLifetime;
    class Compilation;
    class ConversionClassifier;

    using VariablesSpan = std::span<const Ref<const VariableSymbol>>;

    export class Binder : NonCopyable
    {
      protected:
        explicit Binder(const Compilation &compilation);

        explicit Binder(const Binder &next);

        explicit Binder(const Binder &next, const ConversionClassifier &conversion_classifier);

        ~Binder() = default;

      public:
        [[nodiscard]] constexpr const Compilation &compilation() const noexcept
        {
            return compilation_;
        }

        [[nodiscard]] constexpr Optional<const Binder &> next() const noexcept
        {
            return next_;
        }

        [[nodiscard]] constexpr const Binder &next_checked() const noexcept
        {
            DEBUG_ASSERT(next_ != nullptr);
            return *next_;
        }

        [[nodiscard]] const ConversionClassifier &conversion_classifier() const;

        [[nodiscard]] virtual Optional<const Binder &> get_binder(const SyntaxNode &node) const;

        [[nodiscard]] const Binder &get_binder_checked(const SyntaxNode &node) const;

        [[nodiscard]] virtual Optional<const Symbol &> containing_symbol() const;

        [[nodiscard]] virtual Optional<const SyntaxNode &> scope_designator() const;

        [[nodiscard]] virtual VariablesSpan get_declared_local_variables_for_scope(const SyntaxNode &designator) const;

        /**
         * Search the visible scopes and return the set of symbols that match a given name from the first scope to find
         * a non-empty set of results.
         *
         * @param name The name of the symbol to lookup.
         * @return The result of the search
         */
        [[nodiscard]] LookupResult lookup_nearest(Name name) const;

        /**
         * Search all visible scopes and collect all symbols that match that name.
         * The results are returned in order with the nearest results first.
         *
         * @param name The name of the identifier to lookup
         * @return The result of the search
         * @remarks This is generally used for resolving call statements which need all
         *          possible functions to properly resolve overloads.
         */
        [[nodiscard]] LookupResult lookup_all_visible(Name name) const;

      protected:
        [[nodiscard]] SemanticLifetime &lifetime() const noexcept;

        [[nodiscard]] virtual LookupResult lookup_local(Name name) const = 0;

      private:
        const Compilation &compilation_;
        const Binder *next_ = nullptr;
        mutable Lazy<const ConversionClassifier &> conversion_classifier_;
    };
} // namespace prism
