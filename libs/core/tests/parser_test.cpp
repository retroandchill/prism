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

using namespace prism;

TEST_CASE("Can parse a simple variable declaration", "[parser]")
{
    const auto source = SourceFile{"var value: i32 = 5;"};
    auto sink = DiagnosticSink{};
    Parser parser{source, sink};
    auto declaration = parser.parse_declaration();

    CHECK(sink.diagnostics().size() == 0); // NOLINT(*-container-size-empty)
    REQUIRE(std::holds_alternative<VariableDeclarationSyntax>(declaration));

    auto &[variable_name, type, is_mutable, initializer] = std::get<VariableDeclarationSyntax>(declaration);
    CHECK_FALSE(is_mutable);

    REQUIRE(std::holds_alternative<ValidIdentifierSyntax>(variable_name));

    auto &[name, range] = std::get<ValidIdentifierSyntax>(variable_name);
    CHECK(*name == "value");

    REQUIRE(type.has_value());
    REQUIRE(std::holds_alternative<BuiltInTypeSyntax>(*type));
    auto &[type_code, type_range] = std::get<BuiltInTypeSyntax>(*type);
    CHECK(type_code == BuiltInType::i32);

    REQUIRE(initializer != nullptr);
    REQUIRE(std::holds_alternative<LiteralSyntax>(*initializer));

    auto &[literal_value, literal_range] = std::get<LiteralSyntax>(*initializer);

    REQUIRE(std::holds_alternative<std::uint64_t>(literal_value));
    CHECK(std::get<std::uint64_t>(literal_value) == 5);
}

TEST_CASE("Can parse a function declaration", "[parser]")
{
    const auto source = SourceFile{"func add(x: i32, y: i32) -> i32 {"
                                   "    return x + y;"
                                   "}"};
    auto sink = DiagnosticSink{};
    Parser parser{source, sink};
    auto declaration = parser.parse_declaration();

    REQUIRE(std::holds_alternative<FunctionDeclarationSyntax>(declaration));
    auto &[function_name, return_type, parameters, body] = std::get<FunctionDeclarationSyntax>(declaration);

    REQUIRE(std::holds_alternative<ValidIdentifierSyntax>(function_name));
    auto &[name, range] = std::get<ValidIdentifierSyntax>(function_name);
    CHECK(*name == "add");

    REQUIRE(return_type.has_value());
    REQUIRE(std::holds_alternative<BuiltInTypeSyntax>(*return_type));
    auto &[type_code, type_range] = std::get<BuiltInTypeSyntax>(*return_type);
    CHECK(type_code == BuiltInType::i32);

    REQUIRE(parameters.size() == 2);
    CHECK_FALSE(parameters[0].is_mutable);
    CHECK_FALSE(parameters[1].is_mutable);

    REQUIRE(std::holds_alternative<ValidIdentifierSyntax>(parameters[0].name));
    REQUIRE(std::holds_alternative<ValidIdentifierSyntax>(parameters[1].name));
    auto &[param1_name, param1_range] = std::get<ValidIdentifierSyntax>(parameters[0].name);
    auto &[param2_name, param2_range] = std::get<ValidIdentifierSyntax>(parameters[1].name);
    CHECK(*param1_name == "x");
    CHECK(*param2_name == "y");

    REQUIRE(std::holds_alternative<BuiltInTypeSyntax>(parameters[0].type));
    REQUIRE(std::holds_alternative<BuiltInTypeSyntax>(parameters[1].type));
    auto &[param1_type, param1_type_range] = std::get<BuiltInTypeSyntax>(parameters[0].type);
    auto &[param2_type, param2_type_range] = std::get<BuiltInTypeSyntax>(parameters[1].type);
    CHECK(param1_type == BuiltInType::i32);
    CHECK(param2_type == BuiltInType::i32);

    REQUIRE(std::holds_alternative<BlockSyntax>(body));
    auto &[statements] = std::get<BlockSyntax>(body);
    CHECK(statements.size() == 1);
    REQUIRE(std::holds_alternative<ReturnStatementSyntax>(statements[0]));
    auto &[expression] = std::get<ReturnStatementSyntax>(statements[0]);
    REQUIRE(std::holds_alternative<BinaryExpressionSyntax>(expression));
    auto &binary_expression = std::get<BinaryExpressionSyntax>(expression);
    CHECK(binary_expression.op == BinaryOperator::add);
}

