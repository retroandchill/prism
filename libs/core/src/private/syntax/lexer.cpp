/**
 * @file lexer.cpp
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
module;

#include "prism/core/syntax.hpp"

module prism.core:syntax.lexer.impl;

import :syntax.lexer;
import :syntax.diagnostics;
import uni_algo;
import :memory.buffer_pool;

namespace prism
{
    namespace
    {
        using OpPair = std::pair<std::string_view, SyntaxKind>;

        template <std::size_t N>
        consteval std::array<OpPair, N> sort_operators_by_length(std::array<OpPair, N> operators)
        {
            std::sort(operators.begin(),
                      operators.end(),
                      [](const auto &lhs, const auto &rhs) { return lhs.first.size() > rhs.first.size(); });
            return operators;
        }
    } // namespace

    SyntaxToken Lexer::next()
    {
        const auto start = cursor_.position();
        auto leading_trivia = collect_trivia(false);

        if (cursor_.at_end())
            return make_eof(start, std::move(leading_trivia));

        auto green_token = *match_number(leading_trivia)
                                .or_else([&] { return match_punctuation(leading_trivia); })
                                .or_else([&] { return match_character_literal(leading_trivia); })
                                .or_else([&] { return match_string_literal(leading_trivia); })
                                .or_else([&] { return match_identifier_or_keyword(leading_trivia); })
                                .or_else([&] { return Optional{make_bad_token(leading_trivia)}; });
        return SyntaxToken{std::move(green_token), start};
    }

    GreenPtr<GreenTriviaList> Lexer::collect_trivia(bool stop_after_newline)
    {
        std::vector<GreenPtr<GreenTrivia>> collected;
        while (!cursor_.at_end())
        {
            auto trivia =
                match_whitespace().or_else([&] { return match_new_line(); }).or_else([&] { return match_comment(); });
            if (!trivia.has_value())
                break;

            if (const auto &added = collected.emplace_back(*std::move(trivia));
                stop_after_newline &&
                (added->kind() == SyntaxKind::new_line_trivia || added->kind() == SyntaxKind::line_comment_trivia))
                break;
        }

        return !collected.empty() ? make_ref_counted<const GreenTriviaList>(std::move(collected)) : nullptr;
    }

    Optional<GreenPtr<GreenTrivia>> Lexer::match_whitespace()
    {
        if (cursor_.at_end() || cursor_.any('\n', '\r') || !std::isspace(cursor_.current()))
            return std::nullopt;

        std::string str;
        do
        {
            str.push_back(cursor_.current());
            cursor_.advance();
        } while (!cursor_.at_end() && !cursor_.any('\n', '\r') && std::isspace(cursor_.current()));

        if (str.empty())
            return std::nullopt;

        return make_ref_counted<GreenTrivia>(SyntaxKind::whitespace_trivia, std::move(str));
    }
    Optional<GreenPtr<GreenTrivia>> Lexer::match_new_line()
    {
        const auto view = cursor_.remaining();
        if (view.starts_with("\r\n"))
        {
            cursor_.advance(2);
            return GreenTrivia::carriage_return_line_feed;
        }

        if (view.starts_with('\n'))
        {
            cursor_.advance();
            return GreenTrivia::line_feed;
        }

        if (view.starts_with('\r'))
        {
            cursor_.advance();
            return GreenTrivia::carriage_return;
        }

        return std::nullopt;
    }

    Optional<GreenPtr<GreenTrivia>> Lexer::match_comment()
    {
        if (cursor_.current() != '/')
            return std::nullopt;

        switch (cursor_.peek())
        {
            case '/':
                return handle_line_comment();
            case '*':
                return handle_block_comment();
            default:
                return std::nullopt;
        }
    }

    GreenPtr<GreenTrivia> Lexer::handle_line_comment()
    {
        std::string str = "//";
        cursor_.advance(2);
        while (!cursor_.at_end() && !cursor_.any('\n', '\r'))
        {
            str.push_back(cursor_.current());
            cursor_.advance();
        }

        const auto view = cursor_.remaining();
        if (view.starts_with("\r\n"))
        {
            str.append("\r\n");
            cursor_.advance(2);
        }
        else if (view.starts_with('\r') || view.starts_with('\n'))
        {
            str.push_back(cursor_.current());
            cursor_.advance();
        }

        return make_ref_counted<const GreenTrivia>(SyntaxKind::line_comment_trivia, std::move(str));
    }

    GreenPtr<GreenTrivia> Lexer::handle_block_comment()
    {
        auto start = cursor_.position();
        std::string str = "/*";
        cursor_.advance(2);
        while (!cursor_.at_end())
        {
            str.push_back(cursor_.current());
            if (cursor_.remaining().starts_with("*/"))
            {
                cursor_.advance(2);
                str.append("*/");
                return make_ref_counted<const GreenTrivia>(SyntaxKind::block_comment_trivia, std::move(str));
            }

            cursor_.advance();
        }

        auto ptr = make_ref_counted<GreenTrivia>(SyntaxKind::block_comment_trivia, std::move(str));
        ptr->add_diagnostic(DiagnosticInfo::create(unterminated_block_comment, cursor_.position() - start));
        return std::move(ptr);
    }

    SyntaxToken Lexer::make_eof(const std::uint32_t start, GreenPtr<GreenTriviaList> leading_trivia)
    {
        return SyntaxToken{GreenToken::eof->with_leading_trivia(std::move(leading_trivia)), start};
    }

    GreenPtr<GreenToken> Lexer::make_bad_token(GreenPtr<GreenTriviaList> leading_trivia)
    {
        cursor_.advance();
        return GreenToken::bad_token->with_leading_and_trailing_trivia(std::move(leading_trivia), collect_trivia());
    }

    Optional<GreenPtr<GreenToken>> Lexer::match_number(GreenPtr<GreenTriviaList> leading_trivia)
    {
        auto start = cursor_.position();
        auto remainder = cursor_.remaining();
        bool is_float;
        if (remainder.starts_with("0x") || remainder.starts_with("0X"))
        {
            is_float = false;
            if (!handle_hex_literal())
            {
                // If we fail to parse just grab the leading 0 as the literal
                cursor_.advance();
            }
        }
        else if (remainder.starts_with("0b") || remainder.starts_with("0B"))
        {
            is_float = false;
            if (!handle_binary_literal())
            {
                // If we fail to parse just grab the leading 0 as the literal
                cursor_.advance();
            }
        }
        else
        {
            const auto found_digits = consume_digit_sequence([](const char c) { return std::isdigit(c); });
            if (found_digits && cursor_.at_end())
            {
                const auto literal_text = cursor_.since(start);
                return make_green_value(IntegerLiteralData::parse(literal_text),
                                        std::string{literal_text},
                                        std::move(leading_trivia),
                                        collect_trivia());
            }

            is_float = !cursor_.at_end() && cursor_.current() == '.';
            if (is_float)
            {
                cursor_.advance();
                consume_digit_sequence([](const char c) { return std::isdigit(c); });
            }
            else if (!found_digits)
                return std::nullopt;

            is_float &= consume_float_suffix();
            if (!is_float)
            {
                consume_integer_suffix();
            }
        }

        const auto text = cursor_.since(start);
        if (is_float)
        {
            return make_green_value(FloatLiteralData::parse(text),
                                    std::string{text},
                                    std::move(leading_trivia),
                                    collect_trivia());
        }

        return make_green_value(IntegerLiteralData::parse(text),
                                std::string{text},
                                std::move(leading_trivia),
                                collect_trivia());
    }

    Optional<GreenPtr<GreenToken>> Lexer::match_punctuation(GreenPtr<GreenTriviaList> leading_trivia)
    {
        using namespace std::string_view_literals;
        static constexpr auto punctuations = sort_operators_by_length(std::array{
#define X(name, str) std::make_pair(str##sv, SyntaxKind::name##_token),
            PRISM_SYNTAX_PUNCTUATIONS(X)
#undef X
        });

        const auto view = cursor_.remaining();
        for (auto &[str, kind] : punctuations)
        {
            if (!view.starts_with(str))
                continue;

            cursor_.advance(static_cast<std::uint32_t>(str.size()));
            return GreenToken::from(kind)->with_leading_and_trailing_trivia(std::move(leading_trivia),
                                                                            collect_trivia());
        }

        return std::nullopt;
    }

    Optional<GreenPtr<GreenToken>> Lexer::match_character_literal(GreenPtr<GreenTriviaList> leading_trivia)
    {
        DiagnosticInfoList diagnostics;
        CharacterEncoding encoding;
        auto start = cursor_.position();
        auto remaining = cursor_.remaining();
        std::uint32_t advance;
        if (remaining.starts_with("u'"))
        {
            encoding = CharacterEncoding::utf16;
            advance = 2;
        }
        else if (remaining.starts_with("U'"))
        {
            encoding = CharacterEncoding::utf32;
            advance = 2;
        }
        else if (remaining.starts_with('\''))
        {
            encoding = CharacterEncoding::utf8;
            advance = 1;
        }
        else
        {
            return std::nullopt;
        }
        cursor_.advance(advance);

        auto terminated = false;
        auto character = U'\0';
        if (!cursor_.at_end() && !cursor_.any('\n', '\r'))
        {
            auto current = cursor_.current();
            if (current != '\\')
            {
                character = current;
                cursor_.advance();
            }
            else if (auto escape = parse_escape_sequence(cursor_.remaining().substr(1)))
            {
                character = escape->first;
                cursor_.advance(escape->second + 1);
            }
            else
            {
                character = cursor_.peek();
                if (character != '\0')
                {
                    diagnostics.push_back(
                        DiagnosticInfo::create(unexpected_escape, advance, static_cast<std::uint32_t>(character)));
                    cursor_.advance();
                }
                else
                {
                    using namespace std::string_view_literals;
                    diagnostics.push_back(DiagnosticInfo::create(unexpected_escape, advance, ""sv));
                    cursor_.advance(2);
                }
            }

            if (!cursor_.at_end() && cursor_.current() == '\'')
            {
                cursor_.advance();
                terminated = true;
            }
        }

        if (!terminated)
        {
            diagnostics.push_back(DiagnosticInfo::create(unterminated_character_literal, cursor_.position() - start));
        }

        auto ptr = make_green_value(CharacterLiteralData{character, encoding},
                                    std::string{cursor_.since(start)},
                                    std::move(leading_trivia),
                                    collect_trivia());
        ptr->set_diagnostics(std::move(diagnostics));
        return std::move(ptr);
    }

    Optional<GreenPtr<GreenToken>> Lexer::match_string_literal(GreenPtr<GreenTriviaList> leading_trivia)
    {
        if (cursor_.current() != '"')
            return std::nullopt;

        const auto start = cursor_.position();
        std::string str;
        DiagnosticInfoList diagnostics;
        auto terminated = false;
        while (!cursor_.at_end())
        {
            cursor_.advance();

            auto current = cursor_.current();
            if (current == '"')
            {
                cursor_.advance();
                terminated = true;
                break;
            }

            if (current == '\r' || current == '\n')
                break;

            if (current != '\\')
            {
                str.push_back(current);
                continue;
            }

            const auto peeked = cursor_.remaining().substr(1);
            if (auto escaped_char = parse_escape_sequence(peeked); escaped_char.has_value())
            {
                auto [c, length] = escaped_char.value();
                // TODO: We need to actually extract the codepoints from this
                str.push_back(static_cast<char>(c));
                cursor_.advance(length + 1);
            }
            else
            {
                auto next = cursor_.peek();
                str.push_back('\\');
                if (next != '\0')
                {
                    str.push_back(next);
                    diagnostics.push_back(DiagnosticInfo::create(unexpected_escape, cursor_.position() - start, next));
                }
                else
                {
                    using namespace std::string_view_literals;
                    diagnostics.push_back(DiagnosticInfo::create(unexpected_escape, cursor_.position() - start, ""sv));
                }
            }
        }

        if (!terminated)
        {
            diagnostics.push_back(DiagnosticInfo::create(unterminated_string_literal, cursor_.position() - start));
        }

        const auto slice = cursor_.since(start);
        auto literal = make_green_value(StringLiteralData{std::move(str)},
                                        std::string{slice},
                                        std::move(leading_trivia),
                                        collect_trivia());
        literal->set_diagnostics(std::move(diagnostics));
        return std::move(literal);
    }

    Optional<GreenPtr<GreenToken>> Lexer::match_identifier_or_keyword(GreenPtr<GreenTriviaList> leading_trivia)
    {
        auto view = cursor_.remaining();
        auto start = cursor_.position();

        std::uint32_t first_skip;
        char first_char;
        bool is_escaped;
        if (cursor_.current() == '@')
        {
            first_skip = 2;
            first_char = cursor_.peek();
            is_escaped = true;
        }
        else
        {
            first_skip = 1;
            first_char = cursor_.current();
            is_escaped = false;
        }

        if (!std::isalpha(first_char) && first_char != '_')
            return std::nullopt;

        cursor_.advance(first_skip);
        while (!cursor_.at_end() && (std::isalnum(cursor_.current()) || cursor_.current() == '_'))
            cursor_.advance();

        const auto length = cursor_.position() - start;
        const auto identifier = view.substr(0, length);

        using namespace std::string_view_literals;
        static constexpr std::array keywords = {
#define X(name) std::make_pair(SyntaxKind::name##_keyword, #name##sv),
            PRISM_SYNTAX_KEYWORDS(X)
#undef X
        };
        for (auto [kind, keyword] : keywords)
        {
            if (identifier == keyword)
                return GreenToken::from(kind)->with_leading_and_trailing_trivia(std::move(leading_trivia),
                                                                                collect_trivia());
        }

        return make_green_value(IdentifierData{identifier, is_escaped}, std::move(leading_trivia), collect_trivia());
    }
    bool Lexer::handle_hex_literal()
    {
        if (!is_hex_digit(cursor_.peek(3)))
            return false;

        cursor_.advance(2);

        consume_digit_sequence([](const char c) { return is_hex_digit(c); });
        return true;
    }

    bool Lexer::handle_binary_literal()
    {
        if (!is_binary_digit(cursor_.peek(3)))
            return false;

        cursor_.advance(2);

        consume_digit_sequence([](const char c) { return is_binary_digit(c); });
        return true;
    }

    void Lexer::consume_integer_suffix()
    {
        using namespace std::literals;
        if (cursor_.at_end() || !cursor_.any('u', 'i'))
            return;

        auto remaining = cursor_.remaining();
        static constexpr std::array bit_sizes = {"8"sv, "16"sv, "32"sv, "64"sv, "128"sv, "z"sv};
        if (const auto it =
                std::ranges::find_if(bit_sizes, [&](const auto &suffix) { return remaining.starts_with(suffix); });
            it != bit_sizes.end())
            cursor_.advance(it->size() + 1);
    }

    bool Lexer::consume_float_suffix()
    {
        using namespace std::literals;
        if (cursor_.at_end() || cursor_.remaining().starts_with('f'))
            return false;

        auto remaining = cursor_.remaining();
        static constexpr std::array bit_sizes = {"16"sv, "32"sv, "64"sv};
        if (const auto it =
                std::ranges::find_if(bit_sizes, [&](const auto &suffix) { return remaining.starts_with(suffix); });
            it != bit_sizes.end())
            cursor_.advance(it->size() + 1);

        return true;
    }

    template <std::predicate<char> Predicate>
    bool Lexer::consume_digit_sequence(Predicate predicate)
    {
        if (cursor_.at_end() || !std::invoke(predicate, cursor_.current()))
            return false;

        cursor_.advance();
        while (!cursor_.at_end())
        {
            auto current = cursor_.current();
            if (current == '_' && std::invoke(predicate, cursor_.peek()))
            {
                cursor_.advance(2);
            }
            else if (std::invoke(predicate, current))
            {
                cursor_.advance();
            }
            else
            {
                break;
            }
        }

        return true;
    }

    Optional<std::pair<char32_t, std::uint32_t>> Lexer::parse_escape_sequence(std::string_view view)
    {
        if (view.empty())
            return std::nullopt;

        switch (view[0])
        {
            case 'n':
                return std::make_pair(U'\n', 1);
            case 'r':
                return std::make_pair(U'\r', 1);
            case 't':
                return std::make_pair(U'\t', 1);
            case '\\':
                return std::make_pair(U'\\', 1);
            case 'b':
                return std::make_pair(U'\b', 1);
            case 'a':
                return std::make_pair(U'\a', 1);
            case 'v':
                return std::make_pair(U'\v', 1);
            case '\'':
                return std::make_pair(U'\'', 1);
            case '"':
                return std::make_pair(U'"', 1);
            case '0':
                return std::make_pair(U'\0', 1);
            default:
                return std::nullopt;
        }
    }

} // namespace prism
