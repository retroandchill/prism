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
import :semantic.bound.bound_statement;
import :semantic.bound.bound_expression;
import :symbols.error;

namespace prism
{
    namespace
    {

        [[nodiscard]] SpecialType to_special_type(const SyntaxKind kind) noexcept
        {
            switch (kind)
            {
                case SyntaxKind::void_keyword:
                    return SpecialType::void_;
                case SyntaxKind::bool_keyword:
                    return SpecialType::bool_;
                case SyntaxKind::i8_keyword:
                    return SpecialType::i8;
                case SyntaxKind::i16_keyword:
                    return SpecialType::i16;
                case SyntaxKind::i32_keyword:
                    return SpecialType::i32;
                case SyntaxKind::i64_keyword:
                    return SpecialType::i64;
                case SyntaxKind::i128_keyword:
                    return SpecialType::i128;
                case SyntaxKind::u8_keyword:
                    return SpecialType::u8;
                case SyntaxKind::u16_keyword:
                    return SpecialType::u16;
                case SyntaxKind::u32_keyword:
                    return SpecialType::u32;
                case SyntaxKind::u64_keyword:
                    return SpecialType::u64;
                case SyntaxKind::u128_keyword:
                    return SpecialType::u128;
                case SyntaxKind::isize_keyword:
                    return SpecialType::isize;
                case SyntaxKind::usize_keyword:
                    return SpecialType::usize;
                case SyntaxKind::f32_keyword:
                    return SpecialType::f32;
                case SyntaxKind::f64_keyword:
                    return SpecialType::f64;
                case SyntaxKind::char_keyword:
                    return SpecialType::char_;
                case SyntaxKind::char16_keyword:
                    return SpecialType::char16;
                case SyntaxKind::rune_keyword:
                    return SpecialType::rune;
                case SyntaxKind::str_keyword:
                    return SpecialType::str;
                default:
                    UNREACHABLE("unknown special type");
            }
        }

        [[nodiscard]] UnaryOperation to_prefix_operation(const SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind::plus_token:
                    return UnaryOperation::identity;
                case SyntaxKind::minus_token:
                    return UnaryOperation::negation;
                case SyntaxKind::bang_token:
                    return UnaryOperation::logical_not;
                case SyntaxKind::tilde_token:
                    return UnaryOperation::bitwise_not;
                case SyntaxKind::plus_plus_token:
                    return UnaryOperation::pre_increment;
                case SyntaxKind::minus_minus_token:
                    return UnaryOperation::pre_decrement;
                default:
                    throw std::invalid_argument{"invalid unary operation"};
            }
        }

