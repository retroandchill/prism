/**
 * @file syntax.hpp
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
#pragma once

#include <boost/preprocessor.hpp>

#define PRISM_SYNTAX_TRIVIA(X)                                                                                         \
    X(whitespace, "<whitespace>")                                                                                      \
    X(new_line, "<new line>")                                                                                          \
    X(line_comment, "<line comment>")                                                                                  \
    X(block_comment, "<block comment>")                                                                                \
    X(skipped_text, "<skipped text>")

#define PRISM_SYNTAX_KEYWORDS(X)                                                                                       \
    X(var)                                                                                                             \
    X(func)                                                                                                            \
    X(extern)                                                                                                          \
    X(return)                                                                                                          \
    X(mut)                                                                                                             \
    X(true)                                                                                                            \
    X(false)                                                                                                           \
    X(void)                                                                                                            \
    X(bool)                                                                                                            \
    X(i8)                                                                                                              \
    X(i16)                                                                                                             \
    X(i32)                                                                                                             \
    X(i64)                                                                                                             \
    X(i128)                                                                                                            \
    X(u8)                                                                                                              \
    X(u16)                                                                                                             \
    X(u32)                                                                                                             \
    X(u64)                                                                                                             \
    X(u128)                                                                                                            \
    X(isize)                                                                                                           \
    X(usize)                                                                                                           \
    X(f16)                                                                                                             \
    X(f32)                                                                                                             \
    X(f64)                                                                                                             \
    X(char)                                                                                                            \
    X(char16)                                                                                                          \
    X(rune)                                                                                                            \
    X(str)

#define PRISM_SYNTAX_PUNCTUATIONS(X)                                                                                   \
    X(lbracket, "[")                                                                                                   \
    X(rbracket, "]")                                                                                                   \
    X(lparen, "(")                                                                                                     \
    X(rparen, ")")                                                                                                     \
    X(lbrace, "{")                                                                                                     \
    X(rbrace, "}")                                                                                                     \
    X(period, ".")                                                                                                     \
    X(ellipsis, "...")                                                                                                 \
    X(amp, "&")                                                                                                        \
    X(amp_amp, "&&")                                                                                                   \
    X(amp_equal, "&=")                                                                                                 \
    X(star, "*")                                                                                                       \
    X(star_equal, "*=")                                                                                                \
    X(plus, "+")                                                                                                       \
    X(plus_plus, "++")                                                                                                 \
    X(plus_equal, "+=")                                                                                                \
    X(minus, "-")                                                                                                      \
    X(arrow, "->")                                                                                                     \
    X(minus_minus, "--")                                                                                               \
    X(minus_equal, "-=")                                                                                               \
    X(tilde, "~")                                                                                                      \
    X(exclaim, "!")                                                                                                    \
    X(exclaim_equal, "!=")                                                                                             \
    X(slash, "/")                                                                                                      \
    X(slash_equal, "/=")                                                                                               \
    X(percent, "%")                                                                                                    \
    X(percent_equal, "%=")                                                                                             \
    X(less, "<")                                                                                                       \
    X(less_less, "<<")                                                                                                 \
    X(less_equal, "<=")                                                                                                \
    X(less_less_equal, "<<=")                                                                                          \
    X(spaceship, "<=>")                                                                                                \
    X(greater, ">")                                                                                                    \
    X(greater_greater, ">>")                                                                                           \
    X(greater_greater_greater, ">>>")                                                                                  \
    X(greater_equal, ">=")                                                                                             \
    X(greater_greater_equal, ">>=")                                                                                    \
    X(greater_greater_greater_equal, ">>>=")                                                                           \
    X(caret, "^")                                                                                                      \
    X(caret_equal, "^=")                                                                                               \
    X(pipe, "|")                                                                                                       \
    X(pipe_pipe, "||")                                                                                                 \
    X(pipe_equal, "|=")                                                                                                \
    X(question, "?")                                                                                                   \
    X(question_question, "??")                                                                                         \
    X(question_period, "?.")                                                                                           \
    X(colon, ":")                                                                                                      \
    X(colon_colon, "::")                                                                                               \
    X(semicolon, ";")                                                                                                  \
    X(equal, "=")                                                                                                      \
    X(big_arrow, "=>")                                                                                                 \
    X(equal_equal, "==")                                                                                               \
    X(comma, ",")

#define PRISM_SYNTAX_OTHER_TOKENS(X)                                                                                   \
    X(end_of_file, "EOF")                                                                                              \
    X(bad, "<unrecognized>")                                                                                           \
    X(identifier, "identifier")                                                                                        \
    X(integer, "integer")                                                                                              \
    X(float, "float")                                                                                                  \
    X(character, "character")                                                                                          \
    X(string_literal, "string literal")

#define PRISM_TRIVIA_ELEM(name, str) ((name, trivia, str, trivia))
#define PRISM_KEYWORD_ELEM(name) ((name, keyword, BOOST_PP_STRINGIZE(name), keyword))
#define PRISM_PUNCTUATION_ELEM(name, str) ((name, token, str, punctuation))
#define PRISM_TOKEN_ELEM(name, str) ((name, token, str, token))
#define PRISM_SYNTAX_ELEM(name, str) ((name, token, str, token))

#define PRISM_SYNTAX_KIND_ENTRIES                                                                                      \
    PRISM_SYNTAX_TRIVIA(PRISM_TRIVIA_ELEM)                                                                             \
    PRISM_SYNTAX_KEYWORDS(PRISM_KEYWORD_ELEM)                                                                          \
    PRISM_SYNTAX_PUNCTUATIONS(PRISM_PUNCTUATION_ELEM)                                                                  \
    PRISM_SYNTAX_OTHER_TOKENS(PRISM_TOKEN_ELEM)

#define PRISM_KIND_NAME(elem) BOOST_PP_TUPLE_ELEM(4, 0, elem)
#define PRISM_KIND_SUFFIX(elem) BOOST_PP_TUPLE_ELEM(4, 1, elem)
#define PRISM_KIND_TEXT(elem) BOOST_PP_TUPLE_ELEM(4, 2, elem)
#define PRISM_KIND_CATEGORY(elem) BOOST_PP_TUPLE_ELEM(4, 3, elem)

#define PRISM_KIND_ENUM_NAME(elem) BOOST_PP_CAT(BOOST_PP_CAT(PRISM_KIND_NAME(elem), _), PRISM_KIND_SUFFIX(elem))
