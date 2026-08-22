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
import :binder.lookup_result;
import :semantic.constant_value;
import :semantic.operations;
import :semantic.bound.bound_node;

namespace prism
{
    class IdentifierExpressionSyntax;
    class FunctionSymbol;
    class Conversion;
    struct FloatLiteralData;
    struct IntegerLiteralData;
    class LiteralExpressionSyntax;
    class InvocationExpressionSyntax;
    class TernaryExpressionSyntax;
    class PostfixExpressionSyntax;
    class PrefixExpressionSyntax;
    class AssignmentExpressionSyntax;
    class BinaryExpressionSyntax;
    class StatementSyntax;
    class ExpressionStatementSyntax;
    class BlockSyntax;
    class ReturnStatementSyntax;
    class VariableDeclarationStatementSyntax;
    class UsingDirectiveSyntax;
    class BoundStatement;
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

    [[nodiscard]] LookupResult make_lookup_result(SymbolList symbols, LookupOptions options);

    [[nodiscard]] std::string to_string(LookupOptions options);

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

        [[nodiscard]] const BoundStatement &bind_statement(const StatementSyntax &syntax,
                                                           const TypeSymbol &return_type,
                                                           const LookupContext &context) const;

        [[nodiscard]] inline const BoundExpression &bind_expression(const ExpressionSyntax &syntax,
                                                                    const LookupContext &context) const
        {
            return bind_expression(syntax, nullptr, context);
        }

        [[nodiscard]] inline const BoundExpression &bind_expression(const ExpressionSyntax &node,
                                                                    const TypeSymbol &target_type,
                                                                    const LookupContext &context) const
        {
            return bind_expression(node, &target_type, context);
        }

      private:
        [[nodiscard]] const BoundExpression &bind_expression(const ExpressionSyntax &syntax,
                                                             const TypeSymbol *target_type,
                                                             const LookupContext &context) const;

      public:
        [[nodiscard]] const TypeSymbol &resolve_type(const TypeSyntax &syntax, const LookupContext &context) const;

        [[nodiscard]] LookupResult lookup_from_syntax(const NameSyntax &syntax,
                                                      LookupOptions options,
                                                      const LookupContext &context) const;

        [[nodiscard]] LookupResult lookup_unqualified_name(Name name,
                                                           LookupOptions options,
                                                           const LookupContext &context) const;

        [[nodiscard]] LookupResult lookup_qualified_name(Name name,
                                                         const MemberContainerSymbol &container,
                                                         LookupOptions options) const;

      protected:
        [[nodiscard]] SemanticLifetime &lifetime() const noexcept;

        [[nodiscard]] virtual LookupResult lookup_local(Name name,
                                                        LookupOptions options,
                                                        const LookupContext &context) const = 0;

        [[nodiscard]] ImmutableArray<Ref<const NamespaceSymbol>> build_using_namespaces(
            SyntaxList<UsingDirectiveSyntax> usings) const;

        [[nodiscard]] bool visible_from(const Symbol &symbol) const;

      private:
        [[nodiscard]] LookupResult lookup_from_simple_name(const SimpleNameSyntax &syntax,
                                                           LookupOptions options,
                                                           const LookupContext &context) const;
        [[nodiscard]] LookupResult lookup_from_qualified_name(const QualifiedNameSyntax &syntax,
                                                              LookupOptions options,
                                                              const LookupContext &context) const;

        [[nodiscard]] const NamespaceSymbol &resolve_using_namespace(const NameSyntax &syntax) const;

        [[nodiscard]] const TypeSymbol &require_type(const LookupResult &result,
                                                     const NameSyntax &syntax,
                                                     const LookupContext &context) const;

