/**
 * @file syntax_parser.cpp
 * @author Francesco Corso
 * @date 7/24/2026
 * @brief
 */
module prism.core:parser.syntax_parser.impl;

import :parser.syntax_parser;
import :diagnostics.syntax_info;
import :syntax.green.view;
import :syntax.green.structured_trivia;
import :syntax.green.last_token_replacer;
import :diagnostics.syntax_info;

namespace prism
{
    bool SyntaxParser::at_end()
    {
        return stream_.at_end();
    }

    const GreenToken &SyntaxParser::peek_token(const int offset)
    {
        return stream_.peek(offset);
    }

    GreenPtr<GreenToken> SyntaxParser::consume_token()
    {
        return stream_.consume().shared_from_this();
    }

    Optional<GreenPtr<GreenToken>> SyntaxParser::match_token(const SyntaxKind kind)
    {
        if (auto &token = peek_token(); token.kind() == kind)
        {
            stream_.advance();
            return token.shared_from_this();
        }

        return std::nullopt;
    }

    GreenPtr<GreenToken> SyntaxParser::expect_token(const SyntaxKind kind)
    {
        auto &next = peek_token();
        if (next.kind() == kind)
        {
            stream_.advance();
            return next.shared_from_this();
        }

        auto token = GreenToken::get_missing(kind)->clone();
        token->add_diagnostic(SyntaxDiagnosticInfo::create<DiagnosticCode::unexpected_token>(next.to_string()));
        return std::move(token);
    }

    void SyntaxParser::add_skipped_syntax(GreenToken &target, const GreenNode &skipped_syntax, const bool trailing)
    {
        GreenListNodeBuilder builder;

        std::int64_t current_offset;
        if (trailing)
        {
            current_offset = target.width() + target.trailing_trivia_width();

            if (const auto trivia = target.trailing_trivia(); trivia.has_value())
                builder.add(trivia->shared_from_this());
        }
        else
        {
            current_offset = -target.leading_trivia_width() - skipped_syntax.full_width();
        }

        std::shared_ptr<const SyntaxDiagnosticInfo> diagnostic;
        std::uint32_t final_diagnostic_offset;

        for (auto &node : skipped_syntax.enumerate_nodes())
        {
            if (auto token = node.as<GreenToken>(); token.has_value())
            {
                if (const auto leading_trivia = token->leading_trivia(); leading_trivia.has_value())
                    builder.add(leading_trivia->shared_from_this());

                if (token->width() > 0)
                {
                    // Strip off the trivia of the token and place it in the token list
                    builder.add(make_ref_counted<const GreenSkippedTokensTrivia>(token->update(nullptr, nullptr)));
                }
                else
                {
                    auto it = token->diagnostics().begin();
                    if (it != token->diagnostics().end())
                    {
                        diagnostic = static_pointer_cast<const SyntaxDiagnosticInfo>(*it);
                        final_diagnostic_offset = current_offset + token->leading_trivia_width() + diagnostic->offset();
                    }
                }

                if (const auto trailing_trivia = token->trailing_trivia(); trailing_trivia.has_value())
                    builder.add(trailing_trivia->shared_from_this());

                current_offset += token->full_width();
            }
            else if (node.contains_diagnostics() && diagnostic == nullptr)
            {
                auto it = node.diagnostics().begin();
                if (it != node.diagnostics().end())
                {
                    diagnostic = static_pointer_cast<const SyntaxDiagnosticInfo>(*it);
                    final_diagnostic_offset = current_offset + node.leading_trivia_width() + diagnostic->offset();
                }
            }
        }

        if (diagnostic != nullptr)
        {
            target.add_diagnostic(std::make_shared<SyntaxDiagnosticInfo>(final_diagnostic_offset,
                                                                         diagnostic->width(),
                                                                         diagnostic->descriptor(),
                                                                         diagnostic->arguments()));
        }

        if (trailing)
        {
            target.set_trailing_trivia(std::move(builder).build());
        }
        else
        {
            builder.add_range(GreenSyntaxList{from_optional_ref(target.leading_trivia())} |
                              std::views::transform([](auto &trivia) { return trivia.shared_from_this(); }));
            target.set_leading_trivia(std::move(builder).build());
        }
    }
} // namespace prism
