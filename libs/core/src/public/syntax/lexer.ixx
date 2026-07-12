/**
 * @file lexer.ixx
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
export module prism.core:syntax.lexer;

import :text.cursor;
import :syntax.token;

namespace prism
{
    class Lexer final
    {
      public:
        explicit constexpr Lexer(const std::string_view text) : cursor_{text}
        {
        }

        GreenPtr<GreenToken> next();

      private:
        GreenPtr<GreenTriviaList> collect_trivia(bool stop_after_newline = true);
        Optional<GreenPtr<GreenTrivia>> match_whitespace();
        Optional<GreenPtr<GreenTrivia>> match_new_line();
        Optional<GreenPtr<GreenTrivia>> match_comment();
        GreenPtr<GreenTrivia> handle_line_comment();
        GreenPtr<GreenTrivia> handle_block_comment();

        static GreenPtr<GreenToken> make_eof(std::uint32_t start, GreenPtr<GreenTriviaList> leading_trivia);
        GreenPtr<GreenToken> make_bad_token(GreenPtr<GreenTriviaList> leading_trivia);
        Optional<GreenPtr<GreenToken>> match_number(GreenPtr<GreenTriviaList> leading_trivia);
        Optional<GreenPtr<GreenToken>> match_punctuation(GreenPtr<GreenTriviaList> leading_trivia);
        Optional<GreenPtr<GreenToken>> match_character_literal(GreenPtr<GreenTriviaList> leading_trivia);
        Optional<GreenPtr<GreenToken>> match_string_literal(GreenPtr<GreenTriviaList> leading_trivia);
        Optional<GreenPtr<GreenToken>> match_identifier_or_keyword(GreenPtr<GreenTriviaList> leading_trivia);

        bool handle_hex_literal();
        bool handle_binary_literal();
        void consume_integer_suffix();
        bool consume_float_suffix();

        template <std::predicate<char> Predicate>
        bool consume_digit_sequence(Predicate predicate);

        static Optional<std::pair<char32_t, std::uint32_t>> parse_escape_sequence(std::string_view view);

        TextCursor cursor_;
    };
} // namespace prism