        [[nodiscard]] const BoundStatement &bind_block(const BlockSyntax &syntax,
                                                       const TypeSymbol &return_type,
                                                       const LookupContext &context) const;
        [[nodiscard]] const BoundStatement &bind_variable_declaration_statement(
            const VariableDeclarationStatementSyntax &syntax,
            const LookupContext &context) const;
        [[nodiscard]] const BoundStatement &bind_expression_statement(const ExpressionStatementSyntax &syntax,
                                                                      const LookupContext &context) const;
        [[nodiscard]] const BoundStatement &bind_return_statement(const ReturnStatementSyntax &syntax,
                                                                  const TypeSymbol &return_type,
                                                                  const LookupContext &context) const;

        [[nodiscard]] const BoundExpression &bind_literal_expression(const LiteralExpressionSyntax &syntax,
                                                                     const TypeSymbol *return_type,
                                                                     const LookupContext &context) const;
        [[nodiscard]] const BoundExpression &bind_identifier_expression(const IdentifierExpressionSyntax &syntax,
                                                                        const LookupContext &context) const;
        [[nodiscard]] const BoundExpression &bind_binary_expression(const BinaryExpressionSyntax &syntax,
                                                                    const LookupContext &context) const;
        [[nodiscard]] const BoundExpression &bind_assignment_expression(const AssignmentExpressionSyntax &syntax,
                                                                        const LookupContext &context) const;
        [[nodiscard]] const BoundExpression &bind_prefix_expression(const PrefixExpressionSyntax &syntax,
                                                                    const TypeSymbol *return_type,
                                                                    const LookupContext &context) const;
        [[nodiscard]] const BoundExpression &bind_postfix_expression(const PostfixExpressionSyntax &syntax,
                                                                     const LookupContext &context) const;
        [[nodiscard]] const BoundExpression &bind_ternary_expression(const TernaryExpressionSyntax &syntax,
                                                                     const TypeSymbol *target_type,
                                                                     const LookupContext &context) const;
        [[nodiscard]] const BoundExpression &bind_invocation_expression(const InvocationExpressionSyntax &syntax,
                                                                        const LookupContext &context) const;

        [[nodiscard]] const BoundExpression &add_conversion_if_necessary(const BoundExpression &expression,
                                                                         const TypeSymbol &type,
                                                                         const LookupContext &context) const;

        [[nodiscard]] const BoundExpression &add_conversion_if_necessary(const BoundExpression &expression,
                                                                         const TypeSymbol &type,
                                                                         const Conversion &conversion,
                                                                         const LookupContext &context) const;

        [[nodiscard]] ConstantValue evaluate_constant_expression(const SyntaxToken &token,
                                                                 const TypeSymbol *return_type,
                                                                 const LookupContext &context) const;
        [[nodiscard]] ConstantValue evaluate_numeric_expression(const IntegerLiteralData &data,
                                                                const TypeSymbol *return_type,
                                                                const Location &location,
                                                                const LookupContext &context,
                                                                bool is_negative = false) const;
        [[nodiscard]] static ConstantValue evaluate_numeric_expression(const FloatLiteralData &data,
                                                                       const TypeSymbol *return_type,
                                                                       const Location &location,
                                                                       const LookupContext &context,
                                                                       bool is_negative = false);

        [[nodiscard]] const BoundExpression &create_unary_operation(const ExpressionSyntax &syntax,
                                                                    UnaryOperation operation,
                                                                    Ref<const BoundExpression> operand,
                                                                    const LookupContext &context) const;

        [[nodiscard]] static bool is_assignment_valid(const TypeSymbol &type, const AssignmentOperation operation);

        [[nodiscard]] const FunctionSymbol &resolve_overload(const LookupResult &result,
                                                             std::span<Ref<const BoundExpression>> arguments,
                                                             const Location &location,
                                                             const LookupContext &context) const;

        [[nodiscard]] bool try_match_overload(const FunctionSymbol &overload,
                                              std::span<Ref<const BoundExpression>> arguments,
                                              const LookupContext &context) const;

        const Compilation &compilation_;
        const Binder *next_ = nullptr;
        mutable Lazy<const ConversionClassifier &> conversion_classifier_;
    };
} // namespace prism
