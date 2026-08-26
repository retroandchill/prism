/**
 * @file lexer.cpp
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
module prism.core:parser.lexer.impl;

import :parser.lexer;
import uni_algo;
import :memory.buffer_pool;
import :syntax.lexing_utils;
import :diagnostics.syntax_info;
import :diagnostics.factories;

namespace prism
{
    GreenPtr<GreenToken> Lexer::next()
    {
        auto leading_trivia = collect_trivia(false);

        if (cursor_.at_end())
            return make_eof(std::move(leading_trivia));

        return *match_number(leading_trivia)
                    .or_else([&] { return match_punctuation(leading_trivia); })
                    .or_else([&] { return match_character_literal(leading_trivia); })
                    .or_else([&] { return match_string_literal(leading_trivia); })
                    .or_else([&] { return match_identifier_or_keyword(leading_trivia); })
                    .or_else([&] { return Optional{make_bad_token(leading_trivia)}; });
    }

    GreenTriviaList Lexer::collect_trivia(const bool stop_after_newline)
    {
        GreenListBuilder<GreenTrivia> builder;
        while (!cursor_.at_end())
        {
            auto trivia =
                match_whitespace().or_else([&] { return match_new_line(); }).or_else([&] { return match_comment(); });
            if (!trivia.has_value())
                break;

            if (const auto &added = builder.add(*std::move(trivia));
                stop_after_newline &&
                (added.kind() == SyntaxKind::new_line_trivia || added.kind() == SyntaxKind::line_comment_trivia))
                break;
        }

        return std::move(builder).build();
    }

    Optional<GreenPtr<GreenTrivia>> Lexer::match_whitespace()
    {
        if (cursor_.at_end() || cursor_.any('\n', '\r') || !std::isspace(cursor_.current()))
            return std::nullopt;

        PooledString str;
        do
        {
            str.push_back(cursor_.current());
            cursor_.advance();
        } while (!cursor_.at_end() && !cursor_.any('\n', '\r') && std::isspace(cursor_.current()));

        if (str.empty())
            return std::nullopt;

        return make_ref_counted<GreenTrivia>(SyntaxKind::whitespace_trivia, ImmutableString{str});
    }
    Optional<GreenPtr<GreenTrivia>> Lexer::match_new_line()
    {
        const auto view = cursor_.remaining();
        if (view.starts_with("\r\n"))
        {
            cursor_.advance(2);
            return GreenTrivia::carriage_return_line_feed();
        }

        if (view.starts_with('\n'))
        {
            cursor_.advance();
            return GreenTrivia::line_feed();
        }

        if (view.starts_with('\r'))
        {
            cursor_.advance();
            return GreenTrivia::carriage_return();
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
        PooledString str = "//";
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

        return make_ref_counted<const GreenTrivia>(SyntaxKind::line_comment_trivia, ImmutableString{str});
    }

    GreenPtr<GreenTrivia> Lexer::handle_block_comment()
    {
        const auto start = cursor_.position();
        PooledString str = "/*";
        cursor_.advance(2);
        while (!cursor_.at_end())
        {
            str.push_back(cursor_.current());
            if (cursor_.remaining().starts_with("*/"))
            {
                cursor_.advance(2);
                str.append("*/");
                return make_ref_counted<const GreenTrivia>(SyntaxKind::block_comment_trivia, ImmutableString{str});
            }

            cursor_.advance();
        }

        auto ptr = make_ref_counted<GreenTrivia>(SyntaxKind::block_comment_trivia, ImmutableString{str});
        ptr->add_diagnostic(
            SyntaxDiagnosticInfo{diagnostics::info::make_unterminated_block_comment(), cursor_.position() - start});
        return std::move(ptr);
    }

    GreenPtr<GreenToken> Lexer::make_eof(GreenTriviaList leading_trivia)
    {
        return GreenToken::eof()->with_leading_trivia(std::move(leading_trivia).node());
    }

    GreenPtr<GreenToken> Lexer::make_bad_token(GreenTriviaList leading_trivia)
    {
        cursor_.advance();
        return GreenToken::bad_token()->update(std::move(leading_trivia).node(), collect_trivia().node());
    }

    Optional<GreenPtr<GreenToken>> Lexer::match_number(GreenTriviaList leading_trivia)
    {
        auto start = cursor_.position();
        auto remainder = cursor_.remaining();
        BigInteger value;
        bool is_float = false;
        std::int32_t exponent = 0;
        auto base = IntegerBase::decimal;
        if (remainder.starts_with("0x") || remainder.starts_with("0X"))
        {
            if (!handle_hex_literal(value))
            {
                // If we fail to parse just grab the leading 0 as the literal
                cursor_.advance();
            }

            base = IntegerBase::hex;
        }
        else if (remainder.starts_with("0b") || remainder.starts_with("0B"))
        {
            if (!handle_binary_literal(value))
            {
                // If we fail to parse just grab the leading 0 as the literal
                cursor_.advance();
            }

            base = IntegerBase::binary;
        }
        else
        {
            const auto found_digits = consume_digit_sequence([](const char c) { return std::isdigit(c); },
                                                             [&value](const char c)
                                                             {
                                                                 const auto digit = c - '0';
                                                                 value = value * 10 + digit;
                                                             });
            if (found_digits && cursor_.at_end())
            {
                const auto literal_text = cursor_.since(start);
                return make_green_value(
                    IntegerLiteralData{
                        .value = std::move(value),
                        .base = base,
                    },
                    ImmutableString{literal_text},
                    std::move(leading_trivia).node(),
                    collect_trivia().node());
            }

            if (!cursor_.at_end() && cursor_.current() == '.')
            {
                cursor_.advance();
                is_float = true;
                if (!consume_digit_sequence([](const char c) { return std::isdigit(c); },
                                            [&exponent, &value](const char c)
                                            {
                                                const auto digit = c - '0';
                                                value = value * 10 + digit;
                                                exponent--;
                                            }))
                    return std::nullopt;
            }
            else if (!found_digits)
                return std::nullopt;
        }

        if (is_float)
        {
            const auto suffix = consume_float_suffix();
            const auto text = cursor_.since(start);
            return make_green_value(
                FloatLiteralData{
                    .significand = std::move(value),
                    .exponent10 = exponent,
                    .suffix = suffix,
                },
                ImmutableString{text},
                std::move(leading_trivia).node(),
                collect_trivia().node());
        }

        if (base == IntegerBase::decimal)
        {
            if (const auto suffix = consume_float_suffix(); suffix != FloatSuffix::none)
            {
                const auto text = cursor_.since(start);
                return make_green_value(
                    FloatLiteralData{
                        .significand = std::move(value),
                        .suffix = suffix,
                    },
                    ImmutableString{text},
                    std::move(leading_trivia).node(),
                    collect_trivia().node());
            }
        }

        const auto suffix = consume_integer_suffix();
        const auto text = cursor_.since(start);

        return make_green_value(
            IntegerLiteralData{
                .value = std::move(value),
                .base = base,
                .suffix = suffix,
            },
            ImmutableString{text},
            std::move(leading_trivia).node(),
            collect_trivia().node());
    }

    Optional<GreenPtr<GreenToken>> Lexer::match_punctuation(GreenTriviaList leading_trivia)
    {
        return prism::match_punctuation(cursor_).transform(
            [&](const SyntaxKind kind)
            { return GreenToken::from(kind)->update(std::move(leading_trivia).node(), collect_trivia().node()); });
    }

    Optional<GreenPtr<GreenToken>> Lexer::match_character_literal(GreenTriviaList leading_trivia)
    {
        std::vector<SyntaxDiagnosticInfo> diagnostics;
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
                    diagnostics.push_back(SyntaxDiagnosticInfo{
                        diagnostics::info::make_unexpected_escape(una::utf32to8(std::u32string_view{&character, 1})),
                        cursor_.position() - start,
                        1});
                    cursor_.advance();
                }
                else
                {
                    using namespace std::string_view_literals;
                    diagnostics.push_back(SyntaxDiagnosticInfo{diagnostics::info::make_unexpected_escape(""),
                                                               cursor_.position() - start,
                                                               1});
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
            diagnostics.push_back(SyntaxDiagnosticInfo{diagnostics::info::make_unterminated_character_literal(),
                                                       cursor_.position() - start,
                                                       0});
        }

        auto ptr = make_green_value(CharacterLiteralData{character, encoding},
                                    ImmutableString{cursor_.since(start)},
                                    std::move(leading_trivia).node(),
                                    collect_trivia().node());
        ptr->set_diagnostics(make_immutable_array(diagnostics));
        return std::move(ptr);
    }

    Optional<GreenPtr<GreenToken>> Lexer::match_string_literal(GreenTriviaList leading_trivia)
    {
        if (cursor_.current() != '"')
            return std::nullopt;

        const auto start = cursor_.position();
        PooledString str;
        std::vector<SyntaxDiagnosticInfo> diagnostics;
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
                    diagnostics.push_back(
                        SyntaxDiagnosticInfo{diagnostics::info::make_unexpected_escape(std::string{next}),
                                             cursor_.position() - start,
                                             1});
                }
                else
                {
                    using namespace std::string_view_literals;
                    diagnostics.push_back(SyntaxDiagnosticInfo{diagnostics::info::make_unexpected_escape(""),
                                                               cursor_.position() - start,
                                                               1});
                }
            }
        }

        if (!terminated)
        {
            diagnostics.push_back(SyntaxDiagnosticInfo{diagnostics::info::make_unterminated_string_literal(),
                                                       cursor_.position() - start,
                                                       0});
        }

        const auto slice = cursor_.since(start);
        auto literal = make_green_value(StringLiteralData{.value = ImmutableString{str}},
                                        ImmutableString{slice},
                                        std::move(leading_trivia).node(),
                                        collect_trivia().node());
        literal->set_diagnostics(make_immutable_array(diagnostics));
        return std::move(literal);
    }

    Optional<GreenPtr<GreenToken>> Lexer::match_identifier_or_keyword(GreenTriviaList leading_trivia)
    {
        const auto view = cursor_.remaining();
        const auto start = cursor_.position();

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

        // Escaped identifiers are never keywords
        if (!identifier.starts_with('@'))
        {
            if (auto keyword = match_keyword(identifier); keyword.has_value())
                return GreenToken::from(*keyword)->update(std::move(leading_trivia).node(), collect_trivia().node());
        }

        return make_green_value(IdentifierData{.name = identifier, .is_escaped = is_escaped},
                                std::move(leading_trivia).node(),
                                collect_trivia().node());
    }
    bool Lexer::handle_hex_literal(BigInteger &value)
    {
        if (!is_hex_digit(cursor_.peek(3)))
            return false;

        cursor_.advance(2);

        consume_digit_sequence([](const char c) { return is_hex_digit(c); },
                               [&value](const char c)
                               {
                                   const auto hex_digit = hex_digit_value(c);
                                   value = value * 16 + hex_digit;
                               });
        return true;
    }

    bool Lexer::handle_binary_literal(BigInteger &value)
    {
        if (!is_binary_digit(cursor_.peek(3)))
            return false;

        cursor_.advance(2);

        consume_digit_sequence([](const char c) { return is_binary_digit(c); },
                               [&value](const char c)
                               {
                                   const std::uint32_t digit = c == '1' ? 1 : 0;
                                   value = value * 2 + digit;
                               });
        return true;
    }

    IntegerSuffix Lexer::consume_integer_suffix()
    {
        using namespace std::literals;
        static constexpr std::array integer_suffixes = {
            std::make_pair("i8"sv, IntegerSuffix::i8),
            std::make_pair("i16"sv, IntegerSuffix::i16),
            std::make_pair("i32"sv, IntegerSuffix::i32),
            std::make_pair("i64"sv, IntegerSuffix::i64),
            std::make_pair("i128"sv, IntegerSuffix::i128),
            std::make_pair("u8"sv, IntegerSuffix::u8),
            std::make_pair("u16"sv, IntegerSuffix::u16),
            std::make_pair("u32"sv, IntegerSuffix::u32),
            std::make_pair("u64"sv, IntegerSuffix::u64),
            std::make_pair("u128"sv, IntegerSuffix::u128),
            std::make_pair("iz"sv, IntegerSuffix::iz),
            std::make_pair("uz"sv, IntegerSuffix::uz),
        };

        auto remaining = cursor_.remaining();
        for (auto [str, type] : integer_suffixes)
        {
            if (remaining.starts_with(str))
            {
                cursor_.advance(str.size());
                return type;
            }
        }

        return IntegerSuffix::none;
    }

    FloatSuffix Lexer::consume_float_suffix()
    {
        using namespace std::literals;
        static constexpr std::array float_suffixes = {
            std::make_pair("f32"sv, FloatSuffix::f32),
            std::make_pair("f64"sv, FloatSuffix::f64),
        };

        auto remaining = cursor_.remaining();
        for (auto [str, type] : float_suffixes)
        {
            if (remaining.starts_with(str))
            {
                cursor_.advance(str.size());
                return type;
            }
        }

        return FloatSuffix::none;
    }

    template <std::predicate<char> Predicate, std::invocable<char> OnDigit>
    bool Lexer::consume_digit_sequence(Predicate predicate, OnDigit on_digit)
    {
        if (cursor_.at_end() || !std::invoke(predicate, cursor_.current()))
            return false;

        const auto look_ahead_for_digits = [&predicate, this] -> Optional<std::uint32_t>
        {
            std::uint32_t lookahead = 1;
            while (true)
            {
                if (const auto c = cursor_.peek(lookahead); c == '_')
                {
                    lookahead++;
                }
                else if (std::invoke(predicate, c))
                {
                    return lookahead;
                }
                else
                {
                    return std::nullopt;
                }
            }
        };

        while (!cursor_.at_end())
        {
            auto current = cursor_.current();
            if (current == '_')
            {
                auto underscores = look_ahead_for_digits();
                if (underscores.has_value())
                {
                    cursor_.advance(*underscores);
                    current = cursor_.current();
                }
                else
                {
                    break;
                }
            }

            if (std::invoke(predicate, current))
            {
                cursor_.advance();
                std::invoke(on_digit, current);
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
