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

using namespace prism;

namespace
{
    std::vector<Token> lex_all(const SourceFile &source_file)
    {
        Lexer lexer{source_file};
        std::vector<Token> tokens;
        while (true)
        {
            auto token = lexer.next();
            tokens.push_back(token);
            if (token.kind == TokenKind::eof)
                break;
        }
        return tokens;
    }
} // namespace

TEST_CASE("Simple grammar", "[lexer]")
{
    const SourceFile source_file{"func do_thing(x: i32) {"
                                 "    return x + 1;"
                                 "}"};
    auto tokens = lex_all(source_file);

    REQUIRE(tokens.size() == 15);
    CHECK(tokens[0].kind == TokenKind::kw_func);
    CHECK(tokens[1].kind == TokenKind::identifier);
    CHECK(tokens[2].kind == TokenKind::lparen);
    CHECK(tokens[3].kind == TokenKind::identifier);
    CHECK(tokens[4].kind == TokenKind::colon);
    CHECK(tokens[5].kind == TokenKind::kw_i32);
    CHECK(tokens[6].kind == TokenKind::rparen);
    CHECK(tokens[7].kind == TokenKind::lbrace);
    CHECK(tokens[8].kind == TokenKind::kw_return);
    CHECK(tokens[9].kind == TokenKind::identifier);
    CHECK(tokens[10].kind == TokenKind::plus);
    CHECK(tokens[11].kind == TokenKind::integer);
    CHECK(tokens[12].kind == TokenKind::semicolon);
    CHECK(tokens[13].kind == TokenKind::rbrace);
    CHECK(tokens[14].kind == TokenKind::eof);
}

TEST_CASE("Skips line comments until the end of the line", "[lexer]")
{
    const SourceFile source_file{"// This is a line comment\n"
                                 "var x = 5; // This is another line comment\n"};
    auto tokens = lex_all(source_file);
    REQUIRE(tokens.size() == 6);
    CHECK(tokens[0].kind == TokenKind::kw_var);
    CHECK(tokens[1].kind == TokenKind::identifier);
    CHECK(tokens[2].kind == TokenKind::equal);
    CHECK(tokens[3].kind == TokenKind::integer);
    CHECK(tokens[4].kind == TokenKind::semicolon);
    CHECK(tokens[5].kind == TokenKind::eof);
}

TEST_CASE("Block comments can split a single line", "[lexer]")
{
    const SourceFile source_file{"var x = /* Block comment */ 5;"};
    auto tokens = lex_all(source_file);
    REQUIRE(tokens.size() == 6);
    CHECK(tokens[0].kind == TokenKind::kw_var);
    CHECK(tokens[1].kind == TokenKind::identifier);
    CHECK(tokens[2].kind == TokenKind::equal);
    CHECK(tokens[3].kind == TokenKind::integer);
    CHECK(tokens[4].kind == TokenKind::semicolon);
    CHECK(tokens[5].kind == TokenKind::eof);
}

TEST_CASE("Block comments can span multiple lines", "[lexer]")
{
    const SourceFile source_file{"/*\n"
                                 "This is a multiline block comment\n"
                                 "*/\n"
                                 "var x = 5;"};
    auto tokens = lex_all(source_file);
    REQUIRE(tokens.size() == 6);
    CHECK(tokens[0].kind == TokenKind::kw_var);
    CHECK(tokens[1].kind == TokenKind::identifier);
    CHECK(tokens[2].kind == TokenKind::equal);
    CHECK(tokens[3].kind == TokenKind::integer);
    CHECK(tokens[4].kind == TokenKind::semicolon);
    CHECK(tokens[5].kind == TokenKind::eof);
}

TEST_CASE("Doc comments are preserved as opaque tokens", "[lexer]")
{
    const SourceFile source_file{"/**\n"
                                 "This is a documentation comment\n"
                                 "*/\n"
                                 "var x = 5;"};
    auto tokens = lex_all(source_file);
    REQUIRE(tokens.size() == 7);
    CHECK(tokens[0].kind == TokenKind::doc_comment);
    CHECK(tokens[1].kind == TokenKind::kw_var);
    CHECK(tokens[2].kind == TokenKind::identifier);
    CHECK(tokens[3].kind == TokenKind::equal);
    CHECK(tokens[4].kind == TokenKind::integer);
    CHECK(tokens[5].kind == TokenKind::semicolon);
    CHECK(tokens[6].kind == TokenKind::eof);
}
