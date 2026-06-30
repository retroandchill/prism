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
#define PRISM_KEYWORD(name) {#name, TokenKind::kw_##name},
#include "tokens.inl"
#undef PRISM_KEYWORD
        };

        const auto remaining = cursor_.remaining();
        const auto start = cursor_.position();
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
#define PRISM_PUNCT(name, str) std::make_pair(##str##sv, TokenKind::##name),
#include "tokens.inl"
#undef PRISM_PUNCT
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
