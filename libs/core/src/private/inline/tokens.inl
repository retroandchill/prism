#ifndef PRISM_TOKEN
#define PRISM_TOKEN(name, str)
#define PRISM_SKIP_TOKENS
#endif

#ifndef PRISM_KEYWORD
#define PRISM_KEYWORD(name)
#define PRISM_SKIP_KEYWORDS
#endif

#ifndef PRISM_PUNCT
#define PRISM_PUNCT(name, str)
#define PRISM_SKIP_PUNCTS
#endif

PRISM_TOKEN(eof, "EOF")
PRISM_TOKEN(unrecognized, "<unrecognized>")
PRISM_TOKEN(unterminated_block_comment, "unterminated block comment")
PRISM_TOKEN(unterminated_string_literal, "unterminated string literal")
PRISM_TOKEN(doc_comment, "documentation comment")
PRISM_KEYWORD(var)
PRISM_KEYWORD(func)
PRISM_KEYWORD(extern)
PRISM_KEYWORD(return)
PRISM_KEYWORD(mut)

PRISM_KEYWORD(true)
PRISM_KEYWORD(false)

PRISM_KEYWORD(void)
PRISM_KEYWORD(bool)
PRISM_KEYWORD(i8)
PRISM_KEYWORD(i16)
PRISM_KEYWORD(i32)
PRISM_KEYWORD(i64)
PRISM_KEYWORD(i128)
PRISM_KEYWORD(u8)
PRISM_KEYWORD(u16)
PRISM_KEYWORD(u32)
PRISM_KEYWORD(u64)
PRISM_KEYWORD(u128)
PRISM_KEYWORD(isize)
PRISM_KEYWORD(usize)
PRISM_KEYWORD(f16)
PRISM_KEYWORD(f32)
PRISM_KEYWORD(f64)
PRISM_KEYWORD(char)
PRISM_KEYWORD(char16)
PRISM_KEYWORD(rune)
PRISM_KEYWORD(str)

PRISM_PUNCT(lbracket, "[")
PRISM_PUNCT(rbracket, "]")
PRISM_PUNCT(lparen, "(")
PRISM_PUNCT(rparen, ")")
PRISM_PUNCT(lbrace, "{")
PRISM_PUNCT(rbrace, "}")
PRISM_PUNCT(period, ".")
PRISM_PUNCT(ellipsis, "...")
PRISM_PUNCT(amp, "&")
PRISM_PUNCT(amp_amp, "&&")
PRISM_PUNCT(amp_equal, "&=")
PRISM_PUNCT(star, "*")
PRISM_PUNCT(star_equal, "*=")
PRISM_PUNCT(plus, "+")
PRISM_PUNCT(plus_plus, "++")
PRISM_PUNCT(plus_equal, "+=")
PRISM_PUNCT(minus, "-")
PRISM_PUNCT(arrow, "->")
PRISM_PUNCT(minus_minus, "--")
PRISM_PUNCT(minus_equal, "-=")
PRISM_PUNCT(tilde, "~")
PRISM_PUNCT(exclaim, "!")
PRISM_PUNCT(exclaim_equal, "!=")
PRISM_PUNCT(slash, "/")
PRISM_PUNCT(slash_equal, "/=")
PRISM_PUNCT(percent, "%")
PRISM_PUNCT(percent_equal, "%=")
PRISM_PUNCT(less, "<")
PRISM_PUNCT(less_less, "<<")
PRISM_PUNCT(less_equal, "<=")
PRISM_PUNCT(less_less_equal, "<<=")
PRISM_PUNCT(spaceship, "<=>")
PRISM_PUNCT(greater, ">")
PRISM_PUNCT(greater_greater, ">>")
PRISM_PUNCT(greater_greater_greater, ">>>")
PRISM_PUNCT(greater_equal, ">=")
PRISM_PUNCT(greater_greater_equal, ">>=")
PRISM_PUNCT(greater_greater_greater_equal, ">>>=")
PRISM_PUNCT(caret, "^")
PRISM_PUNCT(caret_equal, "^=")
PRISM_PUNCT(pipe, "|")
PRISM_PUNCT(pipe_pipe, "||")
PRISM_PUNCT(pipe_equal, "|=")
PRISM_PUNCT(question, "?")
PRISM_PUNCT(question_question, "??")
PRISM_PUNCT(question_period, "?.")
PRISM_PUNCT(colon, ":")
PRISM_PUNCT(colon_colon, "::")
PRISM_PUNCT(semicolon, ";")
PRISM_PUNCT(equal, "=")
PRISM_PUNCT(big_arrow, "=>")
PRISM_PUNCT(equal_equal, "==")
PRISM_PUNCT(comma, ",")

PRISM_TOKEN(identifier, "identifier")
PRISM_TOKEN(integer, "integer")
PRISM_TOKEN(string_literal, "string literal")

#ifdef PRISM_SKIP_TOKENS
#undef PRISM_TOKEN
#endif
#undef PRISM_SKIP_TOKENS

#ifdef PRISM_SKIP_KEYWORDS
#undef PRISM_KEYWORD
#endif
#undef PRISM_SKIP_KEYWORDS

#ifdef PRISM_SKIP_PUNCTS
#undef PRISM_PUNCT
#endif
#undef PRISM_SKIP_PUNCTS
