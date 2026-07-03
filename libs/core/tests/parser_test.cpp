/**
 * @file parser_test.cpp
 * @author Francesco Corso
 * @date 6/26/2026
 * @brief
 */
#include <catch2/catch_test_macros.hpp>

import std;
import prism.core.parser;
import prism.core.source.source_file;
import prism.core.ast;
import prism.core.diagnostic;
import prism.core.context;
import prism.core.util;

using namespace prism;

TEST_CASE("Can parse a simple variable declaration", "[parser]")
{
    auto ctx = CompilationContext{"var value: i32 = 5;"};
    Parser parser{ctx};
    auto &declaration = parser.parse_declaration();

    CHECK(ctx.diagnostics().size() == 0); // NOLINT(*-container-size-empty)
    REQUIRE(declaration.is<VariableDeclarationSyntax>());

    auto &variable = declaration.cast<VariableDeclarationSyntax>();
    CHECK_FALSE(variable.is_mutable());

    CHECK(variable.name().name.as_string_view() == "value");

    REQUIRE(variable.type() != nullptr);
    REQUIRE(variable.type()->is<BuiltInTypeSyntax>());
    auto type_code = variable.type()->cast<BuiltInTypeSyntax>().type();
    CHECK(type_code == BuiltInType::i32);

    REQUIRE(variable.initializer() != nullptr);
    REQUIRE(variable.initializer()->is<LiteralExpressionSyntax>());

    auto literal_kind = variable.initializer()->cast<LiteralExpressionSyntax>().kind();

    CHECK(literal_kind == LiteralSyntaxKind::integer);
}

TEST_CASE("Can parse a function declaration", "[parser]")
{
    auto ctx = CompilationContext{"func add(x: i32, y: i32) -> i32 {"
                                  "    return x + y;"
                                  "}"};
    Parser parser{ctx};
    auto &declaration = parser.parse_declaration();

    REQUIRE(declaration.is<FunctionDeclarationSyntax>());
    auto &function = declaration.cast<FunctionDeclarationSyntax>();

    CHECK(function.name().name.as_string_view() == "add");

    REQUIRE(function.return_type() != nullptr);
    REQUIRE(function.return_type()->is<BuiltInTypeSyntax>());
    const auto type_code = function.return_type()->cast<BuiltInTypeSyntax>().type();
    CHECK(type_code == BuiltInType::i32);

    auto parameters = function.parameters();
    REQUIRE(parameters.size() == 2);
    CHECK_FALSE(parameters[0]->is_mutable());
    CHECK_FALSE(parameters[1]->is_mutable());

    CHECK(parameters[0]->name().name.as_string_view() == "x");
    CHECK(parameters[1]->name().name.as_string_view() == "y");

    REQUIRE(parameters[0]->type().is<BuiltInTypeSyntax>());
    REQUIRE(parameters[1]->type().is<BuiltInTypeSyntax>());
    const auto param1_type = parameters[0]->type().cast<BuiltInTypeSyntax>().type();
    const auto param2_type = parameters[1]->type().cast<BuiltInTypeSyntax>().type();
    CHECK(param1_type == BuiltInType::i32);
    CHECK(param2_type == BuiltInType::i32);

    REQUIRE(std::holds_alternative<Ref<const BlockStatementSyntax>>(function.body()));
    auto statements = std::get<Ref<const BlockStatementSyntax>>(function.body())->statements();
    CHECK(statements.size() == 1);
    REQUIRE(statements[0]->is<ReturnStatementSyntax>());
    auto &expression = statements[0]->cast<ReturnStatementSyntax>().expression();
    REQUIRE(expression.is<BinaryExpressionSyntax>());
    auto &binary_expression = expression.cast<BinaryExpressionSyntax>();
    CHECK(binary_expression.op() == BinaryOperator::add);
}

