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

#define ENUM_VALUE(r, data, i, elem) (SyntaxKind::PRISM_KIND_ENUM_NAME(elem))

#define PRISM_SYNTAX_KINDS(MACRO)                                                                                      \
    BOOST_PP_SEQ_FOR_EACH_I(MACRO, _, PRISM_SYNTAX_TRIVIA(PRISM_TRIVIA_ELEM))                                          \
    BOOST_PP_SEQ_FOR_EACH_I(MACRO, _, PRISM_SYNTAX_KEYWORDS(PRISM_KEYWORD_ELEM))                                       \
    BOOST_PP_SEQ_FOR_EACH_I(MACRO, _, PRISM_SYNTAX_PUNCTUATIONS(PRISM_PUNCTUATION_ELEM))                               \
    BOOST_PP_SEQ_FOR_EACH_I(MACRO, _, PRISM_SYNTAX_OTHER_TOKENS(PRISM_TOKEN_ELEM))

CATCH_REGISTER_ENUM(SyntaxKind, BOOST_PP_SEQ_ENUM(PRISM_SYNTAX_KINDS(ENUM_VALUE)))

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
            if (token->kind() == SyntaxKind::end_of_file_token)
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
                                             SyntaxKind::lparen_token,
                                             SyntaxKind::identifier_token,
                                             SyntaxKind::colon_token,
                                             SyntaxKind::i32_keyword,
                                             SyntaxKind::rparen_token,
                                             SyntaxKind::lbrace_token,
                                             SyntaxKind::return_keyword,
                                             SyntaxKind::identifier_token,
                                             SyntaxKind::plus_token,
                                             SyntaxKind::number_token,
                                             SyntaxKind::semicolon_token,
                                             SyntaxKind::rbrace_token,
                                             SyntaxKind::end_of_file_token}));
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
                                             SyntaxKind::number_token,
                                             SyntaxKind::semicolon_token,
                                             SyntaxKind::end_of_file_token}));
}

TEST_CASE("Skips block comments", "[lexer]")
{
    constexpr std::string_view source = "var x = /* This is a block comment */ 5;";

    auto tokens = lex_all(source);
    CHECK_THAT(tokens,
               Catch::Matchers::RangeEquals({SyntaxKind::var_keyword,
                                             SyntaxKind::identifier_token,
                                             SyntaxKind::equal_token,
                                             SyntaxKind::number_token,
                                             SyntaxKind::semicolon_token,
                                             SyntaxKind::end_of_file_token}));
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
                                             SyntaxKind::number_token,
                                             SyntaxKind::semicolon_token,
                                             SyntaxKind::end_of_file_token}));
}
