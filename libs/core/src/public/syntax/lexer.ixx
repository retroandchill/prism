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
        GreenTriviaList collect_trivia(bool stop_after_newline = true);
        Optional<GreenPtr<GreenTrivia>> match_whitespace();
        Optional<GreenPtr<GreenTrivia>> match_new_line();
        Optional<GreenPtr<GreenTrivia>> match_comment();
        GreenPtr<GreenTrivia> handle_line_comment();
        GreenPtr<GreenTrivia> handle_block_comment();

        static GreenPtr<GreenToken> make_eof(GreenTriviaList leading_trivia);
        GreenPtr<GreenToken> make_bad_token(GreenTriviaList leading_trivia);
        Optional<GreenPtr<GreenToken>> match_number(GreenTriviaList leading_trivia);
        Optional<GreenPtr<GreenToken>> match_punctuation(GreenTriviaList leading_trivia);
        Optional<GreenPtr<GreenToken>> match_character_literal(GreenTriviaList leading_trivia);
        Optional<GreenPtr<GreenToken>> match_string_literal(GreenTriviaList leading_trivia);
        Optional<GreenPtr<GreenToken>> match_identifier_or_keyword(GreenTriviaList leading_trivia);

        bool handle_hex_literal(BigDecimal &value);
        bool handle_binary_literal(BigDecimal &value);
        NumericSuffix consume_numeric_suffix(NumericLiteralKind kind);

        template <std::predicate<char> Predicate, std::invocable<char> OnDigit>
        bool consume_digit_sequence(Predicate predicate, OnDigit on_digit);

        static Optional<std::pair<char32_t, std::uint32_t>> parse_escape_sequence(std::string_view view);

        TextCursor cursor_;
    };
} // namespace prism
