/**
 * @file binder_test.cpp
 * @author Francesco Corso
 * @date 8/22/2026
 * @brief
 */
module;
#include <catch2/catch_test_macros.hpp>
#include <catch2/matchers/catch_matchers_all.hpp>
module prism.core:tests.binder;

import :syntax.tree;
import :syntax.declarations;
import :compilation;
import :symbols.assembly_symbol;
import :symbols.namespace_symbol;
import :symbols.variable_symbol;
import :symbols.function_symbol;
import :symbols.parameter_symbol;
import :binder;
import :semantic.semantic_model;
import :binder.lookup_context;
import :semantic.bound.bound_expression;

using namespace prism;

TEST_CASE("Variables bound initializers correctly", "[binder]")
{
    auto tree = SyntaxTree::parse("var x: i32 = 5;");
    auto compilation = Compilation::create("test", {tree});

    auto members = compilation->assembly().global_namespace().members();
    REQUIRE(members.size() == 1);
    const auto x = members.front();
    REQUIRE(x.is<VariableSymbol>());
    auto &variable = x->as_checked<VariableSymbol>();
    CHECK(variable.type().special_type() == SpecialType::i32);

    REQUIRE(variable.declaring_syntax_references().size() == 1);
    auto declaration = variable.declaring_syntax_references().front().syntax().as<VariableDeclarationSyntax>();
    REQUIRE(declaration.has_value());

    auto &semantic_model = compilation->get_semantic_model(*tree);

    DiagnosticBag bag;
    LookupContext context{bag};
    auto &expression = SemanticModelInternal::get_bound_initializer(semantic_model, *declaration, context);
    CHECK_FALSE(bag.has_any_errors());
    REQUIRE(expression.is<BoundLiteral>());

    auto &literal = expression.as_checked<BoundLiteral>();
    auto &value = literal.value();
    REQUIRE(value.kind() == ConstantValue::Kind::i32);
    REQUIRE(value.as_i64() == 5);
}
