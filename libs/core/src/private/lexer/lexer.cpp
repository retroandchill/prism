/**
 * @file lexer.cpp
 * @author Francesco Corso
 * @date 6/20/2026
 * @brief Implementation of the lexer module.
 */
module prism.core.lexer;

namespace prism
{
    namespace
    {
        constexpr std::array newline_characters = {'\n', '\r'};

        using OpPair = std::pair<std::string_view, TokenKind>;

        template <std::size_t N>
        constexpr std::array<OpPair, N> sort_operators_by_length(std::array<OpPair, N> operators)
        {
            std::sort(operators.begin(),
                      operators.end(),
                      [](const auto &lhs, const auto &rhs) { return lhs.first.size() > rhs.first.size(); });
            return operators;
        }
    } // namespace

    Token Lexer::next()
    {
        skip_whitespace();

        if (cursor_.at_end())
            return make_eof();

        // Comments can
        if (const auto comment = match_comment(); comment.has_value())
        {
            return *comment;
        }

        if (cursor_.at_end())
            return make_eof();

        return *match_identifier_or_keyword()
                    .or_else([this] { return match_number(); })
                    .or_else([this] { return match_operator(); })
                    .or_else(
                        [this]
                        {
                            return std::make_optional(Token{.kind = TokenKind::unrecognized,
                                                            .range = {cursor_.position(), cursor_.position() + 1}});
                        });
    }

    Token Lexer::make_eof() const
    {
        return Token{.kind = TokenKind::eof, .range = {cursor_.position(), cursor_.position()}};
    }

    void Lexer::skip_whitespace()
    {
        while (!cursor_.at_end())
        {
            if (std::isspace(cursor_.current()))
                cursor_.advance();
            else
                break;
        }
    }

    std::optional<Token> Lexer::match_comment()
    {
        if (cursor_.current() == '/')
        {
            if (cursor_.peek() == '/')
            {
                skip_line_comment();
            }
            else if (cursor_.peek() == '*')
            {
                // We only return a token if we find a block comment and the lexer reaches the end of the input before
                // encountering a closing comment delimiter.
                return skip_block_comment();
            }

            skip_whitespace();
        }

        return std::nullopt;
    }

    void Lexer::skip_line_comment()
    {
        while (!cursor_.at_end() && !cursor_.any(newline_characters))
        {
            cursor_.advance();
        }
    }

    std::optional<Token> Lexer::skip_block_comment()
    {
        auto start = cursor_.position();
        cursor_.advance(2);

        auto is_doc_comment = false;
        if (!cursor_.at_end() && cursor_.current() == '*')
        {
            is_doc_comment = true;
            cursor_.advance();
        }

        while (!cursor_.at_end())
        {
            if (cursor_.current() == '*' && cursor_.peek() == '/')
            {
                cursor_.advance(2);

                // Doc comments are not tokenized by the lexer, but instead will be parsed on demand in contexts
                // where they are needed.
                const auto comment = is_doc_comment
                                         ? std::optional{make_token(TokenKind::doc_comment, start, cursor_.position())}
                                         : std::nullopt;
                skip_whitespace();
                return comment;
            }
            cursor_.advance();
        }

        return make_token(TokenKind::unterminated_block_comment, start, cursor_.position());
    }

    std::optional<Token> Lexer::match_identifier_or_keyword()
    {
        static const std::flat_map<std::string_view, TokenKind> keywords = {
            {"var", TokenKind::kw_var},       {"func", TokenKind::kw_func},     {"extern", TokenKind::kw_extern},
            {"return", TokenKind::kw_return}, {"mut", TokenKind::kw_mut},       {"true", TokenKind::kw_true},
            {"false", TokenKind::kw_false},   {"void", TokenKind::kw_void},     {"bool", TokenKind::kw_bool},
            {"i8", TokenKind::kw_i8},         {"i16", TokenKind::kw_i16},       {"i32", TokenKind::kw_i32},
            {"i64", TokenKind::kw_i64},       {"i128", TokenKind::kw_i128},     {"u8", TokenKind::kw_u8},
            {"u16", TokenKind::kw_u16},       {"u32", TokenKind::kw_u32},       {"u64", TokenKind::kw_u64},
            {"u128", TokenKind::kw_u128},     {"isize", TokenKind::kw_isize},   {"usize", TokenKind::kw_usize},
            {"f16", TokenKind::kw_f16},       {"f32", TokenKind::kw_f32},       {"f64", TokenKind::kw_f64},
            {"char", TokenKind::kw_char},     {"char16", TokenKind::kw_char16}, {"rune", TokenKind::kw_rune},
        };

        auto remaining = cursor_.remaining();
        auto start = cursor_.position();
        if (const auto current = cursor_.current(); !std::isalpha(current) && current != '_')
        {
            return std::nullopt;
        }

        cursor_.advance();
        while (!cursor_.at_end())
        {
            if (const auto current = cursor_.current(); !std::isalnum(current) && current != '_')
            {
                break;
            }

            cursor_.advance();
        }

        auto identifier = remaining.substr(0, cursor_.position() - start);
        if (const auto keyword = keywords.find(identifier); keyword != keywords.end())
        {
            return make_token(keyword->second, start, cursor_.position());
        }

        return make_token(TokenKind::identifier, start, cursor_.position());
    }

