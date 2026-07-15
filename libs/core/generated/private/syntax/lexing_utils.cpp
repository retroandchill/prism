module prism.core:syntax.lexing_utils.impl;

import :syntax.lexing_utils;

namespace prism
{
    GreenPtr<GreenToken> get_static_green_token(SyntaxKind kind)
    {
        if (is_keyword(kind))
        {
            static std::array keywords = {
                make_ref_counted<GreenToken>(SyntaxKind::var_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::func_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::extern_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::return_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::mut_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::true_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::false_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::void_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::bool_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::i8_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::i16_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::i32_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::i64_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::i128_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::u8_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::u16_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::u32_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::u64_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::u128_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::isize_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::usize_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::f16_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::f32_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::f64_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::char_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::char16_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::rune_keyword),
                make_ref_counted<GreenToken>(SyntaxKind::str_keyword),
            };

            return keywords[std::to_underlying(kind) - std::to_underlying(SyntaxKind::keyword_start)];
        }

        if (is_punctuation(kind))
        {
            static std::array punctuations = {
                make_ref_counted<GreenToken>(SyntaxKind::lbracket_token),
                make_ref_counted<GreenToken>(SyntaxKind::rbracket_token),
                make_ref_counted<GreenToken>(SyntaxKind::lparen_token),
                make_ref_counted<GreenToken>(SyntaxKind::rparen_token),
                make_ref_counted<GreenToken>(SyntaxKind::lbrace_token),
                make_ref_counted<GreenToken>(SyntaxKind::rbrace_token),
                make_ref_counted<GreenToken>(SyntaxKind::period_token),
                make_ref_counted<GreenToken>(SyntaxKind::ellipsis_token),
                make_ref_counted<GreenToken>(SyntaxKind::amp_token),
                make_ref_counted<GreenToken>(SyntaxKind::amp_amp_token),
                make_ref_counted<GreenToken>(SyntaxKind::amp_equal_token),
                make_ref_counted<GreenToken>(SyntaxKind::star_token),
                make_ref_counted<GreenToken>(SyntaxKind::star_equal_token),
                make_ref_counted<GreenToken>(SyntaxKind::plus_token),
                make_ref_counted<GreenToken>(SyntaxKind::plus_plus_token),
                make_ref_counted<GreenToken>(SyntaxKind::plus_equal_token),
                make_ref_counted<GreenToken>(SyntaxKind::minus_token),
                make_ref_counted<GreenToken>(SyntaxKind::arrow_token),
                make_ref_counted<GreenToken>(SyntaxKind::minus_minus_token),
                make_ref_counted<GreenToken>(SyntaxKind::minus_equal_token),
                make_ref_counted<GreenToken>(SyntaxKind::tilde_token),
                make_ref_counted<GreenToken>(SyntaxKind::exclaim_token),
                make_ref_counted<GreenToken>(SyntaxKind::exclaim_equal_token),
                make_ref_counted<GreenToken>(SyntaxKind::slash_token),
                make_ref_counted<GreenToken>(SyntaxKind::slash_equal_token),
                make_ref_counted<GreenToken>(SyntaxKind::percent_token),
                make_ref_counted<GreenToken>(SyntaxKind::percent_equal_token),
                make_ref_counted<GreenToken>(SyntaxKind::less_token),
                make_ref_counted<GreenToken>(SyntaxKind::less_less_token),
                make_ref_counted<GreenToken>(SyntaxKind::less_equal_token),
                make_ref_counted<GreenToken>(SyntaxKind::less_less_equal_token),
                make_ref_counted<GreenToken>(SyntaxKind::spaceship_token),
                make_ref_counted<GreenToken>(SyntaxKind::greater_token),
                make_ref_counted<GreenToken>(SyntaxKind::greater_greater_token),
                make_ref_counted<GreenToken>(SyntaxKind::greater_greater_greater_token),
                make_ref_counted<GreenToken>(SyntaxKind::greater_equal_token),
                make_ref_counted<GreenToken>(SyntaxKind::greater_greater_equal_token),
                make_ref_counted<GreenToken>(SyntaxKind::greater_greater_greater_equal_token),
                make_ref_counted<GreenToken>(SyntaxKind::caret_token),
                make_ref_counted<GreenToken>(SyntaxKind::caret_equal_token),
                make_ref_counted<GreenToken>(SyntaxKind::pipe_token),
                make_ref_counted<GreenToken>(SyntaxKind::pipe_pipe_token),
                make_ref_counted<GreenToken>(SyntaxKind::pipe_equal_token),
                make_ref_counted<GreenToken>(SyntaxKind::question_token),
                make_ref_counted<GreenToken>(SyntaxKind::question_question_token),
                make_ref_counted<GreenToken>(SyntaxKind::question_period_token),
                make_ref_counted<GreenToken>(SyntaxKind::colon_token),
                make_ref_counted<GreenToken>(SyntaxKind::colon_colon_token),
                make_ref_counted<GreenToken>(SyntaxKind::semicolon_token),
                make_ref_counted<GreenToken>(SyntaxKind::equal_token),
                make_ref_counted<GreenToken>(SyntaxKind::big_arrow_token),
                make_ref_counted<GreenToken>(SyntaxKind::equal_equal_token),
                make_ref_counted<GreenToken>(SyntaxKind::comma_token),
            };

            return punctuations[std::to_underlying(kind) - std::to_underlying(SyntaxKind::punctuation_start)];
        }

        return nullptr;
    }

