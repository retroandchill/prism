/**
 * @file parser_test.cpp
 * @author Francesco Corso
 * @date 7/28/2026
 * @brief
 */
module;
#include <catch2/catch_test_macros.hpp>
#include <catch2/matchers/catch_matchers_all.hpp>
module prism.core:tests.parser;

import :parser.language_parser;
import :syntax.literals;

using namespace prism;

TEST_CASE("Can parse variable declarations", "[parser]")
{
    LanguageParser parser{"var value: i32 = 5;"};
    auto declaration = parser.parse_declaration();

    CHECK_FALSE(declaration->contains_diagnostics());
    REQUIRE(declaration->is<GreenVariableDeclaration>());

    auto &variable = *declaration->as<GreenVariableDeclaration>();
    SyntaxToken identifier{variable.identifier(), 0};
    auto name = identifier.try_get_value<IdentifierData>();
    CHECK(name.has_value());
    CHECK(name->get_string_view() == "value");

    auto type = variable.type();
    REQUIRE(type.has_value());
    REQUIRE(type->type().is<GreenPredefinedType>());

    auto &built_in_type = type->type().as<GreenPredefinedType>()->keyword();
    CHECK(built_in_type.kind() == SyntaxKind::i32_keyword);

    auto initializer = variable.initializer();
    REQUIRE(initializer.has_value());

    auto &expression = initializer->value();
    REQUIRE(expression.is<GreenLiteralExpression>());
    SyntaxToken literal{expression.as<GreenLiteralExpression>()->value(), 0};

    auto value = literal.try_get_value<IntegerLiteralData>();
    REQUIRE(value.has_value());
    CHECK(value->value == 5);
}

TEST_CASE("Can parse function declarations", "[parser]")
{
    LanguageParser parser{"func add(x: i32, y: i32): i32 {\n"
                          "    return x + y;\n"
                          "}"};

    auto declaration = parser.parse_declaration();

    CHECK_FALSE(declaration->contains_diagnostics());
    REQUIRE(declaration->is<GreenFunctionDeclaration>());

    auto &function = *declaration->as<GreenFunctionDeclaration>();

    SyntaxToken identifier{function.identifier(), 0};
    auto name = identifier.try_get_value<IdentifierData>();
    CHECK(name.has_value());
    CHECK(name->get_string_view() == "add");

    auto type = function.return_type();
    REQUIRE(type.has_value());
    REQUIRE(type->type().is<GreenPredefinedType>());

    auto &built_in_type = type->type().as<GreenPredefinedType>()->keyword();
    CHECK(built_in_type.kind() == SyntaxKind::i32_keyword);

    auto &parameters = function.parameters().parameters();
    REQUIRE(parameters.size() == 2);

    SyntaxToken param1Name{parameters[0].name(), 0};
    auto param1NameValue = param1Name.try_get_value<IdentifierData>();
    CHECK(param1NameValue.has_value());
    CHECK(param1NameValue->get_string_view() == "x");

    SyntaxToken param2Name{parameters[1].name(), 0};
    auto param2NameValue = param2Name.try_get_value<IdentifierData>();
    CHECK(param2NameValue.has_value());
    CHECK(param2NameValue->get_string_view() == "y");

    CHECK(function.body().has_value());
    CHECK_FALSE(function.expression_body().has_value());
    CHECK_FALSE(function.semicolon().has_value());
}

TEST_CASE("Expression precedence is applied", "[parser]")
{
    LanguageParser parser{"1 + 2 * 3"};
    auto expression = parser.parse_expression();

    REQUIRE(expression->is<GreenBinaryExpression>());
    auto &binary_expression = *expression->as<GreenBinaryExpression>();
    CHECK(binary_expression.op().kind() == SyntaxKind::plus_token);
    CHECK(binary_expression.left().is<GreenLiteralExpression>());
    REQUIRE(binary_expression.right().is<GreenBinaryExpression>());

    auto &right_expression = *binary_expression.right().as<GreenBinaryExpression>();
    CHECK(right_expression.op().kind() == SyntaxKind::star_token);
    CHECK(right_expression.left().is<GreenLiteralExpression>());
    CHECK(right_expression.right().is<GreenLiteralExpression>());
}

TEST_CASE("Parentheses are applied", "[parser]")
{
    LanguageParser parser{"(1 + 2) * 3"};
    auto expression = parser.parse_expression();

    REQUIRE(expression->is<GreenBinaryExpression>());
    auto &binary_expression = *expression->as<GreenBinaryExpression>();
    CHECK(binary_expression.op().kind() == SyntaxKind::star_token);
    CHECK(binary_expression.right().is<GreenLiteralExpression>());
    REQUIRE(binary_expression.left().is<GreenParenthesizedExpression>());

    auto &left_expression = *binary_expression.left().as<GreenParenthesizedExpression>();
    REQUIRE(left_expression.expression().is<GreenBinaryExpression>());
    auto &inner = *left_expression.expression().as<GreenBinaryExpression>();
    CHECK(inner.op().kind() == SyntaxKind::plus_token);
    CHECK(inner.left().is<GreenLiteralExpression>());
    CHECK(inner.right().is<GreenLiteralExpression>());
}

TEST_CASE("Prefix operator are applied", "[parser]")
{
    LanguageParser parser{"-a * b + !c"};
    auto expression = parser.parse_expression();

    REQUIRE(expression->is<GreenBinaryExpression>());

    auto &binary_expression = *expression->as<GreenBinaryExpression>();
    CHECK(binary_expression.op().kind() == SyntaxKind::plus_token);
    REQUIRE(binary_expression.left().is<GreenBinaryExpression>());
    REQUIRE(binary_expression.right().is<GreenPrefixExpression>());

    auto &left = *binary_expression.left().as<GreenBinaryExpression>();
    CHECK(left.op().kind() == SyntaxKind::star_token);
    REQUIRE(left.left().is<GreenPrefixExpression>());
    REQUIRE(left.right().is<GreenIdentifierExpression>());

    CHECK(left.left().as<GreenPrefixExpression>()->op().kind() == SyntaxKind::minus_token);
    CHECK(binary_expression.right().as<GreenPrefixExpression>()->op().kind() == SyntaxKind::bang_token);
}

TEST_CASE("Can mix prefix and postfix operators", "[parser]")
{
    LanguageParser parser{"++x++"};
    auto expression = parser.parse_expression();

    REQUIRE(expression->is<GreenPrefixExpression>());
    auto &outer = *expression->as<GreenPrefixExpression>();
    CHECK(outer.op().kind() == SyntaxKind::plus_plus_token);
    REQUIRE(outer.operand().is<GreenPostfixExpression>());
    auto &inner = *outer.operand().as<GreenPostfixExpression>();
    CHECK(inner.op().kind() == SyntaxKind::plus_plus_token);
}

TEST_CASE("Assignment works as expected", "[parser]")
{
    LanguageParser parser{"x = -a * b + !c"};
    auto expression = parser.parse_expression();

    REQUIRE(expression->is<GreenAssignmentExpression>());
    auto &assignment = *expression->as<GreenAssignmentExpression>();
    CHECK(assignment.op().kind() == SyntaxKind::equal_token);
    REQUIRE(assignment.left().is<GreenIdentifierExpression>());
    REQUIRE(assignment.right().is<GreenBinaryExpression>());
}