    std::optional<Token> Lexer::match_number()
    {
        auto start = cursor_.position();
        if (!std::isdigit(cursor_.current()))
        {
            return std::nullopt;
        }

        cursor_.advance();
        while (!cursor_.at_end())
        {
            if (!std::isdigit(cursor_.current()))
            {
                break;
            }
        }

        return make_token(TokenKind::integer, start, cursor_.position());
    }

    std::optional<Token> Lexer::match_operator()
    {
        using namespace std::string_view_literals;
        static constexpr auto operators = sort_operators_by_length(std::array{
            std::make_pair("["sv, TokenKind::lbracket),
            std::make_pair("]"sv, TokenKind::rbracket),
            std::make_pair("("sv, TokenKind::lparen),
            std::make_pair(")"sv, TokenKind::rparen),
            std::make_pair("{"sv, TokenKind::lbrace),
            std::make_pair("}"sv, TokenKind::rbrace),
            std::make_pair("."sv, TokenKind::period),
            std::make_pair("..."sv, TokenKind::ellipsis),
            std::make_pair("&"sv, TokenKind::amp),
            std::make_pair("&&"sv, TokenKind::amp_amp),
            std::make_pair("&="sv, TokenKind::amp_equal),
            std::make_pair("*"sv, TokenKind::star),
            std::make_pair("*="sv, TokenKind::star_equal),
            std::make_pair("+"sv, TokenKind::plus),
            std::make_pair("++"sv, TokenKind::plus_plus),
            std::make_pair("+="sv, TokenKind::plus_equal),
            std::make_pair("-"sv, TokenKind::minus),
            std::make_pair("->"sv, TokenKind::arrow),
            std::make_pair("--"sv, TokenKind::minus_minus),
            std::make_pair("-="sv, TokenKind::minus_equal),
            std::make_pair("~"sv, TokenKind::tilde),
            std::make_pair("!"sv, TokenKind::exclaim),
            std::make_pair("!="sv, TokenKind::exclaim_equal),
            std::make_pair("/"sv, TokenKind::slash),
            std::make_pair("/="sv, TokenKind::slash_equal),
            std::make_pair("%"sv, TokenKind::percent),
            std::make_pair("%="sv, TokenKind::percent_equal),
            std::make_pair("<"sv, TokenKind::less),
            std::make_pair("<<"sv, TokenKind::less_less),
            std::make_pair("<="sv, TokenKind::less_equal),
            std::make_pair("<<="sv, TokenKind::less_less_equal),
            std::make_pair("<=>"sv, TokenKind::spaceship),
            std::make_pair(">"sv, TokenKind::greater),
            std::make_pair(">>"sv, TokenKind::greater_greater),
            std::make_pair(">>>"sv, TokenKind::greater_greater_greater),
            std::make_pair(">="sv, TokenKind::greater_equal),
            std::make_pair(">>="sv, TokenKind::greater_greater_equal),
            std::make_pair(">>>="sv, TokenKind::greater_greater_greater_equal),
            std::make_pair("^"sv, TokenKind::caret),
            std::make_pair("^="sv, TokenKind::caret_equal),
            std::make_pair("|"sv, TokenKind::pipe),
            std::make_pair("||"sv, TokenKind::pipe_pipe),
            std::make_pair("|="sv, TokenKind::pipe_equal),
            std::make_pair("?"sv, TokenKind::question),
            std::make_pair("??"sv, TokenKind::question_question),
            std::make_pair("?."sv, TokenKind::question_period),
            std::make_pair(":"sv, TokenKind::colon),
            std::make_pair("::"sv, TokenKind::colon_colon),
            std::make_pair(";"sv, TokenKind::semicolon),
            std::make_pair("="sv, TokenKind::equal),
            std::make_pair("=>"sv, TokenKind::big_arrow),
            std::make_pair("=="sv, TokenKind::equal_equal),
            std::make_pair(","sv, TokenKind::comma),
        });

        const std::string_view remaining = cursor_.remaining();
        for (const auto &[text, kind] : operators)
        {
            if (remaining.starts_with(text))
            {
                cursor_.advance(text.size());
                return make_token(kind, cursor_.position() - text.size(), cursor_.position());
            }
        }

        return std::nullopt;
    }

    std::optional<Token> Lexer::match_string_literal()
    {
        if (cursor_.current() != '"')
        {
            return std::nullopt;
        }

        const auto start = cursor_.position();
        while (!cursor_.at_end())
        {
            cursor_.advance();

            if (cursor_.current() == '"')
            {
                cursor_.advance();
                return make_token(TokenKind::string_literal, start, cursor_.position());
            }

            if (cursor_.current() == '\\')
            {
                cursor_.advance();
            }

            // String literals can't run off the end of the line
            if (cursor_.any(newline_characters))
            {
                return make_token(TokenKind::unterminated_string_literal, start, cursor_.position());
            }
        }

        return make_token(TokenKind::unterminated_string_literal, start, cursor_.position());
    }
} // namespace prism