    Optional<SyntaxKind> match_keyword(std::string_view text)
    {
        switch (text.size())
        {
            case 2:
                if (text == "i8")
                    return SyntaxKind::i8_keyword;
                if (text == "u8")
                    return SyntaxKind::u8_keyword;
                break;
            case 3:
                if (text == "var")
                    return SyntaxKind::var_keyword;
                if (text == "mut")
                    return SyntaxKind::mut_keyword;
                if (text == "i16")
                    return SyntaxKind::i16_keyword;
                if (text == "i32")
                    return SyntaxKind::i32_keyword;
                if (text == "i64")
                    return SyntaxKind::i64_keyword;
                if (text == "u16")
                    return SyntaxKind::u16_keyword;
                if (text == "u32")
                    return SyntaxKind::u32_keyword;
                if (text == "u64")
                    return SyntaxKind::u64_keyword;
                if (text == "f16")
                    return SyntaxKind::f16_keyword;
                if (text == "f32")
                    return SyntaxKind::f32_keyword;
                if (text == "f64")
                    return SyntaxKind::f64_keyword;
                if (text == "str")
                    return SyntaxKind::str_keyword;
                break;
            case 4:
                if (text == "func")
                    return SyntaxKind::func_keyword;
                if (text == "true")
                    return SyntaxKind::true_keyword;
                if (text == "void")
                    return SyntaxKind::void_keyword;
                if (text == "bool")
                    return SyntaxKind::bool_keyword;
                if (text == "i128")
                    return SyntaxKind::i128_keyword;
                if (text == "u128")
                    return SyntaxKind::u128_keyword;
                if (text == "char")
                    return SyntaxKind::char_keyword;
                if (text == "rune")
                    return SyntaxKind::rune_keyword;
                break;
            case 5:
                if (text == "false")
                    return SyntaxKind::false_keyword;
                if (text == "isize")
                    return SyntaxKind::isize_keyword;
                if (text == "usize")
                    return SyntaxKind::usize_keyword;
                break;
            case 6:
                if (text == "extern")
                    return SyntaxKind::extern_keyword;
                if (text == "return")
                    return SyntaxKind::return_keyword;
                if (text == "char16")
                    return SyntaxKind::char16_keyword;
                break;
        }

        return std::nullopt;
    }
} // namespace prism
