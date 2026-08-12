module prism.core:syntax.green.structured_trivia.impl;

import :syntax.lifetime;
import :syntax.green.structured_trivia;
import :syntax.structured_trivia;

namespace prism
{
    GreenSkippedTokensTrivia::GreenSkippedTokensTrivia(GreenSyntaxList<GreenToken> tokens,
                                                       DiagnosticInfoList diagnostics)
        : GreenStructuredTrivia{SyntaxKind::skipped_tokens_trivia, std::move(diagnostics)}, tokens_{std::move(tokens)}
    {
        set_slot_count(1);
        adjust_flags_and_width(tokens_);
    }

    GreenSkippedTokensTrivia::~GreenSkippedTokensTrivia() = default;

    void GreenSkippedTokensTrivia::set_tokens(GreenSyntaxList<GreenToken> value) noexcept
    {
        tokens_ = std::move(value);
    }

    Optional<const GreenNode &> GreenSkippedTokensTrivia::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return tokens_.node();
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenSkippedTokensTrivia::create_red(SyntaxLifetime &lifetime,
                                                                   const SyntaxNode *parent,
                                                                   std::uint32_t position) const
    {
        return lifetime.add<SkippedTokensTriviaSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenSkippedTokensTrivia> GreenSkippedTokensTrivia::with_tokens(
        GreenSyntaxList<GreenToken> tokens) const
    {
        return update(std::move(tokens));
    }

    GreenPtr<GreenSkippedTokensTrivia> GreenSkippedTokensTrivia::update(GreenSyntaxList<GreenToken> tokens) const
    {
        if (tokens == tokens_)
            return shared_from_this();

        return make_ref_counted<const GreenSkippedTokensTrivia>(std::move(tokens));
    }

    RefCountPtr<GreenNode> GreenSkippedTokensTrivia::clone_internal() const
    {
        return make_ref_counted<GreenSkippedTokensTrivia>(tokens_);
    }
} // namespace prism
