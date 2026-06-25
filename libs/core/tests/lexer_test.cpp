/**
 * @file lexer_test.cpp
 * @author Francesco Corso
 * @date 6/20/2026
 * @brief Test for the lexer module.
 */
#include <catch2/catch_test_macros.hpp>

import std;
import antlr.runtime;
import prism.core.generated;
import prism.core.lexer;
import prism.core.lexer.token;
import prism.core.source.source_file;
import prism.core.lexer.defaults;

using namespace prism;
using namespace prism::gen;

TEST_CASE("Simple grammar", "[lexer]")
{
    antlr4::ANTLRInputStream input_stream{"func do_thing(x: i32) {"
                                          "    return x + 1;"
                                          "}"};
    PrismLexer lexer(&input_stream);
    auto all_tokens = lexer.getAllTokens();

    std::vector<std::unique_ptr<antlr4::Token>> tokens;
    tokens.reserve(all_tokens.size());
    for (auto &token : all_tokens)
    {
        if (token->getChannel() == antlr4::Token::DEFAULT_CHANNEL)
        {
            tokens.push_back(std::move(token));
        }
    }

    REQUIRE(tokens.size() == 14);
    CHECK(tokens[0]->getType() == PrismLexer::FUNC);
    CHECK(tokens[1]->getType() == PrismLexer::IDENTIFIER);
    CHECK(tokens[2]->getType() == PrismLexer::LPAREN);
    CHECK(tokens[3]->getType() == PrismLexer::IDENTIFIER);
    CHECK(tokens[4]->getType() == PrismLexer::COLON);
    CHECK(tokens[5]->getType() == PrismLexer::IDENTIFIER);
    CHECK(tokens[6]->getType() == PrismLexer::RPAREN);
    CHECK(tokens[7]->getType() == PrismLexer::LBRACE);
    CHECK(tokens[8]->getType() == PrismLexer::RETURN);
    CHECK(tokens[9]->getType() == PrismLexer::IDENTIFIER);
    CHECK(tokens[10]->getType() == PrismLexer::PLUS);
    CHECK(tokens[11]->getType() == PrismLexer::INT);
    CHECK(tokens[12]->getType() == PrismLexer::SEMICOLON);
    CHECK(tokens[13]->getType() == PrismLexer::RBRACE);
}

TEST_CASE("Skips line comments until the end of the line", "[lexer]")
{
    const SourceFile source_file{"// This is a line comment\n"
                                 "var x = 5; // This is another line comment\n"};
    const auto lexer = default_lexer();
    auto tokens = lexer.lex(source_file);
    REQUIRE(tokens.size() == 6);
    CHECK(tokens[0].kind == TokenKind::kw_var);
    CHECK(tokens[1].kind == TokenKind::identifier);
    CHECK(tokens[2].kind == TokenKind::equal);
    CHECK(tokens[3].kind == TokenKind::number);
    CHECK(tokens[4].kind == TokenKind::semicolon);
    CHECK(tokens[5].kind == TokenKind::eof);
}

TEST_CASE("Block comments can split a single line", "[lexer]")
{
    const SourceFile source_file{"var x = /* Block comment */ 5;"};
    const auto lexer = default_lexer();
    auto tokens = lexer.lex(source_file);
    REQUIRE(tokens.size() == 6);
    CHECK(tokens[0].kind == TokenKind::kw_var);
    CHECK(tokens[1].kind == TokenKind::identifier);
    CHECK(tokens[2].kind == TokenKind::equal);
    CHECK(tokens[3].kind == TokenKind::number);
    CHECK(tokens[4].kind == TokenKind::semicolon);
    CHECK(tokens[5].kind == TokenKind::eof);
}

TEST_CASE("Block comments can span multiple lines", "[lexer]")
{
    const SourceFile source_file{"/*\n"
                                 "This is a multiline block comment\n"
                                 "*/\n"
                                 "var x = 5;"};
    const auto lexer = default_lexer();
    auto tokens = lexer.lex(source_file);
    REQUIRE(tokens.size() == 6);
    CHECK(tokens[0].kind == TokenKind::kw_var);
    CHECK(tokens[1].kind == TokenKind::identifier);
    CHECK(tokens[2].kind == TokenKind::equal);
    CHECK(tokens[3].kind == TokenKind::number);
    CHECK(tokens[4].kind == TokenKind::semicolon);
    CHECK(tokens[5].kind == TokenKind::eof);
}

TEST_CASE("Doc comments are preserved as opaque tokens", "[lexer]")
{
    const SourceFile source_file{"/**\n"
                                 "This is a documentation comment\n"
                                 "*/\n"
                                 "var x = 5;"};
    const auto lexer = default_lexer();
    auto tokens = lexer.lex(source_file);
    REQUIRE(tokens.size() == 7);
    CHECK(tokens[0].kind == TokenKind::doc_comment);
    CHECK(tokens[1].kind == TokenKind::kw_var);
    CHECK(tokens[2].kind == TokenKind::identifier);
    CHECK(tokens[3].kind == TokenKind::equal);
    CHECK(tokens[4].kind == TokenKind::number);
    CHECK(tokens[5].kind == TokenKind::semicolon);
    CHECK(tokens[6].kind == TokenKind::eof);
}
