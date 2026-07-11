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

namespace prism
{
    namespace
    {
        using OpPair = std::pair<std::string_view, SyntaxKind>;

        template <std::size_t N>
        constexpr std::array<OpPair, N> sort_operators_by_length(std::array<OpPair, N> operators)
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
        if (cursor_.at_end() || cursor_.any('\n', '\r') || !std::isspace(cursor_.peek()))
            return std::nullopt;

        std::string str;
        do
        {
            str.push_back(cursor_.current());
            cursor_.advance();
        } while (!cursor_.at_end() && !cursor_.any('\n', '\r') && std::isspace(cursor_.peek()));

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
        return GreenToken::bad_token->with_leading_and_trailing_trivia(std::move(leading_trivia), collect_trivia());
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
        return std::nullopt;
    }

    Optional<GreenPtr<GreenToken>> Lexer::match_string_literal(GreenPtr<GreenTriviaList> leading_trivia)
    {
        if (cursor_.current() != '"')
            return std::nullopt;

        auto start = cursor_.position();
        auto remaining = cursor_.remaining();
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

            using namespace std::string_view_literals;
            switch (auto next = cursor_.peek())
            {
                case 'n':
                    str.push_back('\n');
                    cursor_.advance(2);
                    break;
                case 'r':
                    str.push_back('\r');
                    cursor_.advance(2);
                    break;
                case 't':
                    str.push_back('\t');
                    cursor_.advance(2);
                    break;
                case '\\':
                    str.push_back('\\');
                    cursor_.advance(2);
                    break;
                case 'b':
                    str.push_back('\b');
                    cursor_.advance(2);
                    break;
                case 'a':
                    str.push_back('\a');
                    cursor_.advance(2);
                    break;
                case 'v':
                    str.push_back('\v');
                    cursor_.advance(2);
                    break;
                case '\'':
                    str.push_back('\'');
                    cursor_.advance(2);
                    break;
                case '"':
                    str.push_back('"');
                    cursor_.advance(2);
                    break;
                case '\0':
                    str.push_back('\\');
                    diagnostics.emplace_back(
                        DiagnosticInfo::create(unexpected_escape, cursor_.position() - start, ""sv));
                    cursor_.advance(2);
                    break;
                default:
                    str.push_back('\\');
                    str.push_back(next);
                    diagnostics.push_back(DiagnosticInfo::create(unexpected_escape, cursor_.position() - start, next));
                    cursor_.advance(2);
                    break;
            }
        }

        if (!terminated)
        {
            diagnostics.push_back(DiagnosticInfo::create(unterminated_string_literal, cursor_.position() - start));
        }

        const auto slice = remaining.substr(0, cursor_.position() - start);
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

} // namespace prism
