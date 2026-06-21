/**
 * @file lexer_test.cpp
 * @author Francesco Corso
 * @date 6/20/2026
 * @brief Test for the lexer module.
 */
#include <catch2/catch_test_macros.hpp>

import std;
import prism.core.lexer;
import prism.core.lexer.token;
import prism.core.source.source_file;
import prism.core.lexer.defaults;

using namespace prism;

TEST_CASE("Simple grammar", "[lexer]")
{
    SourceFile source_file{"func do_thing(x: int) {"
                           "    return x + 1;"
                           "}"};
    auto lexer = default_lexer();
    auto tokens = lexer.lex(source_file);

    REQUIRE(tokens.size() == 15);
    CHECK(tokens[0].kind == TokenKind::kw_func);
    CHECK(tokens[1].kind == TokenKind::identifier);
    CHECK(tokens[2].kind == TokenKind::lparen);
    CHECK(tokens[3].kind == TokenKind::identifier);
    CHECK(tokens[4].kind == TokenKind::colon);
    CHECK(tokens[5].kind == TokenKind::identifier);
    CHECK(tokens[6].kind == TokenKind::rparen);
    CHECK(tokens[7].kind == TokenKind::lbrace);
    CHECK(tokens[8].kind == TokenKind::kw_return);
    CHECK(tokens[9].kind == TokenKind::identifier);
    CHECK(tokens[10].kind == TokenKind::plus);
    CHECK(tokens[11].kind == TokenKind::number);
    CHECK(tokens[12].kind == TokenKind::semicolon);
    CHECK(tokens[13].kind == TokenKind::rbrace);
    CHECK(tokens[14].kind == TokenKind::eof);
}
