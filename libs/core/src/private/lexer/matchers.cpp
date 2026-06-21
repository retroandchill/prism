/**
 * @file matchers.cpp
 * @author Francesco Corso
 * @date 6/21/2026
 * @brief Contains the implementation of the matcher classes.
 */
module prism.core.lexer.matchers;

namespace prism
{
    using namespace std::literals::string_view_literals;

    namespace
    {
        const std::flat_map<std::string_view, TokenKind> keywords = {{"var", TokenKind::kw_var},
                                                                     {"func", TokenKind::kw_func},
                                                                     {"extern", TokenKind::kw_extern},
                                                                     {"return", TokenKind::kw_return}};

        constexpr std::array raw_operators = {
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
            std::make_pair(">="sv, TokenKind::greater_equal),
            std::make_pair(">>="sv, TokenKind::greater_greater_equal),
            std::make_pair("^"sv, TokenKind::caret),
            std::make_pair("^="sv, TokenKind::caret_equal),
            std::make_pair("|"sv, TokenKind::pipe),
            std::make_pair("||"sv, TokenKind::pipe_pipe),
            std::make_pair("|="sv, TokenKind::pipe_equal),
            std::make_pair("?"sv, TokenKind::question),
            std::make_pair("??"sv, TokenKind::question_question),
            std::make_pair(":"sv, TokenKind::colon),
            std::make_pair("::"sv, TokenKind::colon_colon),
            std::make_pair(";"sv, TokenKind::semicolon),
            std::make_pair("="sv, TokenKind::equal),
            std::make_pair("=="sv, TokenKind::equal_equal),
            std::make_pair(","sv, TokenKind::comma),
        };

        using OpPair = std::pair<std::string_view, TokenKind>;

        template <std::size_t N>
        constexpr std::array<OpPair, N> sort_operators_by_length(std::array<OpPair, N> operators)
        {
            std::sort(operators.begin(),
                      operators.end(),
                      [](const auto &lhs, const auto &rhs) { return lhs.first.size() < rhs.first.size(); });
            return operators;
        }

        constexpr auto sorted_operators = sort_operators_by_length(raw_operators);

        void skip_line_comment(TextCursor &cursor)
        {
            while (!cursor.at_end() && cursor.current() != '\n')
            {
                cursor.advance();
            }
        }

        std::optional<Token> skip_block_comment(TextCursor &cursor)
        {
            auto start = cursor.position();
            while (!cursor.at_end())
            {
                if (cursor.current() == '*' && cursor.peek() == '/')
                {
                    cursor.advance(2);
                    return std::nullopt;
                }
                cursor.advance();
            }

            return make_token(TokenKind::unterminated_block_comment, start, cursor.position());
        }
    } // namespace

    std::optional<Token> CommentMatcher::try_match(TextCursor &cursor) const
    {
        if (cursor.current() == '/')
        {
            if (cursor.peek() == '/')
            {
                skip_line_comment(cursor);
            }
            else if (cursor.peek() == '*')
            {
                // We only return a token if we find a block comment and the lexer reaches the end of the input before
                // encountering a closing comment delimiter.
                return skip_block_comment(cursor);
            }

            cursor.skip_whitespace();
        }

        return std::nullopt;
    }

    std::optional<Token> IdentifierMatcher::try_match(TextCursor &cursor) const
    {
        auto remaining = cursor.remaining();
        auto start = cursor.position();
        if (const auto current = cursor.current(); !std::isalpha(current) && current != '_')
        {
            return std::nullopt;
        }

        cursor.advance();
        while (!cursor.at_end())
        {
            if (const auto current = cursor.current(); !std::isalnum(current) && current != '_')
            {
                break;
            }

            cursor.advance();
        }

        auto identifier = remaining.substr(0, cursor.position() - start);
        if (const auto keyword = keywords.find(identifier); keyword != keywords.end())
        {
            return make_token(keyword->second, start, cursor.position());
        }

        return make_token(TokenKind::identifier, start, cursor.position());
    }

    std::optional<Token> NumberMatcher::try_match(TextCursor &cursor) const
    {
        auto start = cursor.position();
        if (!std::isdigit(cursor.current()))
        {
            return std::nullopt;
        }

        cursor.advance();
        while (!cursor.at_end())
        {
            if (!std::isdigit(cursor.current()))
            {
                break;
            }
        }

        return make_token(TokenKind::number, start, cursor.position());
    }

    std::optional<Token> OperatorMatcher::try_match(TextCursor &cursor) const
    {
        const std::string_view remaining = cursor.remaining();
        for (const auto &[text, kind] : sorted_operators)
        {
            if (remaining.starts_with(text))
            {
                cursor.advance(text.size());
                return make_token(kind, cursor.position() - text.size(), cursor.position());
            }
        }

        return std::nullopt;
    }
} // namespace prism