TEST_CASE("Can parse various expressions", "[parser]")
{
    SECTION("Simple precedence evaluation")
    {
        const auto source = SourceFile{"1 + 2 * 3"};
        auto sink = DiagnosticSink{};
        Parser parser{source, sink};
        auto expression = parser.parse_expression();

        REQUIRE(std::holds_alternative<BinaryExpressionSyntax>(expression));
        auto &[op, left, right] = std::get<BinaryExpressionSyntax>(expression);
        CHECK(op == BinaryOperator::add);

        CHECK(std::holds_alternative<LiteralSyntax>(*left));
        REQUIRE(std::holds_alternative<BinaryExpressionSyntax>(*right));
        auto &inner = std::get<BinaryExpressionSyntax>(*right);
        CHECK(inner.op == BinaryOperator::mul);
    }

    SECTION("Parenthetical grouping")
    {
        const auto source = SourceFile{"(1 + 2) * 3"};
        auto sink = DiagnosticSink{};
        Parser parser{source, sink};
        auto expression = parser.parse_expression();

        REQUIRE(std::holds_alternative<BinaryExpressionSyntax>(expression));
        auto &[op, left, right] = std::get<BinaryExpressionSyntax>(expression);
        CHECK(op == BinaryOperator::mul);

        REQUIRE(std::holds_alternative<BinaryExpressionSyntax>(*left));
        CHECK(std::holds_alternative<LiteralSyntax>(*right));
        auto &inner = std::get<BinaryExpressionSyntax>(*left);
        CHECK(inner.op == BinaryOperator::add);
    }

    SECTION("Can parse with unary operators")
    {
        const auto source = SourceFile{"-a * b + !c"};
        auto sink = DiagnosticSink{};
        Parser parser{source, sink};
        auto expression = parser.parse_expression();

        REQUIRE(std::holds_alternative<BinaryExpressionSyntax>(expression));
        auto &[op, left, right] = std::get<BinaryExpressionSyntax>(expression);
        CHECK(op == BinaryOperator::add);

        REQUIRE(std::holds_alternative<BinaryExpressionSyntax>(*left));
        auto &inner = std::get<BinaryExpressionSyntax>(*left);
        CHECK(inner.op == BinaryOperator::mul);
        REQUIRE(std::holds_alternative<UnaryExpressionSyntax>(*inner.left));
        auto &unary1 = std::get<UnaryExpressionSyntax>(*inner.left);
        CHECK(unary1.op == UnaryOperator::negate);

        REQUIRE(std::holds_alternative<UnaryExpressionSyntax>(*right));
        auto &unary2 = std::get<UnaryExpressionSyntax>(*right);
        CHECK(unary2.op == UnaryOperator::logical_not);
    }

    SECTION("Prefix and postfix")
    {
        const auto source = SourceFile{"++x++"};
        auto sink = DiagnosticSink{};
        Parser parser{source, sink};
        auto expression = parser.parse_expression();

        REQUIRE(std::holds_alternative<UnaryExpressionSyntax>(expression));
        auto &outer = std::get<UnaryExpressionSyntax>(expression);
        CHECK(outer.op == UnaryOperator::pre_increment);
        REQUIRE(std::holds_alternative<UnaryExpressionSyntax>(*outer.operand));
        auto &inner = std::get<UnaryExpressionSyntax>(*outer.operand);
        CHECK(inner.op == UnaryOperator::post_increment);
    }

    SECTION("Assignment")
    {
        const auto source = SourceFile{"x = -a * b + !c"};
        auto sink = DiagnosticSink{};
        Parser parser{source, sink};
        auto expression = parser.parse_expression();

        REQUIRE(std::holds_alternative<BinaryExpressionSyntax>(expression));
        auto &[op, left, right] = std::get<BinaryExpressionSyntax>(expression);
        CHECK(op == BinaryOperator::assign);

        CHECK(std::holds_alternative<IdentifierSyntax>(*left));
        REQUIRE(std::holds_alternative<BinaryExpressionSyntax>(*right));
        auto &assignment_expression = std::get<BinaryExpressionSyntax>(*right);

        REQUIRE(std::holds_alternative<BinaryExpressionSyntax>(*assignment_expression.left));
        auto &inner = std::get<BinaryExpressionSyntax>(*assignment_expression.left);
        CHECK(inner.op == BinaryOperator::mul);
        REQUIRE(std::holds_alternative<UnaryExpressionSyntax>(*inner.left));
        auto &unary1 = std::get<UnaryExpressionSyntax>(*inner.left);
        CHECK(unary1.op == UnaryOperator::negate);

        REQUIRE(std::holds_alternative<UnaryExpressionSyntax>(*assignment_expression.right));
        auto &unary2 = std::get<UnaryExpressionSyntax>(*assignment_expression.right);
        CHECK(unary2.op == UnaryOperator::logical_not);
    }
}
