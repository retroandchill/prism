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

using namespace prism;

TEST_CASE("Can parse a simple variable declaration", "[parser]")
{
    const auto source = SourceFile{"var value: i32 = 5;"};
    Parser parser{source};
    auto [compilation_unit, diagnostics] = parser.parse();

    CHECK(compilation_unit.declarations.size() == 1);
    CHECK(diagnostics.size() == 0);
    REQUIRE(std::holds_alternative<VariableDeclarationSyntax>(compilation_unit.declarations[0]));

    auto &[variable_name, type, is_mutable, initializer] =
        std::get<VariableDeclarationSyntax>(compilation_unit.declarations[0]);
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