TEST_CASE("Can parse various expressions", "[parser]")
{
    SECTION("Simple precedence evaluation")
    {
        auto ctx = CompilationContext{"1 + 2 * 3"};
        Parser parser{ctx};
        auto &expression = parser.parse_expression();

        REQUIRE(expression.is<BinaryExpressionSyntax>());
        auto &bin_expr = expression.cast<BinaryExpressionSyntax>();
        CHECK(bin_expr.op() == BinaryOperator::add);

        CHECK(bin_expr.left().is<LiteralExpressionSyntax>());
        REQUIRE(bin_expr.right().is<BinaryExpressionSyntax>());
        auto &inner = bin_expr.right().cast<BinaryExpressionSyntax>();
        CHECK(inner.op() == BinaryOperator::mul);
    }

    SECTION("Parenthetical grouping")
    {
        auto ctx = CompilationContext{"(1 + 2) * 3"};
        Parser parser{ctx};
        auto &expression = parser.parse_expression();

        REQUIRE(expression.is<BinaryExpressionSyntax>());
        auto &bin_expr = expression.cast<BinaryExpressionSyntax>();
        CHECK(bin_expr.op() == BinaryOperator::mul);

        REQUIRE(bin_expr.left().is<BinaryExpressionSyntax>());
        CHECK(bin_expr.right().is<LiteralExpressionSyntax>());
        auto &inner = bin_expr.left().cast<BinaryExpressionSyntax>();
        CHECK(inner.op() == BinaryOperator::add);
    }

    SECTION("Can parse with unary operators")
    {
        auto ctx = CompilationContext{"-a * b + !c"};
        Parser parser{ctx};
        auto &expression = parser.parse_expression();

        REQUIRE(expression.is<BinaryExpressionSyntax>());
        auto &bin_expr = expression.cast<BinaryExpressionSyntax>();
        CHECK(bin_expr.op() == BinaryOperator::add);

        auto &left = bin_expr.left();
        auto &right = bin_expr.right();
        REQUIRE(left.is<BinaryExpressionSyntax>());
        auto &inner = left.cast<BinaryExpressionSyntax>();
        CHECK(inner.op() == BinaryOperator::mul);
        REQUIRE(inner.left().is<UnaryExpressionSyntax>());
        auto &unary1 = inner.left().cast<UnaryExpressionSyntax>();
        CHECK(unary1.op() == UnaryOperator::negate);

        REQUIRE(right.is<UnaryExpressionSyntax>());
        auto &unary2 = right.cast<UnaryExpressionSyntax>();
        CHECK(unary2.op() == UnaryOperator::logical_not);
    }

    SECTION("Prefix and postfix")
    {
        auto ctx = CompilationContext{"++x++"};
        Parser parser{ctx};
        auto &expression = parser.parse_expression();

        REQUIRE(expression.is<UnaryExpressionSyntax>());
        auto &outer = expression.cast<UnaryExpressionSyntax>();
        CHECK(outer.op() == UnaryOperator::pre_increment);
        REQUIRE(outer.operand().is<UnaryExpressionSyntax>());
        auto &inner = outer.operand().cast<UnaryExpressionSyntax>();
        CHECK(inner.op() == UnaryOperator::post_increment);
    }

    SECTION("Assignment")
    {
        auto ctx = CompilationContext{"x = -a * b + !c"};
        Parser parser{ctx};
        auto &expression = parser.parse_expression();

        REQUIRE(expression.is<BinaryExpressionSyntax>());
        auto &bin_expr = expression.cast<BinaryExpressionSyntax>();
        auto op = bin_expr.op();
        auto &left = bin_expr.left();
        auto &right = bin_expr.right();

        CHECK(op == BinaryOperator::assign);

        CHECK(left.is<IdentifierExpressionSyntax>());
        REQUIRE(right.is<BinaryExpressionSyntax>());
        auto &assignment_expression = right.cast<BinaryExpressionSyntax>();

        REQUIRE(assignment_expression.left().is<BinaryExpressionSyntax>());
        auto &inner = assignment_expression.left().cast<BinaryExpressionSyntax>();
        CHECK(inner.op() == BinaryOperator::mul);
        REQUIRE(inner.left().is<UnaryExpressionSyntax>());
        auto &unary1 = inner.left().cast<UnaryExpressionSyntax>();
        CHECK(unary1.op() == UnaryOperator::negate);

        REQUIRE(assignment_expression.right().is<UnaryExpressionSyntax>());
        auto &unary2 = assignment_expression.right().cast<UnaryExpressionSyntax>();
        CHECK(unary2.op() == UnaryOperator::logical_not);
    }
}