        [[nodiscard]] UnaryOperation to_postfix_operation(const SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind::plus_plus_token:
                    return UnaryOperation::post_increment;
                case SyntaxKind::minus_minus_token:
                    return UnaryOperation::post_decrement;
                default:
                    throw std::invalid_argument{"invalid unary operation"};
            }
        }

        [[nodiscard]] BinaryOperation to_binary_operation(const SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind::plus_token:
                    return BinaryOperation::addition;
                case SyntaxKind::minus_token:
                    return BinaryOperation::subtraction;
                case SyntaxKind::star_token:
                    return BinaryOperation::multiplication;
                case SyntaxKind::slash_token:
                    return BinaryOperation::division;
                case SyntaxKind::percent_token:
                    return BinaryOperation::modulo;
                case SyntaxKind::amp_token:
                    return BinaryOperation::bitwise_and;
                case SyntaxKind::pipe_token:
                    return BinaryOperation::bitwise_or;
                case SyntaxKind::caret_token:
                    return BinaryOperation::bitwise_xor;
                case SyntaxKind::amp_amp_token:
                    return BinaryOperation::logical_and;
                case SyntaxKind::pipe_pipe_token:
                    return BinaryOperation::logical_or;
                case SyntaxKind::equal_equal_token:
                    return BinaryOperation::equals;
                case SyntaxKind::bang_equal_token:
                    return BinaryOperation::not_equals;
                case SyntaxKind::less_token:
                    return BinaryOperation::less_than;
                case SyntaxKind::less_equal_token:
                    return BinaryOperation::less_than_or_equals;
                case SyntaxKind::greater_token:
                    return BinaryOperation::greater_than;
                case SyntaxKind::greater_equal_token:
                    return BinaryOperation::greater_than_or_equals;
                case SyntaxKind::spaceship_token:
                    return BinaryOperation::three_way_comparison;
                case SyntaxKind::less_less_token:
                    return BinaryOperation::shift_left;
                case SyntaxKind::greater_greater_token:
                    return BinaryOperation::shift_right;
                case SyntaxKind::greater_greater_greater_token:
                    return BinaryOperation::unsigned_shift_right;
                case SyntaxKind::question_question_token:
                    return BinaryOperation::null_coalescing;
                default:
                    throw std::invalid_argument{"invalid binary operation"};
            }
        }

        [[nodiscard]] bool can_be_signed(const IntegerSuffix suffix)
        {
            return suffix != IntegerSuffix::u8 && suffix != IntegerSuffix::u16 && suffix != IntegerSuffix::u32 &&
                   suffix != IntegerSuffix::u64 && suffix != IntegerSuffix::u128 && suffix != IntegerSuffix::uz;
        }

        void throw_if_negative(const bool is_negative)
        {
            if (is_negative)
                throw InvalidStateException{"invalid unary operation"};
        }

        [[nodiscard]] BigInteger maybe_negate(BigInteger value, bool is_negative)
        {
            if (is_negative)
                return -value;

            return value;
        }

        [[nodiscard]] std::string get_type_names(BoundSpan<BoundExpression> args)
        {
            using namespace std::string_view_literals;
            return args |
                   std::views::transform([](const BoundExpression &expression)
                                         { return expression.type().to_display_string(); }) |
                   std::views::join_with(", "sv) | std::ranges::to<std::string>();
        };
    } // namespace

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
        for (const auto [i, view] : result | std::views::enumerate)
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

    const BoundStatement &Binder::bind_statement(const StatementSyntax &syntax,
                                                 const TypeSymbol &return_type,
                                                 const LookupContext &context) const
    {
        return visit(syntax,
                     Overload{[&](const BlockSyntax &block) -> const BoundStatement &
                              { return bind_block(block, return_type, context); },
                              [&](const ExpressionStatementSyntax &expression) -> const BoundStatement &
                              { return bind_expression_statement(expression, context); },
                              [&](const ReturnStatementSyntax &statement) -> const BoundStatement &
                              { return bind_return_statement(statement, return_type, context); },
                              [&](const VariableDeclarationStatementSyntax &variable) -> const BoundStatement &
                              { return bind_variable_declaration_statement(variable, context); },
                              [&](const EmptyStatementSyntax &) -> const BoundStatement &
                              {
                                  UNREACHABLE("We should guard against entering into this context");
                              }});
    }

    const BoundExpression &Binder::bind_expression(const ExpressionSyntax &syntax,
                                                   const TypeSymbol *target_type,
                                                   const LookupContext &context) const
    {
        auto &bound = visit(syntax,
                            Overload{[&](const LiteralExpressionSyntax &e) -> const BoundExpression &
                                     { return bind_literal_expression(e, target_type, context); },
                                     [&](const IdentifierExpressionSyntax &e) -> const BoundExpression &
                                     { return bind_identifier_expression(e, context); },
                                     [&](const ParenthesizedExpressionSyntax &e) -> const BoundExpression &
                                     { return bind_expression(e.expression(), context); },
                                     [&](const BinaryExpressionSyntax &e) -> const BoundExpression &
                                     { return bind_binary_expression(e, context); },
                                     [&](const AssignmentExpressionSyntax &e) -> const BoundExpression &
                                     { return bind_assignment_expression(e, context); },
                                     [&](const PrefixExpressionSyntax &e) -> const BoundExpression &
                                     { return bind_prefix_expression(e, target_type, context); },
                                     [&](const PostfixExpressionSyntax &e) -> const BoundExpression &
                                     { return bind_postfix_expression(e, context); },
                                     [&](const TernaryExpressionSyntax &e) -> const BoundExpression &
                                     { return bind_ternary_expression(e, target_type, context); },
                                     [&](const InvocationExpressionSyntax &e) -> const BoundExpression &
                                     {
                                         return bind_invocation_expression(e, context);
                                     }});

        if (target_type != nullptr)
        {
            return add_conversion_if_necessary(bound, *target_type, context);
        }

        return bound;
    }

    const TypeSymbol &Binder::resolve_type(const TypeSyntax &syntax, const LookupContext &context) const
    {
        return visit(syntax,
                     Overload{
                         [&](const NamedTypeSyntax &named) -> auto &
                         {
                             const auto result = lookup_from_syntax(named.identifier(), LookupOptions::type, context);
                             return require_type(result, named.identifier(), context);
                         },
                         [&](const PredefinedTypeSyntax &predefined) -> const TypeSymbol &
                         { return compilation_.get_special_type(to_special_type(predefined.keyword().kind())); },
                     });
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
                                               const LookupOptions options) const
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

    ImmutableArray<Ref<const NamespaceSymbol>> Binder::build_using_namespaces(
        SyntaxList<UsingDirectiveSyntax> usings) const
    {
        PooledVector<Ref<const NamespaceSymbol>> result;
        PooledSet<const NamespaceSymbol *> seen;
        result.reserve(usings.size());
        seen.reserve(result.capacity());

        for (auto &directive : usings)
        {
            auto &resolved = resolve_using_namespace(directive.name());
            if (auto [it, inserted] = seen.insert(&resolved); inserted)
                continue;

            result.push_back(resolved);
        }

        return result | std::ranges::to<ImmutableArray>();
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
        return lookup_qualified_name(unqualified_name, *symbol, options);
    }

    const NamespaceSymbol &Binder::resolve_using_namespace(const NameSyntax &syntax) const
    {
        DiagnosticBag diagnostics;
        const auto names = collect_names(syntax);
        auto current = Ref{compilation().global_namespace()};
        bool is_error = false;
        for (auto simple : names)
        {
            const auto name = get_unqualified_name(simple);
            if (auto result = lookup_qualified_name(name, current, LookupOptions::namespace_); result.viable())
            {
                current = result.symbol().as_checked<NamespaceSymbol>();
            }
            else
            {
                current = compilation().create_error_namespace_symbol(current, name);
                if (!is_error)
                {
                    diagnostics.add(Diagnostic{DiagnosticInfo::create<DiagnosticCode::unresolved_symbol>(name),
                                               simple->location()});
                    is_error = true;
                }
            }
        }

        if (is_error)
        {
            CompilationInternal::get_declaration_diagnostics(compilation()).add_range(diagnostics);
        }

        return current;
    }

    const TypeSymbol &Binder::require_type(const LookupResult &result,
                                           const NameSyntax &syntax,
                                           const LookupContext &context) const
    {
        if (result.viable())
        {
            if (const auto type = result.symbol().as<TypeSymbol>(); type.has_value())
                return *type;
        }

        diagnose_lookup_failure(result, syntax, LookupOptions::type, context);
        auto names = collect_names(syntax);
        return create_error_type_symbol(containing_symbol(), compilation_, names);
    }

    const BoundBlock &Binder::bind_block(const BlockSyntax &syntax,
                                         const TypeSymbol &return_type,
                                         const LookupContext &context) const
    {
        PooledVector<Ref<const BoundStatement>> statements;
        for (auto &statement : syntax.statements())
        {
            if (statement.is<EmptyStatementSyntax>())
                continue;

            statements.emplace_back(bind_statement(statement, return_type, context));
        }

        auto interned = lifetime().copy_refs(statements);
        return lifetime().create<BoundBlock>(syntax, interned);
    }

    const BoundVariableDeclaration &Binder::bind_variable_declaration_statement(
        const VariableDeclarationStatementSyntax &syntax,
        const LookupContext &context) const
    {
        auto &semantic_model = compilation_.get_semantic_model(syntax.tree());
        auto &declaration = syntax.declaration();
        auto &variable = semantic_model.get_declared_symbol(declaration).value();

        const auto initializer = declaration.initializer().transform(
            [&](const InitializerSyntax &i) -> auto &
            {
                return declaration.type().has_value()
                           ? bind_expression(i.value(), variable.type(), context)
                           : SemanticModelInternal::get_bound_expression(semantic_model, i.value(), *this, context);
            });

        return lifetime().create<BoundVariableDeclaration>(syntax, variable, initializer.value_ptr());
    }

    const BoundExpressionStatement &Binder::bind_expression_statement(const ExpressionStatementSyntax &syntax,
                                                                      const LookupContext &context) const
    {
        auto &expression = bind_expression(syntax.expression(), context);
        return lifetime().create<BoundExpressionStatement>(syntax, expression);
    }

    const BoundReturnStatement &Binder::bind_return_statement(const ReturnStatementSyntax &syntax,
                                                              const TypeSymbol &return_type,
                                                              const LookupContext &context) const
    {
        auto *expression = syntax.expression()
                               .transform([&](const ExpressionSyntax &e) -> auto &
                                          { return bind_expression(e, return_type, context); })
                               .value_ptr();

        return lifetime().create<BoundReturnStatement>(syntax, expression);
    }

    const BoundLiteral &Binder::bind_literal_expression(const LiteralExpressionSyntax &syntax,
                                                        const TypeSymbol *return_type,
                                                        const LookupContext &context) const
    {
        const auto token = syntax.value();
        auto value = evaluate_constant_expression(token, return_type, context);
        auto &type = compilation().get_special_type(value.special_type());
        return lifetime().create<BoundLiteral>(syntax, value, type);
    }

    const BoundExpression &Binder::bind_identifier_expression(const IdentifierExpressionSyntax &syntax,
                                                              const LookupContext &context) const
    {
        const auto result = lookup_from_syntax(syntax.value(), LookupOptions::value, context);
        if (!result.viable())
            return lifetime().create<BoundBadExpression>(syntax, unnamed_error_type);

        auto &symbol = result.symbol();
        return visit(
            symbol,
            Overload{
                [&](const VariableSymbol &s) -> const BoundExpression &
                { return lifetime().create<BoundVariableAccess>(syntax, s); },
                [&](const ParameterSymbol &s) -> const BoundExpression &
                { return lifetime().create<BoundParameterAccess>(syntax, s); },
                [](const Symbol &) -> const BoundExpression &
                {
                    throw InvalidStateException{
                        "We must have added a symbol type that can hold a value that we haven't accounted for yet."};
                }});

        diagnose_lookup_failure(result, syntax.value(), LookupOptions::value, context);
    }

    const BoundBinaryExpression &Binder::bind_binary_expression(const BinaryExpressionSyntax &syntax,
                                                                const LookupContext &context) const
    {
        Ref left = bind_expression(syntax.left(), context);
        Ref right = bind_expression(syntax.right(), context);
        const auto operation = to_binary_operation(syntax.op().kind());
        auto conversion = conversion_classifier().classify_binary_operand_type(operation, left->type(), right->type());
        if (conversion.has_value())
        {
            left = add_conversion_if_necessary(left, conversion->type, conversion->left_conversion, context);
            right = add_conversion_if_necessary(right, conversion->type, conversion->right_conversion, context);
        }
        else
        {
            context.report_diagnostic(Diagnostic{
                DiagnosticInfo::create<DiagnosticCode::binary_operator_undefined>(left->type().to_display_string(),
                                                                                  right->type().to_display_string()),
                syntax.location()});
        }

        return lifetime().create<BoundBinaryExpression>(
            syntax,
            left,
            right,
            operation,
            conversion
                .transform([](const BinaryOperandConversion &c) -> const TypeSymbol & { return c.type; })
                .value_or_ref(unnamed_error_type));
    }

    const BoundAssignmentExpression &Binder::bind_assignment_expression(const AssignmentExpressionSyntax &syntax,
                                                                        const LookupContext &context) const
    {
        throw NotImplementedException{};
    }

    const BoundExpression &Binder::bind_prefix_expression(const PrefixExpressionSyntax &syntax,
                                                          const TypeSymbol *return_type,
                                                          const LookupContext &context) const
    {
        const auto op = to_prefix_operation(syntax.op().kind());

        // Negation has a special treatment where it collapses the value into a literal provided the value is a signed
        // integer or floating-point value. We use a special path here because literals are allowed to be in a slightly
        // different range for negative integers.
        if (op == UnaryOperation::negation)
        {
            if (const auto literal = syntax.operand().as<LiteralExpressionSyntax>(); literal.has_value())
            {
                if (const auto int_data = literal->value().try_get_value<IntegerLiteralData>();
                    int_data.has_value() && can_be_signed(int_data->suffix))
                {
                    auto negated =
                        evaluate_numeric_expression(*int_data, return_type, syntax.location(), context, true);
                    return lifetime().create<BoundLiteral>(syntax,
                                                           negated,
                                                           compilation().get_special_type(negated.special_type()));
                }

                if (const auto float_data = literal->value().try_get_value<FloatLiteralData>(); float_data.has_value())
                {
                    auto negated =
                        evaluate_numeric_expression(*float_data, return_type, syntax.location(), context, true);
                    return lifetime().create<BoundLiteral>(syntax,
                                                           negated,
                                                           compilation().get_special_type(negated.special_type()));
                }
            }
        }

        auto &operand = bind_expression(syntax.operand(), return_type, context);
        return create_unary_operation(syntax, op, operand, context);
    }

    const BoundUnaryExpression &Binder::bind_postfix_expression(const PostfixExpressionSyntax &syntax,
                                                                const LookupContext &context) const
    {
        auto &operand = bind_expression(syntax.operand(), context);
        const auto op = to_postfix_operation(syntax.op().kind());

        return create_unary_operation(syntax, op, operand, context);
    }

    const BoundConditionalExpression &Binder::bind_ternary_expression(const TernaryExpressionSyntax &syntax,
                                                                      const TypeSymbol *target_type,
                                                                      const LookupContext &context) const
    {
        auto &condition = add_conversion_if_necessary(bind_expression(syntax.condition(), context),
                                                      compilation_.get_special_type(SpecialType::bool_),
                                                      context);

        Ref when_true = bind_expression(syntax.when_true(), context);
        Ref when_false = bind_expression(syntax.when_false(), context);

        if (target_type != nullptr)
        {
            when_true = add_conversion_if_necessary(when_true, *target_type, context);
            when_false = add_conversion_if_necessary(when_false, *target_type, context);
        }
        else
        {
            when_false = add_conversion_if_necessary(when_false, when_true->type(), context);
            target_type = &when_true->type();
        }

        return lifetime().create<BoundConditionalExpression>(syntax, condition, when_true, when_false, *target_type);
    }

    const BoundInvocationExpression &Binder::bind_invocation_expression(const InvocationExpressionSyntax &syntax,
                                                                        const LookupContext &context) const
    {
        PooledVector<Ref<const BoundExpression>> arguments;
        for (auto &argument_syntax : syntax.arguments().arguments())
        {
            arguments.emplace_back(bind_expression(argument_syntax.value(), context));
        }

        if (const auto name_syntax = syntax.callee().as<IdentifierExpressionSyntax>(); name_syntax.has_value())
        {
            if (const auto overloads = lookup_from_syntax(name_syntax->value(), LookupOptions::callable, context);
                overloads.viable())
            {
                auto &overload = resolve_overload(overloads, arguments, syntax.callee().location(), context);
                auto interned = lifetime().copy_refs(arguments);
                return lifetime().create<BoundInvocationExpression>(syntax, overload, interned);
            }
        }

        auto &callee = bind_expression(syntax.callee(), context);
        arguments.insert(arguments.begin(), callee);

        // TODO: For now we don't have user-defined callable operators, so for now we just emit an error
        context.report_diagnostic(Diagnostic{
            DiagnosticInfo::create<DiagnosticCode::no_call_operator_defined>(callee.type().to_display_string()),
            syntax.callee().location()});
        return lifetime().create<BoundInvocationExpression>(syntax, unnamed_error_function, arguments);
    }

    const BoundExpression &Binder::add_conversion_if_necessary(const BoundExpression &expression,
                                                               const TypeSymbol &type,
                                                               const LookupContext &context) const
    {
        auto &conversions = conversion_classifier();
        const auto conversion = conversions.classify_conversion(expression.type(), type);
        return add_conversion_if_necessary(expression, type, conversion, context);
    }

    const BoundExpression &Binder::add_conversion_if_necessary(const BoundExpression &expression,
                                                               const TypeSymbol &type,
                                                               const Conversion &conversion,
                                                               const LookupContext &context) const
    {
        auto &syntax = expression.syntax().as_checked<ExpressionSyntax>();
        if (!conversion.exists())
        {
            context.report_diagnostic(
                Diagnostic{DiagnosticInfo::create<DiagnosticCode::no_conversion>(expression.type().to_display_string(),
                                                                                 type.to_display_string()),
                           syntax.location()});
        }
        else if (!conversion.is_identity())
        {
            if (!conversion.is_implicit())
            {
                context.report_diagnostic(Diagnostic{DiagnosticInfo::create<DiagnosticCode::conversion_is_explicit>(
                                                         expression.type().to_display_string(),
                                                         type.to_display_string()),
                                                     syntax.location()});
            }

            return lifetime().create<BoundConversionExpression>(syntax, expression, type, conversion);
        }

        return expression;
    }

    ConstantValue Binder::evaluate_constant_expression(const SyntaxToken &token,
                                                       const TypeSymbol *return_type,
                                                       const LookupContext &context) const
    {
        if (auto bool_value = token.try_get_value<bool>(); bool_value.has_value())
        {
            return ConstantValue::boolean(*bool_value);
        }

        if (const auto numeric_value = token.try_get_value<IntegerLiteralData>(); numeric_value.has_value())
        {
            return evaluate_numeric_expression(*numeric_value, return_type, token.location(), context);
        }

        if (const auto floating_point_value = token.try_get_value<FloatLiteralData>(); floating_point_value.has_value())
        {
            return evaluate_numeric_expression(*floating_point_value, return_type, token.location(), context);
        }

        if (const auto character_value = token.try_get_value<CharacterLiteralData>(); character_value.has_value())
        {
            switch (character_value->encoding)
            {
                case CharacterEncoding::utf8:
                    DEBUG_ASSERT(character_value->value <= std::numeric_limits<char8_t>::max());
                    return ConstantValue::character(static_cast<char8_t>(character_value->value));
                    break;
                case CharacterEncoding::utf16:
                    DEBUG_ASSERT(character_value->value <= std::numeric_limits<char16_t>::max());
                    return ConstantValue::character16(static_cast<char16_t>(character_value->value));
                    break;
                case CharacterEncoding::utf32:
                    return ConstantValue::rune(character_value->value);
            }
        }

        if (const auto string_value = token.try_get_value<StringLiteralData>(); string_value.has_value())
        {
            return ConstantValue::str(string_value->value);
        }

        UNREACHABLE("Invalid literal type");
    }

    ConstantValue Binder::evaluate_numeric_expression(const IntegerLiteralData &data,
                                                      const TypeSymbol *return_type,
                                                      const Location &location,
                                                      const LookupContext &context,
                                                      bool is_negative) const
    {
        auto target_type = get_integer_target_kind(data, return_type);

        // If it doesn't fit, we're going to raise an error and then truncate the value
        if (!fits_in(data.value, target_type, compilation_.target_settings()))
        {
            context.report_diagnostic(
                Diagnostic{DiagnosticInfo::create<DiagnosticCode::literal_value_too_big>(), location});
        }

        switch (target_type)
        {
            case IntegerTargetKind::i8:
                return ConstantValue::i8(maybe_negate(data.value, is_negative).convert_to<std::int8_t>());
            case IntegerTargetKind::i16:
                return ConstantValue::i16(maybe_negate(data.value, is_negative).convert_to<std::int16_t>());
            case IntegerTargetKind::i32:
                return ConstantValue::i32(maybe_negate(data.value, is_negative).convert_to<std::int32_t>());
            case IntegerTargetKind::i64:
                return ConstantValue::i64(maybe_negate(data.value, is_negative).convert_to<std::int64_t>());
            case IntegerTargetKind::i128:
                return ConstantValue::i128(maybe_negate(data.value, is_negative).convert_to<Int128>());
            case IntegerTargetKind::isize:
                return ConstantValue::isize(maybe_negate(data.value, is_negative).convert_to<std::int64_t>());
            case IntegerTargetKind::u8:
                throw_if_negative(is_negative);
                return ConstantValue::u8(data.value.convert_to<std::uint8_t>());
            case IntegerTargetKind::u16:
                throw_if_negative(is_negative);
                return ConstantValue::u16(data.value.convert_to<std::uint16_t>());
            case IntegerTargetKind::u32:
                throw_if_negative(is_negative);
                return ConstantValue::u32(data.value.convert_to<std::uint32_t>());
            case IntegerTargetKind::u64:
                throw_if_negative(is_negative);
                return ConstantValue::u64(data.value.convert_to<std::uint64_t>());
            case IntegerTargetKind::u128:
                throw_if_negative(is_negative);
                return ConstantValue::u128(data.value.convert_to<UInt128>());
            case IntegerTargetKind::usize:
                throw_if_negative(is_negative);
                return ConstantValue::usize(data.value.convert_to<std::uint64_t>());
            case IntegerTargetKind::f32:
                return ConstantValue::usize(data.value.convert_to<float>());
            case IntegerTargetKind::f64:
                return ConstantValue::usize(data.value.convert_to<double>());
            case IntegerTargetKind::best_fit:
                {
                    auto value = maybe_negate(data.value, is_negative);
                    if (fits_in<std::int32_t>(value))
                        return ConstantValue::i32(value.convert_to<std::int32_t>());

                    if (fits_in<std::uint32_t>(value))
                        return ConstantValue::u32(value.convert_to<std::uint32_t>());

                    if (fits_in<std::int64_t>(value))
                        return ConstantValue::i64(value.convert_to<std::int64_t>());

                    if (fits_in<std::uint64_t>(value))
                        return ConstantValue::u64(value.convert_to<std::uint64_t>());

                    if (fits_in<Int128>(value))
                        return ConstantValue::i128(value.convert_to<Int128>());

                    return ConstantValue::u128(value.convert_to<UInt128>());
                }
            default:
                UNREACHABLE("Invalid input");
        }
    }

    ConstantValue Binder::evaluate_numeric_expression(const FloatLiteralData &data,
                                                      const TypeSymbol *return_type,
                                                      const Location &location,
                                                      const LookupContext &context,
                                                      bool is_negative)
    {
        SpecialType target{};
        switch (data.suffix)
        {
            case FloatSuffix::none:
                target = return_type != nullptr ? return_type->special_type() : SpecialType::f64;
                break;
            case FloatSuffix::f32:
                target = SpecialType::f32;
                break;
            case FloatSuffix::f64:
                target = SpecialType::f64;
                break;
        }
        switch (target)
        {
            case SpecialType::f32:
                if (!fits_in_finite_float_magnitude<float>(data.significand, data.exponent10))
                {
                    context.report_diagnostic(
                        Diagnostic{DiagnosticInfo::create<DiagnosticCode::literal_value_too_big>(), location});
                }

                return ConstantValue::f32(parse_decimal_float<float>(data.significand, data.exponent10, is_negative));

            case SpecialType::f64:
                if (!fits_in_finite_float_magnitude<double>(data.significand, data.exponent10))
                {
                    context.report_diagnostic(
                        Diagnostic{DiagnosticInfo::create<DiagnosticCode::literal_value_too_big>(), location});
                }
                return ConstantValue::f64(parse_decimal_float<double>(data.significand, data.exponent10, is_negative));

            default:
                UNREACHABLE("Invalid input");
        }
    }
    const BoundUnaryExpression &Binder::create_unary_operation(const ExpressionSyntax &syntax,
                                                               UnaryOperation operation,
                                                               Ref<const BoundExpression> operand,
                                                               const LookupContext &context) const
    {
        const auto result_type = conversion_classifier().classify_unary_operand_type(operation, operand->type());
        if (result_type.has_value())
        {
            operand = add_conversion_if_necessary(operand, result_type->type, result_type->conversion, context);
        }
        else
        {
            context.report_diagnostic(Diagnostic{
                DiagnosticInfo::create<DiagnosticCode::unary_operator_undefined>(operand->type().to_display_string()),
                syntax.location()});
        }

        if (is_assigning_operation(operation) && !operand->is_assignable())
        {
            context.report_diagnostic(
                Diagnostic{DiagnosticInfo::create<DiagnosticCode::cannot_assign_expression>(), syntax.location()});
        }

        auto &final_type = result_type
                               .transform([](const OperandConversion &operand_conversion) -> const TypeSymbol &
                                          { return operand_conversion.type; })
                               .value_or_ref(unnamed_error_type);
        return lifetime().create<BoundUnaryExpression>(syntax, operand, operation, final_type);
    }

    const FunctionSymbol &Binder::resolve_overload(const LookupResult &result,
                                                   const std::span<Ref<const BoundExpression>> arguments,
                                                   const Location &location,
                                                   const LookupContext &context) const
    {
        DEBUG_ASSERT(result.viable());
        if (result.symbols().size() == 1)
        {
            auto &symbol = result.symbol().as_checked<FunctionSymbol>();
            if (symbol.parameters().size() != arguments.size())
            {
                context.report_diagnostic(
                    Diagnostic{DiagnosticInfo::create<DiagnosticCode::no_overload_matching_arg_count>(arguments.size()),
                               location});
                return symbol;
            }

            if (!try_match_overload(symbol, arguments, context))
            {
                context.report_diagnostic(Diagnostic{
                    DiagnosticInfo::create<DiagnosticCode::no_overload_for_arg_types>(get_type_names(arguments)),
                    location});
            }

            return symbol;
        }

        bool matches_arg_size = false;
        for (const auto overload : result.symbols())
        {
            auto &symbol = overload->as_checked<FunctionSymbol>();
            if (symbol.parameters().size() == arguments.size())
            {
                matches_arg_size = true;
            }

            if (!try_match_overload(symbol, arguments, context))
                continue;

            return symbol;
        }

        if (matches_arg_size)
        {
            context.report_diagnostic(
                Diagnostic{DiagnosticInfo::create<DiagnosticCode::no_overload_matching_arg_count>(arguments.size()),
                           location});
        }
        else
        {
            context.report_diagnostic(
                Diagnostic{DiagnosticInfo::create<DiagnosticCode::no_overload_for_arg_types>(get_type_names(arguments)),
                           location});
        }

        return result.symbols().front()->as_checked<FunctionSymbol>();
    }

    bool Binder::try_match_overload(const FunctionSymbol &overload,
                                    const std::span<Ref<const BoundExpression>> arguments,
                                    const LookupContext &context) const
    {
        DEBUG_ASSERT(overload.parameters().size() == arguments.size());
        PooledVector<Conversion> conversions;
        conversions.reserve(overload.parameters().size());

        auto &classifier = conversion_classifier();
        for (auto [param, arg] : std::views::zip(overload.parameters(), arguments))
        {
            if (auto &conversion = conversions.emplace_back(classifier.classify_conversion(arg->type(), param->type()));
                !conversion.is_implicit())
            {
                return false;
            }
        }

        for (auto [param, arg, conversion] : std::views::zip(overload.parameters(), arguments, conversions))
        {
            arg = add_conversion_if_necessary(arg, param->type(), conversion, context);
        }

        return true;
    }

} // namespace prism
