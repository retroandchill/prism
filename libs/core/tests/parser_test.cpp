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
import prism.core.ast.common_syntax;
import prism.core.ast.statement_syntax;
import prism.core.ast.type_syntax;
import prism.core.ast.expression_syntax;
import prism.core.diagnostic;
import prism.core.context;

using namespace prism;

TEST_CASE("Can parse a simple variable declaration", "[parser]")
{
    auto ctx = CompilationContext{"var value: i32 = 5;"};
    Parser parser{ctx};
    auto declaration = parser.parse_declaration();

    CHECK(ctx.diagnostics().size() == 0); // NOLINT(*-container-size-empty)
    REQUIRE(declaration.is<VariableDeclarationSyntax>());

    auto &[variable_name, type, is_mutable, modifiers, initializer] = declaration.get<VariableDeclarationSyntax>();
    CHECK_FALSE(is_mutable);

    CHECK(variable_name.name.as_string_view() == "value");

    REQUIRE(type.has_value());
    REQUIRE(type->is<BuiltInType>());
    auto type_code = type->get<BuiltInType>();
    CHECK(type_code == BuiltInType::i32);

    REQUIRE(initializer != nullptr);
    REQUIRE(initializer->is<LiteralSyntax>());

    auto &[literal_kind] = initializer->get<LiteralSyntax>();

    CHECK(literal_kind == LiteralSyntaxKind::integer);
}

TEST_CASE("Can parse a function declaration", "[parser]")
{
    auto ctx = CompilationContext{"func add(x: i32, y: i32) -> i32 {"
                                  "    return x + y;"
                                  "}"};
    Parser parser{ctx};
    auto declaration = parser.parse_declaration();

    REQUIRE(declaration.is<FunctionDeclarationSyntax>());
    auto &[function_name, return_type, parameters, body, modifiers] = declaration.get<FunctionDeclarationSyntax>();

    CHECK(function_name.name.as_string_view() == "add");

    REQUIRE(return_type.has_value());
    REQUIRE(return_type->is<BuiltInType>());
    const auto type_code = return_type->get<BuiltInType>();
    CHECK(type_code == BuiltInType::i32);

    REQUIRE(parameters.size() == 2);
    CHECK_FALSE(parameters[0].is_mutable);
    CHECK_FALSE(parameters[1].is_mutable);

    CHECK(parameters[0].name.name.as_string_view() == "x");
    CHECK(parameters[1].name.name.as_string_view() == "y");

    REQUIRE(parameters[0].type.is<BuiltInType>());
    REQUIRE(parameters[1].type.is<BuiltInType>());
    const auto param1_type = parameters[0].type.get<BuiltInType>();
    const auto param2_type = parameters[1].type.get<BuiltInType>();
    CHECK(param1_type == BuiltInType::i32);
    CHECK(param2_type == BuiltInType::i32);

    REQUIRE(body.is<BlockSyntax>());
    auto &[statements] = body.get<BlockSyntax>();
    CHECK(statements.size() == 1);
    REQUIRE(statements[0].is<ReturnStatementSyntax>());
    auto &[expression] = statements[0].get<ReturnStatementSyntax>();
    REQUIRE(expression.is<BinaryExpressionSyntax>());
    auto &binary_expression = expression.get<BinaryExpressionSyntax>();
    CHECK(binary_expression.op == BinaryOperator::add);
}

TEST_CASE("Can parse various expressions", "[parser]")
{
    SECTION("Simple precedence evaluation")
    {
        auto ctx = CompilationContext{"1 + 2 * 3"};
        Parser parser{ctx};
        auto expression = parser.parse_expression();

        REQUIRE(expression.is<BinaryExpressionSyntax>());
        auto &[op, left, right] = expression.get<BinaryExpressionSyntax>();
        CHECK(op == BinaryOperator::add);

        CHECK(left->is<LiteralSyntax>());
        REQUIRE(right->is<BinaryExpressionSyntax>());
        auto &inner = right->get<BinaryExpressionSyntax>();
        CHECK(inner.op == BinaryOperator::mul);
    }

    SECTION("Parenthetical grouping")
    {
        auto ctx = CompilationContext{"(1 + 2) * 3"};
        Parser parser{ctx};
        auto expression = parser.parse_expression();

        REQUIRE(expression.is<BinaryExpressionSyntax>());
        auto &[op, left, right] = expression.get<BinaryExpressionSyntax>();
        CHECK(op == BinaryOperator::mul);

        REQUIRE(left->is<BinaryExpressionSyntax>());
        CHECK(right->is<LiteralSyntax>());
        auto &inner = left->get<BinaryExpressionSyntax>();
        CHECK(inner.op == BinaryOperator::add);
    }

    SECTION("Can parse with unary operators")
    {
        auto ctx = CompilationContext{"-a * b + !c"};
        Parser parser{ctx};
        auto expression = parser.parse_expression();

        REQUIRE(expression.is<BinaryExpressionSyntax>());
        auto &[op, left, right] = expression.get<BinaryExpressionSyntax>();
        CHECK(op == BinaryOperator::add);

        REQUIRE(left->is<BinaryExpressionSyntax>());
        auto &inner = left->get<BinaryExpressionSyntax>();
        CHECK(inner.op == BinaryOperator::mul);
        REQUIRE(inner.left->is<UnaryExpressionSyntax>());
        auto &unary1 = inner.left->get<UnaryExpressionSyntax>();
        CHECK(unary1.op == UnaryOperator::negate);

        REQUIRE(right->is<UnaryExpressionSyntax>());
        auto &unary2 = right->get<UnaryExpressionSyntax>();
        CHECK(unary2.op == UnaryOperator::logical_not);
    }

    SECTION("Prefix and postfix")
    {
        auto ctx = CompilationContext{"++x++"};
        Parser parser{ctx};
        auto expression = parser.parse_expression();

        REQUIRE(expression.is<UnaryExpressionSyntax>());
        auto &outer = expression.get<UnaryExpressionSyntax>();
        CHECK(outer.op == UnaryOperator::pre_increment);
        REQUIRE(outer.operand->is<UnaryExpressionSyntax>());
        auto &inner = outer.operand->get<UnaryExpressionSyntax>();
        CHECK(inner.op == UnaryOperator::post_increment);
    }

    SECTION("Assignment")
    {
        auto ctx = CompilationContext{"x = -a * b + !c"};
        Parser parser{ctx};
        auto expression = parser.parse_expression();

        REQUIRE(expression.is<BinaryExpressionSyntax>());
        auto &[op, left, right] = expression.get<BinaryExpressionSyntax>();
        CHECK(op == BinaryOperator::assign);

        CHECK(left->is<IdentifierSyntax>());
        REQUIRE(right->is<BinaryExpressionSyntax>());
        auto &assignment_expression = right->get<BinaryExpressionSyntax>();

        REQUIRE(assignment_expression.left->is<BinaryExpressionSyntax>());
        auto &inner = assignment_expression.left->get<BinaryExpressionSyntax>();
        CHECK(inner.op == BinaryOperator::mul);
        REQUIRE(inner.left->is<UnaryExpressionSyntax>());
        auto &unary1 = inner.left->get<UnaryExpressionSyntax>();
        CHECK(unary1.op == UnaryOperator::negate);

        REQUIRE(assignment_expression.right->is<UnaryExpressionSyntax>());
        auto &unary2 = assignment_expression.right->get<UnaryExpressionSyntax>();
        CHECK(unary2.op == UnaryOperator::logical_not);
    }
}
