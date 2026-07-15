export module prism.core:syntax.lexing_utils;

import :syntax.green.token;

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
} // namespace prism
