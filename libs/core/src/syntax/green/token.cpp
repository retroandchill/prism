/**
 * @file token.cpp
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:syntax.green.token.impl;

import :syntax.green.token;
import :syntax.lexing_utils;

namespace prism
{
    namespace
    {
        template <SyntaxKind Start, SyntaxKind End>
            requires(std::to_underlying(End) > std::to_underlying(Start))
        constexpr auto get_missing_token_list()
        {
            static constexpr std::size_t size = std::to_underlying(End) - std::to_underlying(Start);
            std::array<GreenPtr<GreenToken>, size> result;
            for (std::size_t i = 0; i < size; ++i)
            {
                result[i] = make_ref_counted<GreenMissingToken>(static_cast<SyntaxKind>(std::to_underlying(Start) + i));
            }
            return result;
        }
    } // namespace

    GreenToken::GreenToken(const SyntaxKind kind,
                           GreenPtr<GreenNode> leading_trivia,
                           GreenPtr<GreenNode> trailing_trivia)
        : GreenToken{kind,
                     static_cast<std::uint32_t>(get_name(kind).length()),
                     std::move(leading_trivia),
                     std::move(trailing_trivia)}
    {
        set_flags(SyntaxFlags::not_missing);
        if (leading_trivia_ != nullptr)
            adjust_flags_and_width(*leading_trivia_);
        if (trailing_trivia_ != nullptr)
            adjust_flags_and_width(*trailing_trivia_);
    }

    GreenToken::GreenToken(const SyntaxKind kind,
                           const std::uint32_t width,
                           GreenPtr<GreenNode> leading_trivia,
                           GreenPtr<GreenNode> trailing_trivia)
        : GreenNode{kind, width}, leading_trivia_{std::move(leading_trivia)},
          trailing_trivia_{std::move(trailing_trivia)}
    {
        set_flags(SyntaxFlags::not_missing);
        DEBUG_ASSERT(prism::is_token(kind));
        if (leading_trivia_ != nullptr)
            adjust_flags_and_width(*leading_trivia_);
        if (trailing_trivia_ != nullptr)
            adjust_flags_and_width(*trailing_trivia_);
    }

    const GreenPtr<GreenToken> &GreenToken::eof()
    {
        static auto instance = make_ref_counted<const GreenToken>(SyntaxKind::eof_token);
        return instance;
    }

    const GreenPtr<GreenToken> &GreenToken::bad_token()
    {
        static auto instance = make_ref_counted<const GreenToken>(SyntaxKind::bad_token, 1);
        return instance;
    }

    GreenPtr<GreenToken> GreenToken::from(const SyntaxKind kind)
    {
        switch (kind)
        {
            case SyntaxKind::eof_token:
                return eof();
            case SyntaxKind::bad_token:
                return bad_token();
            default:
                return get_static_green_token(kind);
        }
    }

    const GreenPtr<GreenToken> &GreenToken::get_missing(const SyntaxKind kind)
    {
        using enum SyntaxKind;
        if (is_keyword(kind))
        {
            static const auto keywords = get_missing_token_list<keyword_start, keyword_end>();
            return keywords[std::to_underlying(kind) - std::to_underlying(keyword_start)];
        }

        if (is_punctuation(kind))
        {
            static const auto punctuations = get_missing_token_list<punctuation_start, punctuation_end>();
            return punctuations[std::to_underlying(kind) - std::to_underlying(punctuation_start)];
        }

        ASSUME(is_other_token(kind));
        static const auto other_tokens = get_missing_token_list<other_token_start, other_token_end>();
        return other_tokens[std::to_underlying(kind) - std::to_underlying(other_token_start)];
    }

    std::string_view GreenToken::text() const
    {
        return get_name(kind());
    }

    std::uint32_t GreenToken::leading_trivia_width() const
    {
        return leading_trivia().transform([](const GreenNode &trivia) { return trivia.width(); }).value_or(0);
    }

    std::uint32_t GreenToken::trailing_trivia_width() const
    {
        return trailing_trivia().transform([](const GreenNode &trivia) { return trivia.width(); }).value_or(0);
    }

    Optional<const GreenNode &> GreenToken::get_slot(std::size_t index) const
    {
        UNREACHABLE("We shouldn't be able to get here");
    }

    GreenPtr<GreenToken> GreenToken::with_leading_trivia(GreenPtr<GreenNode> leading_trivia) const
    {
        if (leading_trivia_ == leading_trivia)
            return shared_from_this();

        return clone_with_trivia(std::move(leading_trivia), trailing_trivia_);
    }

    GreenPtr<GreenToken> GreenToken::with_trailing_trivia(GreenPtr<GreenNode> trailing_trivia) const
    {
        if (trailing_trivia_ == trailing_trivia)
            return shared_from_this();

        return clone_with_trivia(leading_trivia_, std::move(trailing_trivia));
    }

    GreenPtr<GreenToken> GreenToken::update(GreenPtr<GreenNode> leading_trivia,
                                            GreenPtr<GreenNode> trailing_trivia) const
    {
        if (leading_trivia_ == leading_trivia && trailing_trivia_ == trailing_trivia)
            return shared_from_this();

        return clone_with_trivia(std::move(leading_trivia), std::move(trailing_trivia));
    }

    void GreenToken::write_to(TextWriter &writer) const
    {
        if (leading_trivia_ != nullptr)
            leading_trivia_->write_to(writer);
        writer.write(text());
        if (trailing_trivia_ != nullptr)
            trailing_trivia_->write_to(writer);
    }

    GreenPtr<GreenToken> GreenToken::clone_with_trivia(GreenPtr<GreenNode> leading_trivia,
                                                       GreenPtr<GreenNode> trailing_trivia) const
    {
        return make_ref_counted<const GreenToken>(kind(),
                                                  width(),
                                                  std::move(leading_trivia),
                                                  std::move(trailing_trivia));
    }

    RefCountPtr<GreenNode> GreenToken::clone_internal() const
    {
        return make_ref_counted<GreenToken>(kind(), width(), leading_trivia_, trailing_trivia_);
    }

    GreenMissingToken::GreenMissingToken(const SyntaxKind kind,
                                         GreenPtr<GreenNode> leading_trivia,
                                         GreenPtr<GreenNode> trailing_trivia)
        : GreenToken{kind, 0, std::move(leading_trivia), std::move(trailing_trivia)}
    {
        clear_flags(SyntaxFlags::not_missing);
    }

    std::string_view GreenMissingToken::text() const
    {
        return "";
    }

    GreenPtr<GreenToken> GreenMissingToken::clone_with_trivia(GreenPtr<GreenNode> leading_trivia,
                                                              GreenPtr<GreenNode> trailing_trivia) const
    {
        return make_ref_counted<GreenMissingToken>(kind(), std::move(leading_trivia), std::move(trailing_trivia));
    }
    RefCountPtr<GreenNode> GreenMissingToken::clone_internal() const
    {
        constexpr auto get_ptr = [](const Optional<const GreenNode &> node) -> GreenPtr<GreenNode>
        {
            if (!node.has_value())
                return nullptr;

            return node->shared_from_this();
        };

        return make_ref_counted<GreenMissingToken>(kind(), get_ptr(leading_trivia()), get_ptr(trailing_trivia()));
    }
} // namespace prism
