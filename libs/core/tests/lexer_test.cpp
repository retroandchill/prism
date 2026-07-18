/**
 * @file lexer_test.cpp
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
module;
#include "prism/core/syntax.hpp"

#include <catch2/catch_test_macros.hpp>
#include <catch2/matchers/catch_matchers_all.hpp>
module prism.core:tests.lexer;

import :syntax.lexer;

import std;

using namespace prism;

namespace
{
    std::vector<SyntaxKind> lex_all(std::string_view source)
    {
        Lexer lexer{source};
        std::vector<SyntaxKind> tokens;
        while (true)
        {
            auto token = lexer.next();
            tokens.push_back(token->kind());
            if (token->kind() == SyntaxKind::eof_token)
                break;
        }

        return tokens;
    }
} // namespace

TEST_CASE("Simple grammar", "[lexer]")
{
    constexpr std::string_view source = R"(
            func do_thing(x: i32) {
              return x + 1;
            })";

    auto tokens = lex_all(source);
    CHECK_THAT(tokens,
               Catch::Matchers::RangeEquals({SyntaxKind::func_keyword,
                                             SyntaxKind::identifier_token,
                                             SyntaxKind::open_paren_token,
                                             SyntaxKind::identifier_token,
                                             SyntaxKind::colon_token,
                                             SyntaxKind::i32_keyword,
                                             SyntaxKind::close_paren_token,
                                             SyntaxKind::open_brace_token,
                                             SyntaxKind::return_keyword,
                                             SyntaxKind::identifier_token,
                                             SyntaxKind::plus_token,
                                             SyntaxKind::numeric_literal_token,
                                             SyntaxKind::semicolon_token,
                                             SyntaxKind::close_brace_token,
                                             SyntaxKind::eof_token}));
}

TEST_CASE("Skips line comments", "[lexer]")
{
    constexpr std::string_view source = R"(
            // This is a line comment
            var x = 5; // This is another line comment
        )";

    auto tokens = lex_all(source);
    CHECK_THAT(tokens,
               Catch::Matchers::RangeEquals({SyntaxKind::var_keyword,
                                             SyntaxKind::identifier_token,
                                             SyntaxKind::equal_token,
                                             SyntaxKind::numeric_literal_token,
                                             SyntaxKind::semicolon_token,
                                             SyntaxKind::eof_token}));
}

TEST_CASE("Skips block comments", "[lexer]")
{
    constexpr std::string_view source = "var x = /* This is a block comment */ 5;";

    auto tokens = lex_all(source);
    CHECK_THAT(tokens,
               Catch::Matchers::RangeEquals({SyntaxKind::var_keyword,
                                             SyntaxKind::identifier_token,
                                             SyntaxKind::equal_token,
                                             SyntaxKind::numeric_literal_token,
                                             SyntaxKind::semicolon_token,
                                             SyntaxKind::eof_token}));
}

TEST_CASE("Block comments span multiple lines", "[lexer]")
{
    constexpr std::string_view source = R"(
            /*
            This is a block comment
            that spans multiple lines
            */
            var x = 5;
        )";

    auto tokens = lex_all(source);
    CHECK_THAT(tokens,
               Catch::Matchers::RangeEquals({SyntaxKind::var_keyword,
                                             SyntaxKind::identifier_token,
                                             SyntaxKind::equal_token,
                                             SyntaxKind::numeric_literal_token,
                                             SyntaxKind::semicolon_token,
                                             SyntaxKind::eof_token}));
}
