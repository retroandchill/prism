/**
 * @file binder.ixx
 * @author Francesco Corso
 * @date 8/17/2026
 * @brief
 */
module;

#include <complex.h>
#include <libassert/assert-macros.hpp>

export module prism.core:binder;

import libassert;
import :util.noncopyable;
import :util.optional;
import :util.lazy;
import :util.ref;
import :binder.lookup_result;

namespace prism
{
    class TypeSyntax;
    class ExpressionSyntax;
    class BoundExpression;
    class QualifiedNameSyntax;
    class SimpleNameSyntax;
    class NameSyntax;
    class MemberContainerSymbol;
    class LookupContext;
    class VariableSymbol;
    class Symbol;
    class SyntaxNode;
    class SemanticLifetime;
    class Compilation;
    class ConversionClassifier;

    using VariablesSpan = std::span<const Ref<const VariableSymbol>>;

    enum class LookupOptions : std::uint32_t
    {
        none = 0,
        namespace_ = 1 << 0,
        type = 1 << 1,
        value = 1 << 2,
        callable = 1 << 3,

        all = namespace_ | type | value | callable,
        namespace_or_type = namespace_ | type,
        value_or_callable = value | callable,
    };

    template <>
    constexpr bool is_flag_enum<LookupOptions> = true;

    LookupResult make_lookup_result(SymbolList symbols, LookupOptions options);

    std::string to_string(LookupOptions options);

    class Binder : NonCopyable
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

        [[nodiscard]] const BoundExpression &get_bound_expression(const ExpressionSyntax &node) const;

        [[nodiscard]] LookupResult lookup_from_syntax(const NameSyntax &syntax,
                                                      LookupOptions options,
                                                      const LookupContext &context) const;

        [[nodiscard]] LookupResult lookup_unqualified_name(Name name,
                                                           LookupOptions options,
                                                           const LookupContext &context) const;

        [[nodiscard]] LookupResult lookup_qualified_name(Name name,
                                                         const MemberContainerSymbol &container,
                                                         LookupOptions options,
                                                         const LookupContext &context) const;

      protected:
        [[nodiscard]] SemanticLifetime &lifetime() const noexcept;

        [[nodiscard]] virtual LookupResult lookup_local(Name name,
                                                        LookupOptions options,
                                                        const LookupContext &context) const = 0;

        [[nodiscard]] bool visible_from(const Symbol &symbol) const;

      private:
        [[nodiscard]] LookupResult lookup_from_simple_name(const SimpleNameSyntax &syntax,
                                                           LookupOptions options,
                                                           const LookupContext &context) const;
        [[nodiscard]] LookupResult lookup_from_qualified_name(const QualifiedNameSyntax &syntax,
                                                              LookupOptions options,
                                                              const LookupContext &context) const;

        const Compilation &compilation_;
        const Binder *next_ = nullptr;
        mutable Lazy<const ConversionClassifier &> conversion_classifier_;
    };
} // namespace prism
